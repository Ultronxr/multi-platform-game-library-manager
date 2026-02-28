namespace GameLibrary.Api.Services;

public sealed class AuthOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 120;
    public string BootstrapToken { get; init; } = string.Empty;
}
