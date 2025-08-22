namespace UMA.Domain.Exceptions.ImageUpload
{
    public class InvalidImageFormatException : Exception
    {
        private static string DefaultMessage = "Only {0} Format is allowed.";
        public InvalidImageFormatException(string allowedFileFormat) : base(string.Format(DefaultMessage, allowedFileFormat)) { }
    }
}
