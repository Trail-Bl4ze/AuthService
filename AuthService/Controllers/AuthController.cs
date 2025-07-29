
using AuthService.App.Interfaces;
using AuthService.App.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private IAuthorizeService FAuthService;
    public AuthController(IAuthorizeService authService)
    {
        FAuthService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await FAuthService.Login(request);

        // Возвращаем только access token
        return Ok(new
        {
            AccessToken = result.AccessToken,
            ExpiresIn = result.ExpiresIn
        });
    }

    [HttpPost("registration")]
    public async Task<IActionResult> RegistrationAsync([FromBody] LoginRequest request) 
    {
        var result = await FAuthService.Registration(request);

        return Ok(new
        {
            AccessToken = result.AccessToken,
            ExpiresIn = result.ExpiresIn
        });
    }

    //[HttpPost("refresh")]
    // public async Task<IActionResult> RefreshTokenAsync()
    // {
    //     // Получаем refresh token из cookie
    //     var refreshToken = Request.Cookies["refreshToken"];

    //     if (string.IsNullOrEmpty(refreshToken))
    //         return Unauthorized("Refresh token is missing");

    //     var result = await FAuthService.RefreshTokenAsync(refreshToken);

    //     return Ok(new
    //     {
    //         AccessToken = result.AccessToken,
    //         ExpiresIn = result.ExpiresIn
    //     });
    // }

    // Вспомогательный метод для установки cookie, костыльное время пока
    // private void SetRefreshTokenCookie(string token)
    // {
    //     var cookieOptions = new CookieOptions
    //     {
    //         HttpOnly = true,
    //         Expires = DateTime.UtcNow.AddDays(7),
    //         Secure = true, // Только для HTTPS
    //         SameSite = SameSiteMode.Strict,
    //         Path = "/api/auth" // Ограничиваем путь
    //     };

    //     Response.Cookies.Append("refreshToken", token, cookieOptions);
    // }
}