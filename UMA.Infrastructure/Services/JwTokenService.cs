using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UMA.Domain.Services;
using UMA.Shared.DTOs.Common;

namespace UMA.Infrastructure.Services
{
    public class JwTokenService : IJwTokenService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwTokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateAccessToken(Guid userID, string email, DateTime loginTime, string jwTokenID)
        {
            //Set JWT token claims
            //Configurable in appsettings
            //Must match with UI 
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Secret"];
            var tokenValidityMinutes = _config.GetValue<int>("Jwt:TokenValidityMinutes");
            var tokenExpiryTimeStamp = loginTime.AddMinutes(tokenValidityMinutes);

            //Configure claims to validate request while calling
            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.Jti, jwTokenID),
                    new Claim(JwtRegisteredClaimNames.NameId, userID.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
            };

            //Create jwt instance to create token and write into string format
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptior);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }
        public string GenerateRefreshToken(Guid userID, string email, DateTime loginTime)
        {
            //Generate refresh token so that user dun need to keep login to get the access token
            //Configurable in appsettings
            var refreshTokenExpiryDays = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays");
            var tokenExpiryTimeStamp = loginTime.AddDays(refreshTokenExpiryDays);

            //Generate JW token ID, using random numbers to bytes
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var token = new TokenDto
            {
                JwTokenID = Convert.ToBase64String(randomNumber),
                ExpiryDate = tokenExpiryTimeStamp
            };

            //Custom write Token feature, encoding for refresh token only
            string tokenString = JsonSerializer.Serialize(token);
            byte[] bytes= Encoding.UTF8.GetBytes(tokenString);
            string refreshToken = Convert.ToBase64String(bytes);

            return refreshToken;
        }

        public bool VerifyUserTokenClaims(Guid userID, string Email, string refreshToken)
        {
            //Get Token claims from current request header
            var currentUser = _httpContextAccessor.HttpContext?.User;
            
            var jwtTokenIDFromToken = currentUser!.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userIDStringFromToken = currentUser!.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
            var emailFromToken = currentUser.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            //Convert to valid userID type, GUID
            Guid userIDFromToken;
            Guid.TryParse(userIDStringFromToken, out userIDFromToken);

            //Get jwTokenID from db refresh token
            TokenDto refreshTokenDto  = GetRefreshTokenValue(refreshToken);

            //Checking on NameID, Email, Expiry Date, 
            return (
                userIDFromToken == userID && 
                emailFromToken.ToLower() == Email.ToLower() && 
                jwtTokenIDFromToken == refreshTokenDto.JwTokenID
            );
        }

        public TokenDto GetRefreshTokenValue(string refreshToken)
        {
            //Custom revert Token feature, decoding for refresh token only
            byte[] bytes = Convert.FromBase64String(refreshToken);
            string tokenString = Encoding.UTF8.GetString(bytes);
            var decodedRefreshToken = JsonSerializer.Deserialize<TokenDto>(tokenString);

            return decodedRefreshToken;
        }
    }
}