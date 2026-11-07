using System.ComponentModel.DataAnnotations;

namespace Voidwell.Deadlock.Data.Models;

public class Item
{
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// Internal game name, for example upgrade_clip_size.
    /// </summary>
    public string? ClassName { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Item type as reported by the asset source: ability, weapon or upgrade.
    /// </summary>
    public string? Type { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// Shop slot (weapon, spirit or vitality). Only set for upgrades.
    /// </summary>
    public string? SlotType { get; set; }

    /// <summary>
    /// Shop tier, 1 to 4. Only set for upgrades.
    /// </summary>
    public int? Tier { get; set; }

    public int? Cost { get; set; }

    public bool IsShopable { get; set; }
}
