namespace Voidwell.Deadlock.Collections.Models;

public class AssetHeroModel
{
    public int Id { get; set; }
    public string? ClassName { get; set; }
    public string? Name { get; set; }
    public string? HeroType { get; set; }
    public string? DevelopmentState { get; set; }
    public bool PlayerSelectable { get; set; }
    public bool Disabled { get; set; }
    public bool InDevelopment { get; set; }

    /// <summary>
    /// Image URLs keyed by their asset name, for example icon_hero_card or icon_image_small.
    /// </summary>
    public Dictionary<string, string?>? Images { get; set; }
}
