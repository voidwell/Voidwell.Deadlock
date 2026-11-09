namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// A patch note published in the game's forum or Steam news. The API only returns the latest entries, so the table
/// grows as they are collected each day.
/// </summary>
public class Patch
{
    /// <summary>
    /// The entry's permanent link, which is unique across sources.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// forum or steam.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string? Link { get; set; }
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// True for the maintainer-curated major patch days.
    /// </summary>
    public bool IsBigDay { get; set; }
}
