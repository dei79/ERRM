using System.ComponentModel.DataAnnotations;

namespace ERRM.Models;

public sealed class OidcSettings
{
    public const string SectionName = "Authentication:Oidc";
    public const string AuthorityPlaceholder = "https://your-identity-provider.example.com";
    public const string ClientIdPlaceholder = "your-client-id";
    public const string ClientSecretPlaceholder = "your-client-secret";

    [Required]
    public string Authority { get; init; } = AuthorityPlaceholder;

    [Required]
    public string ClientId { get; init; } = ClientIdPlaceholder;

    [Required]
    public string ClientSecret { get; init; } = ClientSecretPlaceholder;

    [Required]
    public string CallbackPath { get; init; } = "/signin-oidc";

    public string SignedOutCallbackPath { get; init; } = "/signout-callback-oidc";

    public string[] Scopes { get; init; } = ["openid", "profile", "email"];
}
