using UMA.Shared.DTOs.Response;


namespace UMA.Domain.Entities
{
    public class User
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public UserResponse ToDto() => new(FirstName, LastName, Email, ProfilePictureUrl, CreatedAt);

    }
}
