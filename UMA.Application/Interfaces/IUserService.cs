using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;

namespace UMA.Application.Interfaces
{

    public interface IUserService
    {
        Task<UserResponse> GetUserAsync(GetUserRequest request);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse> UpdateUserAsync(UpdateUserRequest request);
        Task<UploadPictureResponse> UploadProfilePictureAsync(UploadPictureRequest request);
    }
}