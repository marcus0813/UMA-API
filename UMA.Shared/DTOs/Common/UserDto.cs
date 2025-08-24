namespace UMA.Shared.DTOs.Common
{
    public class UserDto
    {
        public Guid ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public UserDto(string? firstName, string? lastName, string? email, string? profilePictureUrl, DateTime? createdAt, DateTime? updatedAt)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            ProfilePictureUrl = profilePictureUrl;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

    }
}
