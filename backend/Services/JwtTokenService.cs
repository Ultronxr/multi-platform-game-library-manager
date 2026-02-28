using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameLibrary.Api.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace GameLibrary.Api.Services;

/// <summary>
/// JWT 令牌签发服务。
/// </summary>
public sealed class JwtTokenService(AuthOptions authOptions)
{
    private readonly byte[] _signingKey = Encoding.UTF8.GetBytes(authOptions.SigningKey);

    /// <summary>
    /// 为指定用户生成访问令牌。
    /// </summary>
    /// <param name="user">登录用户。</param>
    /// <returns>令牌字符串及过期时间。</returns>
    public (string token, DateTime expiresAtUtc) CreateAccessToken(AppUserEntity user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(authOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        // 访问令牌包含用户标识和角色，用于接口级授权控制。
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = authOptions.Issuer,
            Audience = authOptions.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_signingKey),
                SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return (handler.WriteToken(token), expiresAtUtc);
    }
}
