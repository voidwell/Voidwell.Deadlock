using Voidwell.Deadlock.Collections;
using Voidwell.Deadlock.Collections.Collection;
using Voidwell.Deadlock.Collections.Models;
using Voidwell.Deadlock.Data.Models;
using Xunit;

namespace Voidwell.Deadlock.CollectionStore.Test;

public class PatchCollectionTest
{
    private sealed class FakeClient : IAssetsClient
    {
        private readonly List<AssetPatchModel> _feed;
        private readonly List<string> _bigDays;

        public FakeClient(List<AssetPatchModel> feed, List<string> bigDays)
        {
            _feed = feed;
            _bigDays = bigDays;
        }

        public Task<IEnumerable<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            object list = path switch
            {
                "v2/patches" => _feed,
                "v1/patches/big-days" => _bigDays,
                _ => throw new InvalidOperationException($"Unexpected path {path}")
            };

            return Task.FromResult((IEnumerable<T>)list);
        }

        public IAsyncEnumerable<T> StreamLinesAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private static AssetPatchModel Entry(string source, string title, DateTime published)
    {
        return new AssetPatchModel
        {
            Source = source,
            Title = title,
            Link = $"https://example.com/{title}",
            PubDate = published,
            Guid = new AssetPatchGuid { Text = $"https://example.com/{title}" }
        };
    }

    [Fact]
    public async Task GetCollectionAsync_MarksFeedEntriesOnABigDay()
    {
        var feed = new List<AssetPatchModel>
        {
            Entry("forum", "major", new DateTime(2026, 9, 29, 20, 25, 11, DateTimeKind.Utc)),
            Entry("steam", "minor", new DateTime(2026, 10, 5, 3, 0, 0, DateTimeKind.Utc))
        };
        var collection = new PatchCollection(new FakeClient(feed, ["2026-09-29T21:00:00Z"]));

        var patches = (await collection.GetCollectionAsync()).ToList();

        Assert.Equal(2, patches.Count);
        Assert.True(patches.Single(a => a.Title == "major").IsBigDay);
        Assert.False(patches.Single(a => a.Title == "minor").IsBigDay);
    }

    [Fact]
    public async Task GetCollectionAsync_KeepsBigDaysTheFeedNoLongerLists()
    {
        var feed = new List<AssetPatchModel> { Entry("steam", "recent", new DateTime(2026, 10, 5, 3, 0, 0, DateTimeKind.Utc)) };
        var collection = new PatchCollection(new FakeClient(feed, ["2025-08-18T20:43:52Z"]));

        var patches = (await collection.GetCollectionAsync()).ToList();

        var old = patches.Single(a => a.Source == PatchCollection.BigDaySource);
        Assert.True(old.IsBigDay);
        Assert.Equal(new DateTime(2025, 8, 18, 20, 43, 52, DateTimeKind.Utc), old.PubDate);
        Assert.Equal("big-day:2025-08-18T20:43:52Z", old.Guid.Text);
    }

    [Fact]
    public void Map_UsesTheFeedLinkAsTheId()
    {
        var patch = CollectionEntityMaps.Map(Entry("steam", "  Halloween  ", new DateTime(2026, 10, 30, 20, 51, 49, DateTimeKind.Utc)));

        Assert.Equal("https://example.com/  Halloween  ", patch.Id);
        Assert.Equal("Halloween", patch.Title);
        Assert.Equal("steam", patch.Source);
        Assert.Equal(DateTimeKind.Utc, patch.PublishedAt.Kind);
    }

    [Fact]
    public void Map_WithoutAGuid_FallsBackToTheLinkThenTheSourceAndTime()
    {
        var published = new DateTime(2026, 10, 30, 20, 51, 49, DateTimeKind.Utc);

        var withLink = CollectionEntityMaps.Map(new AssetPatchModel { Source = "forum", Link = "https://example.com/a", PubDate = published });
        var withoutLink = CollectionEntityMaps.Map(new AssetPatchModel { Source = "forum", PubDate = published });

        Assert.Equal("https://example.com/a", withLink.Id);
        Assert.StartsWith("forum:2026-10-30T20:51:49", withoutLink.Id);
    }

    [Fact]
    public void Map_CarriesTheBigDayFlag()
    {
        var model = Entry("forum", "major", new DateTime(2026, 9, 29, 0, 0, 0, DateTimeKind.Utc));
        model.IsBigDay = true;

        Patch patch = CollectionEntityMaps.Map(model);

        Assert.True(patch.IsBigDay);
    }
}
