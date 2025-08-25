using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UMA.Shared.DTOs.Request
{
    public class RefreshRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string Token { get; set; }

        [JsonIgnore]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    }
}
