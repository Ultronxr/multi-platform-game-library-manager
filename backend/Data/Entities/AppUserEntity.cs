namespace GameLibrary.Api.Data.Entities;

public sealed class AppUserEntity
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}
