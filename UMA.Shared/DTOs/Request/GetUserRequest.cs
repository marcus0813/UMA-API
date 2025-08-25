using System.ComponentModel.DataAnnotations;

namespace UMA.Shared.DTOs.Request
{
    public class GetUserRequest
    {
        [Required(ErrorMessage = "User ID is required.")]

        public Guid UserID { get; set; }
        [Required(ErrorMessage = "Email is required.")]

        public string Email { get; set; }
    }
}
