using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UMA.Shared.DTOs.Request
{
    public class UploadPictureRequest
    {
        [Required(ErrorMessage = "Image not found.")]
        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserID { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
