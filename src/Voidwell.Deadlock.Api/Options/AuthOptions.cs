using System.ComponentModel.DataAnnotations;

namespace Voidwell.Deadlock.Api.Options;

public class AuthOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Auth:Authority is required.")]
    public string? Authority { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Auth:ClientId is required.")]
    public string? ClientId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Auth:ClientSecret is required.")]
    public string? ClientSecret { get; set; }

    public string? RoleClaimType { get; set; }
}
