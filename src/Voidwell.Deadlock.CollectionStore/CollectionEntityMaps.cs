using Riok.Mapperly.Abstractions;
using Voidwell.Deadlock.Collections.Models;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.CollectionStore;

/// <summary>
/// Collection model -> database entity maps. Every public static method here is discovered by
/// <see cref="CollectionToEntityMapper"/> and registered under its (source, destination) type pair.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class CollectionEntityMaps
{
    public static Hero Map(AssetHeroModel source)
    {
        return new Hero
        {
            Id = source.Id,
            ClassName = source.ClassName,
            Name = source.Name,
            HeroType = source.HeroType,
            DevelopmentState = source.DevelopmentState,
            IsPlayable = source.PlayerSelectable && !source.Disabled && !source.InDevelopment,
            ImageUrl = GetImage(source, "icon_hero_card"),
            IconUrl = GetImage(source, "icon_image_small")
        };
    }

    public static Item Map(AssetItemModel source)
    {
        return new Item
        {
            Id = source.Id,
            ClassName = source.ClassName,
            Name = source.Name,
            Type = source.Type,
            ImageUrl = source.Image,
            SlotType = source.ItemSlotType,
            Tier = source.ItemTier,
            Cost = source.Cost,
            IsShopable = source.Shopable ?? false
        };
    }

    public static Rank Map(AssetRankModel source)
    {
        return new Rank
        {
            Tier = source.Tier,
            Name = source.Name,
            Color = source.Color,
            ImageUrl = source.Images?.Large
        };
    }

    private static string? GetImage(AssetHeroModel source, string key)
    {
        return source.Images != null && source.Images.TryGetValue(key, out var url) ? url : null;
    }
}
