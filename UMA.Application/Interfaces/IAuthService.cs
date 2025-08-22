using UMA.Shared.DTOs.Common;

namespace UMA.Application.Interfaces
{
    public interface IAuthService
    {
        Task<JwtTokenResponseDto> VerifyLogin(string email, string password);

    }
}
