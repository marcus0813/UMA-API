namespace UMA.Shared.DTOs.Common
{
    public class JwtTokenDto
    {
        public string JwtToken { get; set; }
        public DateTime ExpiryDate { get; set; }     
    }
}
