namespace Voidwell.Deadlock.Data.Models;

/// <summary>
/// A source file that has been loaded, so an import can resume and skip files it has already read.
/// </summary>
public class IngestFile
{
    /// <summary>
    /// The file's path in the source manifest.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public long Rows { get; set; }
    public DateTime IngestedAt { get; set; }
}
