using UMA.Shared.DTOs.Common;

namespace UMA.Domain.Services
{
    public interface IJwtTokenService
    {
        JwtTokenDto GenerateAccessToken(Guid userID, string email, DateTime loginTime);
        JwtTokenDto GenerateRefreshToken(Guid userID, string email, DateTime loginTime);
        UserDto GetValueFromTokenClaims();
    }
}
