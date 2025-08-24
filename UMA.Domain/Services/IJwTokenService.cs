using UMA.Shared.DTOs.Common;

namespace UMA.Domain.Services
{
    public interface IJwTokenService
    {
        string GenerateAccessToken(Guid userID, string email, DateTime loginTime, string jwTokenID);
        string GenerateRefreshToken(Guid userID, string email, DateTime loginTime);
        bool VerifyUserTokenClaims(Guid userID, string Email, string refreshToken);
        TokenDto GetRefreshTokenValue(string refreshToken);
        
    }
}
