namespace UMA.Domain.Exceptions.User
{
    public class EmailAlreadyExistsException : Exception
    {
        private const string DefaultMessage = "Email already exists.";
        public EmailAlreadyExistsException() : base(DefaultMessage) { }
    }
}
