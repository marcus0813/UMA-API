using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;

namespace UMA.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> VerifyLogin(LoginRequest request);
        Task<TokenResponse> RefreshAcess(RefreshRequest request);
    }
}
