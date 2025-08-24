using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;

namespace UMA.Application.Interfaces
{

    public interface IUserService
    {
        Task<UserResponse> GetUserAsync(GetUserRequest request);
        Task<TokenResponse> CreateUserAsync(CreateUserRequest request);
        Task UpdateUserAsync(UpdateUserRequest request);
        Task<UploadPictureResponse> UploadProfilePictureAsync(UploadPictureRequest request);
    }
}