using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UMA.Domain.Services;
using UMA.Shared.DTOs.Common;

namespace UMA.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public JwtTokenDto GenerateAccessToken(Guid userID, string email, DateTime loginTime)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Secret"];
            var tokenValidityMinutes = _config.GetValue<int>("Jwt:TokenValidityMinutes");
            var tokenExpiryTimeStamp = loginTime.AddMinutes(tokenValidityMinutes);

            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.NameId, userID.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptior);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new JwtTokenDto
            {
                JwtToken= accessToken,
                ExpiryDate = tokenExpiryTimeStamp
            };
        }
        public JwtTokenDto GenerateRefreshToken(Guid userID, string email, DateTime loginTime)
        {
            var refreshTokenExpiryDays = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays");
            var tokenExpiryTimeStamp = loginTime.AddDays(refreshTokenExpiryDays);

            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new JwtTokenDto
            {
                JwtToken = Convert.ToBase64String(randomNumber),
                ExpiryDate = tokenExpiryTimeStamp
            };
        }
        public UserDto GetValueFromTokenClaims()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var userIDFromToken = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailFromToken = currentUser.FindFirst(ClaimTypes.Email)?.Value;

            Guid userID;
            Guid.TryParse(userIDFromToken, out userID);

            return new UserDto
            {
                ID = userID,
                Email = emailFromToken
            };
        }
    }
}