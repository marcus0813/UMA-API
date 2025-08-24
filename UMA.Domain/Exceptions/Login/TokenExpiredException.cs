namespace UMA.Domain.Exceptions.Login
{
    public class TokenExpiredException : Exception
    {
        private const string DefaultMessage = "Token Expired, please login again.";
        public TokenExpiredException() : base(DefaultMessage) { }
    }
}
