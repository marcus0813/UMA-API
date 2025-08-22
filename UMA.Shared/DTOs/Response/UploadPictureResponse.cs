using UMA.Shared.DTOs.Common;

namespace UMA.Shared.DTOs.Response
{
    public class UploadPictureResponse: JwtTokenResponseDto
    {
        public Guid UserID { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
