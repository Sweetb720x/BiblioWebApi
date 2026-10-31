using System.Security.Claims;
using BiblioWebApi.Application.Auth;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous, HttpPost("register")]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request, CancellationToken ct) => Created("/api/auth/me", await sender.Send(new RegisterCommand(request), ct));
    [AllowAnonymous, HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequestDto request, CancellationToken ct) => Ok(await sender.Send(new LoginCommand(request), ct));
    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct) => Ok(await sender.Send(new GetCurrentUserQuery(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)), ct));
    [Authorize, HttpPost("logout")]
    public IActionResult Logout() => NoContent();
    [Authorize, HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(CancellationToken ct) => Ok(await sender.Send(new RefreshTokenQuery(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)), ct));
    [AllowAnonymous, HttpPost("forgot-password")]
    public IActionResult ForgotPassword() => Ok(new { message = "Si el correo existe, se enviaron instrucciones de recuperación." });
    [AllowAnonymous, HttpPost("reset-password")]
    public IActionResult ResetPassword() => Ok(new { message = "Solicitud recibida." });
}


