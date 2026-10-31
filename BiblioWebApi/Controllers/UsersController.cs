using BiblioWebApi.Application.Users;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Route("api/users"), Authorize(Roles = "Administrator,Librarian")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyCollection<UserResponse>> GetAll(CancellationToken ct) => sender.Send(new ListUsersQuery(), ct);
    [HttpGet("{id:int}")] public async Task<ActionResult<UserResponse>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetUserQuery(id), ct));
    [HttpPost] public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken ct) { var response = await sender.Send(new CreateUserCommand(request), ct); return CreatedAtAction(nameof(Get), new { id = response.Id }, response); }
    [HttpPut("{id:int}"), HttpPatch("{id:int}")] public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request, CancellationToken ct) => Ok(await sender.Send(new UpdateUserCommand(id, request), ct));
    [HttpPatch("{id:int}/status")] public async Task<IActionResult> Status(int id, StatusRequest request, CancellationToken ct) { await sender.Send(new ChangeUserStatusCommand(id, request), ct); return NoContent(); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) { await sender.Send(new RemoveUserCommand(id), ct); return NoContent(); }
}


