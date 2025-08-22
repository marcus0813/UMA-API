using UMA.Shared.DTOs.Common;

namespace UMA.Shared.DTOs.Response
{
    public class LoginResponse: JwtTokenResponseDto
    {
         public UserResponse UserResponse { get; set; }
    }
}
