using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Users;

public sealed record ListUsersQuery : IRequest<IReadOnlyCollection<UserResponse>>;
public sealed record GetUserQuery(int Id) : IRequest<UserResponse>;
public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<UserResponse>;
public sealed record UpdateUserCommand(int Id, UpdateUserRequest Request) : IRequest<UserResponse>;
public sealed record ChangeUserStatusCommand(int Id, StatusRequest Request) : IRequest;
public sealed record RemoveUserCommand(int Id) : IRequest;

public sealed class ListUsersHandler(IRepository<User> users, IAuthService auth) : IRequestHandler<ListUsersQuery, IReadOnlyCollection<UserResponse>>
{
    public async Task<IReadOnlyCollection<UserResponse>> Handle(ListUsersQuery request, CancellationToken ct) => (await users.Query().ToListAsync(ct)).Select(auth.ToUserResponse).ToArray();
}
public sealed class GetUserHandler(IRepository<User> users, IAuthService auth) : IRequestHandler<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken ct) => auth.ToUserResponse(await users.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND"));
}
public sealed class CreateUserHandler(IRepository<User> users, IUnitOfWork unitOfWork, IAuthService auth) : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var request = command.Request; var user = new User { FullName = request.FullName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), PasswordHash = auth.HashPassword(request.Password), Role = request.Role };
        await users.AddAsync(user, ct); await unitOfWork.SaveChangesAsync(ct); return auth.ToUserResponse(user);
    }
}
public sealed class UpdateUserHandler(IRepository<User> users, IUnitOfWork unitOfWork, IAuthService auth) : IRequestHandler<UpdateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND"); var request = command.Request;
        if (request.FullName is not null) user.FullName = request.FullName; if (request.Email is not null) user.Email = request.Email.Trim().ToLowerInvariant(); if (request.Role.HasValue) user.Role = request.Role.Value; if (request.Status.HasValue) user.Status = request.Status.Value;
        await unitOfWork.SaveChangesAsync(ct); return auth.ToUserResponse(user);
    }
}
public sealed class ChangeUserStatusHandler(IRepository<User> users, IUnitOfWork unitOfWork) : IRequestHandler<ChangeUserStatusCommand>
{
    public async Task Handle(ChangeUserStatusCommand command, CancellationToken ct) { var user = await users.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND"); user.Status = command.Request.Status; await unitOfWork.SaveChangesAsync(ct); }
}
public sealed class RemoveUserHandler(IRepository<User> users, IUnitOfWork unitOfWork) : IRequestHandler<RemoveUserCommand>
{
    public async Task Handle(RemoveUserCommand command, CancellationToken ct) { var user = await users.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND"); user.Status = UserStatus.Inactive; await unitOfWork.SaveChangesAsync(ct); }
}




