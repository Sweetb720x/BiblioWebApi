using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BiblioWebApi.Application.Services;

public sealed class AuthService(IConfiguration configuration) : IAuthService
{
    public string HashPassword(string password) { var salt = RandomNumberGenerator.GetBytes(16); var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA256, 32); return $"pbkdf2$120000$sha256${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"; }
    public bool VerifyPassword(string password, string encoded) { var parts = encoded.Split('$'); if (parts.Length != 5 || !int.TryParse(parts[1], out var iterations)) return false; var salt = Convert.FromBase64String(parts[3]); var expected = Convert.FromBase64String(parts[4]); var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length); return CryptographicOperations.FixedTimeEquals(actual, expected); }
    public TokenResponse CreateToken(User user) { var jwt = configuration.GetSection("Jwt"); var key = jwt["Key"] ?? throw new InvalidOperationException("JWT key not configured"); var expires = DateTime.UtcNow.AddMinutes(jwt.GetValue<int>("ExpireMinutes", 60)); var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role.ToString()) }; var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256); var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims, expires: expires, signingCredentials: credentials); return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expires); }
    public UserResponse ToUserResponse(User user) => new(user.Id, user.FullName, user.Email, user.Role, user.Status, user.CreatedAt);
}



