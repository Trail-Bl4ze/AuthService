
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mapster;
using AuthService.Domain;
using AuthService.App.Interfaces;
using AuthService.App.Models;
using User = AuthService.Domain.Entities.User;
using Microsoft.Extensions.Configuration;

namespace AuthService.App.Services
{

    public class AuthorizeService : IAuthorizeService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthorizeService(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TokenResponse> Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) throw new Exception("Пользователь не найден");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            var domainRefreshToken = newRefreshToken.Adapt<Domain.Entities.RefreshToken>();
            await _context.RefreshTokens.AddAsync(domainRefreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponse(accessToken, 60*120);
        }

        public async Task<TokenResponse> Registration(LoginRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
                throw new Exception("Пользователь с таким email уже есть");

            var user = new User
            {
                PasswordHash = request.Password,
                Email = request.Email
            };

            await _context.Users.AddAsync(user);

            return await GenerateTokens(user);
        }
        
        // public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        // {
        //     var existingToken = await _context.RefreshTokens
        //         .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        //     var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existingToken.UserId);

        //     if (existingToken == null)
        //         throw new SecurityTokenException("Invalid refresh token");
            
        //     if (existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
        //         throw new SecurityTokenException("Expired or revoked token");

        //     existingToken.IsRevoked = true;

        //     var newRefreshToken = new Domain.Entities.RefreshToken
        //     {
        //         Token = GenerateRefreshToken(),
        //         ExpiresAt = DateTime.UtcNow.AddDays(7),
        //         UserId = existingToken.UserId
        //     };

        //     _context.RefreshTokens.Update(existingToken);
        //     await _context.RefreshTokens.AddAsync(newRefreshToken);
        //     await _context.SaveChangesAsync();

        //     return new TokenResponse(GenerateJwtToken(user), newRefreshToken.Token, 60*120);
        // }

        private async Task<TokenResponse> GenerateTokens(User user)
        {
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            var domainRefreshToken = newRefreshToken.Adapt<Domain.Entities.RefreshToken>();
            await _context.RefreshTokens.AddAsync(domainRefreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponse(accessToken, 60*120);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}