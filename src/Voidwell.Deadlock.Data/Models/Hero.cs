using System.ComponentModel.DataAnnotations;

namespace Voidwell.Deadlock.Data.Models;

public class Hero
{
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Internal game name, for example hero_inferno.
    /// </summary>
    public string? ClassName { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// Hero damage archetype as reported by the asset source, for example mystic.
    /// </summary>
    public string? HeroType { get; set; }

    /// <summary>
    /// Release state as reported by the asset source, for example release.
    /// </summary>
    public string? DevelopmentState { get; set; }

    /// <summary>
    /// False for heroes that are unreleased, in development or disabled.
    /// </summary>
    public bool IsPlayable { get; set; }

    public string? ImageUrl { get; set; }
    public string? IconUrl { get; set; }
}
