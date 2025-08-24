using System.ComponentModel.DataAnnotations;

namespace UMA.Shared.DTOs.Request
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 24 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(24, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 24 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
