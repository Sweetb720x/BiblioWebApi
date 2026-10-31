using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Domain;

namespace BiblioWebApi.Application.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string encoded);
    TokenResponse CreateToken(User user);
    UserResponse ToUserResponse(User user);
}


