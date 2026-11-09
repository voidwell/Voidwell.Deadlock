using System.Globalization;
using Voidwell.Deadlock.Collections.Collection.Abstract;
using Voidwell.Deadlock.Collections.Models;

namespace Voidwell.Deadlock.Collections.Collection;

/// <summary>
/// Patch notes from the combined feed, marked with the curated major patch days. The API returns only the latest
/// entries and has no way to page back, so older history builds up as this is collected over time.
/// </summary>
public class PatchCollection : IStaticCollection<AssetPatchModel>
{
    public const string BigDaySource = "big-day";

    private readonly IAssetsClient _client;

    public string CollectionName => "patches";

    public PatchCollection(IAssetsClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<AssetPatchModel>> GetCollectionAsync()
    {
        var feed = (await _client.GetListAsync<AssetPatchModel>("v2/patches")).ToList();
        var bigDays = (await _client.GetListAsync<string>("v1/patches/big-days"))
            .Select(a => DateTimeOffset.Parse(a, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime)
            .ToList();

        var bigDayDates = bigDays.Select(a => DateOnly.FromDateTime(a)).ToHashSet();

        foreach (var item in feed)
        {
            item.IsBigDay = bigDayDates.Contains(DateOnly.FromDateTime(item.PubDate));
        }

        // A major patch day the feed no longer lists still marks a boundary, so keep it as its own entry.
        var feedDates = feed.Select(a => DateOnly.FromDateTime(a.PubDate)).ToHashSet();
        foreach (var bigDay in bigDays.Where(a => !feedDates.Contains(DateOnly.FromDateTime(a))))
        {
            feed.Add(new AssetPatchModel
            {
                Source = BigDaySource,
                Title = "Major patch day",
                PubDate = bigDay,
                Guid = new AssetPatchGuid { Text = $"{BigDaySource}:{bigDay:yyyy-MM-ddTHH:mm:ssZ}" },
                IsBigDay = true
            });
        }

        return feed;
    }
}
