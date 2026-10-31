using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using BiblioWebApi.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Auth;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<TokenResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => string.IsNullOrWhiteSpace(Request.FullName) || string.IsNullOrWhiteSpace(Request.Email) || string.IsNullOrWhiteSpace(Request.Password) ? ["FullName, Email and Password are required."] : [];
}

public sealed record LoginCommand(LoginRequestDto Request) : IRequest<TokenResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => string.IsNullOrWhiteSpace(Request.Email) || string.IsNullOrWhiteSpace(Request.Password) ? ["Email and Password are required."] : [];
}

public sealed record GetCurrentUserQuery(int UserId) : IRequest<UserResponse>;
public sealed record RefreshTokenQuery(int UserId) : IRequest<TokenResponse>;

public sealed class RegisterCommandHandler(IRepository<User> users, IUnitOfWork unitOfWork, IAuthService auth) : IRequestHandler<RegisterCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        if (await users.Query().AnyAsync(x => x.Email == email, ct)) throw new InvalidOperationException("EMAIL_EXISTS");
        var user = new User { FullName = command.Request.FullName.Trim(), Email = email, PasswordHash = auth.HashPassword(command.Request.Password), Role = command.Request.Role is UserRole.Administrator or UserRole.Librarian ? UserRole.Student : command.Request.Role };
        await users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return auth.CreateToken(user);
    }
}

public sealed class RefreshTokenHandler(IRepository<User> users, IAuthService auth) : IRequestHandler<RefreshTokenQuery, TokenResponse>
{
    public async Task<TokenResponse> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct) ?? throw new UnauthorizedAccessException();
        if (user.Status != UserStatus.Active) throw new UnauthorizedAccessException();
        return auth.CreateToken(user);
    }
}

public sealed class LoginCommandHandler(IRepository<User> users, IAuthService auth) : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await users.Query().SingleOrDefaultAsync(x => x.Email == command.Request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !auth.VerifyPassword(command.Request.Password, user.PasswordHash)) throw new UnauthorizedAccessException();
        if (user.Status != UserStatus.Active) throw new UnauthorizedAccessException();
        return auth.CreateToken(user);
    }
}

public sealed class GetCurrentUserHandler(IRepository<User> users, IAuthService auth) : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND");
        return auth.ToUserResponse(user);
    }
}



