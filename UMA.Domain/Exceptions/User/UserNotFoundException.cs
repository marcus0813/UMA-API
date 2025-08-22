namespace UMA.Domain.Exceptions.User
{
    public class UserNotFoundException : Exception
    {
        private const string DefaultMessage = "User not exists.";
        public UserNotFoundException() : base(DefaultMessage) { }
    }
}
