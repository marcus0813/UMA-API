namespace UMA.Domain.Exceptions.User
{
    public class UnauthorizedUserException: Exception
    {
        private const string DefaultMessage = "Unauthorized User, please try login again.";
        public UnauthorizedUserException() : base(DefaultMessage) { }
    }
}
