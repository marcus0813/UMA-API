namespace UMA.Domain.Exceptions.Login
{
    public class InvalidCrendentialsException : Exception
    {
        private const string DefaultMessage = "Invalid Credentials, please try again.";
        public InvalidCrendentialsException() : base(DefaultMessage) { }
    }
}
