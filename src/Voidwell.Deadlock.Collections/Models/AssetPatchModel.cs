namespace Voidwell.Deadlock.Collections.Models;

/// <summary>
/// One entry of the combined forum and Steam patch note feed (/v2/patches). The entry's HTML content is not read.
/// </summary>
public class AssetPatchModel
{
    public string? Source { get; set; }
    public string? Title { get; set; }
    public string? Link { get; set; }
    public DateTime PubDate { get; set; }
    public AssetPatchGuid? Guid { get; set; }

    /// <summary>
    /// Set by the collection when the entry falls on one of the curated major patch days.
    /// </summary>
    public bool IsBigDay { get; set; }
}

public class AssetPatchGuid
{
    public string? Text { get; set; }
}
