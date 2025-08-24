namespace UMA.Domain.Exceptions.ImageUpload
{
    public class InvalidImageSizeException : Exception
    {
        private static string DefaultMessage = "Image size exceeds the limit of {0}MB.";
        public InvalidImageSizeException(string fileSize) : base(string.Format(DefaultMessage, fileSize)) { }
    }
}
