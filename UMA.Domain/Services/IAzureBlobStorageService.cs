using Microsoft.AspNetCore.Http;
using UMA.Shared.DTOs.Common;

namespace UMA.Domain.Services
{
    public interface IAzureBlobStorageService
    {
        Task<BlobDto> UploadFilesAsync(IFormFile blob, Guid UserID);
    }
}
