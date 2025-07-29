using AuthService.App.Models;

namespace AuthService.App.Interfaces;

public interface IAuthorizeService
{
    Task<TokenResponse> Login(LoginRequest request);
    Task<TokenResponse> Registration(LoginRequest request);
    //Task<TokenResponse> RefreshTokenAsync(string refreshToken);
}