namespace UMA.Domain.Exceptions.Extensions
{
    public class AzureStorageException: Exception
    {
        public AzureStorageException(string message) : base($"Azure Storage: {message}") { }
    }

}
