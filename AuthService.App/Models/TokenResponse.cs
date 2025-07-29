namespace AuthService.App.Models;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; } // Время жизни access token в секундах

    public TokenResponse(string accessToken, int expiresIn)
    {
        AccessToken = accessToken;
        ExpiresIn = expiresIn;
    }
}