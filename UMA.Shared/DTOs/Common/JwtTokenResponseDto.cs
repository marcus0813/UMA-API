namespace UMA.Shared.DTOs.Common
{
    public class JwtTokenResponseDto
    {
        public JwtTokenDto AccessToken { get; set; }
        public JwtTokenDto RefreshToken { get; set; }

    }
}
