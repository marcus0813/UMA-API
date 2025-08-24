using UMA.Shared.DTOs.Common;

namespace UMA.Domain.Entities
{
    public class User
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        //Convert from Entity to Dto manually
        public UserDto ToDto() => new(FirstName, LastName, Email, ProfilePictureUrl, CreatedAt, UpdatedAt);
    }
}
