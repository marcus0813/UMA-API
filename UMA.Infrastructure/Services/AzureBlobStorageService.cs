using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UMA.Shared.DTOs.Common;
using UMA.Domain.Services;

namespace UMA.Infrastructure.Services
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly string _storageAccount;
        private readonly string _accessKey;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainer;
        private readonly IConfiguration _config;

        public AzureBlobStorageService(IConfiguration config)
        {
            _config = config;
            _storageAccount = config["AzureBlob:StorageAccount"];
            _accessKey = config["AzureBlob:AccessKey"];

            var credential = new StorageSharedKeyCredential(_storageAccount, _accessKey);
            var blobUri = string.Format(config["AzureBlob:BlobUri"], _storageAccount);

            _blobServiceClient= new BlobServiceClient(new Uri(blobUri), credential);
            _blobContainer = _blobServiceClient.GetBlobContainerClient(_config["AzureBlob:Container"]);
        }

        public async Task<BlobDto> UploadFilesAsync(IFormFile blob)
        {

            BlobDto result = new BlobDto();

            BlobClient client = _blobContainer.GetBlobClient(blob.FileName);

            using (var stream = blob.OpenReadStream())
            {
                await client.UploadAsync(stream, overwrite: true);
            }
            result.Uri = client.Uri.AbsoluteUri;
            result.Name = client.Name;

            return result;
        }
    }
}
