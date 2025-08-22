using System.ComponentModel.DataAnnotations;

namespace UMA.Shared.DTOs.Request
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 24 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 24 characters.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
