using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Auth.Core.Entities;
using Auth.Core.Interfaces;
using Auth.Core.Models;

namespace Auth.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenService(IOptions<AuthenticationSettings> authenticationSettings, IRefreshTokenRepository refreshTokenRepository)
        {
            this._authenticationSettings = authenticationSettings.Value;
            this._refreshTokenRepository = refreshTokenRepository;
            this._tokenHandler = new JwtSecurityTokenHandler();
        }        

        public async Task<bool> ValidateAccessTokenAsync(string accessToken)
        {
            var canReadAccessToken = this._tokenHandler.CanReadToken(accessToken);

            if (!canReadAccessToken) 
            {
                return false;
            }

            // Not sure if this takes expiration into account?
            SecurityToken validatedToken; 
            var key = Encoding.ASCII.GetBytes(this._authenticationSettings.Secret);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var user = this._tokenHandler.ValidateToken(accessToken, parameters, out validatedToken);

            return validatedToken != null;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, UserDetails userDetails)
        {
            var refreshTokenValue = this._refreshTokenRepository
                .FindByCondition(e => !e.IsRevoked && e.Token == refreshToken && e.UserId == userDetails.Id)
                .Select(e => e.Token)
                .SingleOrDefault();
            
            return refreshTokenValue != null;
        }

        public async Task<Token> GenerateTokenAsync(UserDetails userDetails)
        {
            var key = Encoding.ASCII.GetBytes(this._authenticationSettings.Secret);
            var expires = DateTime.UtcNow.AddDays(7);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userDetails.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, userDetails.Email)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _authenticationSettings.Issuer,
                Audience = _authenticationSettings.Audience
            };

            var accessToken = this._tokenHandler.CreateToken(tokenDescriptor);

            var refreshTokenValue = this._refreshTokenRepository
                .FindByCondition(e => !e.IsRevoked && e.UserId == userDetails.Id)
                .Select(e => e.Token)
                .SingleOrDefault();

            if (refreshTokenValue == null)
            {
                refreshTokenValue = this.GenerateRefreshToken();
                var refreshToken = new RefreshToken {
                    Token = refreshTokenValue,
                    UserId = userDetails.Id
                };

                this._refreshTokenRepository.Create(refreshToken);
            } 

            return new Token { 
                AccessToken = this._tokenHandler.WriteToken(accessToken),
                RefreshToken = refreshTokenValue,
                Expires = expires
            };
        }

        public async Task<UserDetails> DecodeAccessTokenAsync(string accessToken)
        {
            var tokenCanBeRead = this._tokenHandler.CanReadToken(accessToken);
            
            if (!tokenCanBeRead)
            {
                return null;
            }

            SecurityToken validatedToken; 
            var key = Encoding.ASCII.GetBytes(this._authenticationSettings.Secret);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var user = this._tokenHandler.ValidateToken(accessToken, parameters, out validatedToken);

            return this.ToUserDetails(user);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create()){
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private UserDetails ToUserDetails(ClaimsPrincipal principal)
        {
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            Guid userId;
            if (Guid.TryParse(idClaim, out var parsedId))
            {
                return new UserDetails 
                {
                    Id = parsedId,
                    Email = email,
                };
            }

            return new UserDetails {};
        }
    
    }
}