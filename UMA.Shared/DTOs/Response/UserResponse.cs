using UMA.Shared.DTOs.Common;

namespace UMA.Shared.DTOs.Response
{
    public class UserResponse: JwtTokenResponseDto
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public string? ProfilePictureUrl { get; }
        public DateTime CreatedAt { get; }

        public UserResponse(string firstName, string lastName, string email, string? profilePictureUrl, DateTime createdAt)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            ProfilePictureUrl = profilePictureUrl;
            CreatedAt = createdAt;
        }
    }
}
