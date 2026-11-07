namespace Voidwell.Deadlock.Collections.Models;

public class AssetRankModel
{
    public int Tier { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
    public AssetRankImages? Images { get; set; }
}

public class AssetRankImages
{
    public string? Large { get; set; }
}
