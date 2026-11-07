using System.ComponentModel.DataAnnotations;

namespace Voidwell.Deadlock.Data.Models;

public class Rank
{
    /// <summary>
    /// Rank tier index, 0 (Obscurus) to 11.
    /// </summary>
    [Required]
    public int Tier { get; set; }

    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
}
