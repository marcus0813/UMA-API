using UMA.Shared.DTOs.Common;

namespace UMA.Shared.DTOs.Response
{
    public class UserResponse
    {
        public UserDto User { get; set; }
        public TokenDto Token { get; set; }
    }
}
