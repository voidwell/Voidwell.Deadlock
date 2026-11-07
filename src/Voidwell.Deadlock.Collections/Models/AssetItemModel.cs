namespace Voidwell.Deadlock.Collections.Models;

public class AssetItemModel
{
    /// <summary>
    /// Item ids are unsigned 32-bit values, so they do not fit in an int.
    /// </summary>
    public long Id { get; set; }
    public string? ClassName { get; set; }
    public string? Name { get; set; }

    /// <summary>
    /// ability, weapon or upgrade.
    /// </summary>
    public string? Type { get; set; }
    public string? Image { get; set; }

    // The remaining fields are only populated for shop upgrades.
    public string? ItemSlotType { get; set; }
    public int? ItemTier { get; set; }
    public int? Cost { get; set; }
    public bool? Shopable { get; set; }
}
