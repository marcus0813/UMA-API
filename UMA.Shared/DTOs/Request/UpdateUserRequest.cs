using System.ComponentModel.DataAnnotations;

namespace UMA.Shared.DTOs.Request
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserID { get; set; }

        [StringLength(24, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 24 characters.")]
        public string FirstName { get; set; }

        [StringLength(24, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 24 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter and one number.")]
        public string Password { get; set; } = string.Empty;
    }
}
