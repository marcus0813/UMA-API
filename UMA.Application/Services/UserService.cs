using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UMA.Application.Interfaces;
using UMA.Domain.Entities;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;
using UMA.Domain.Repositories;
using UMA.Domain.Services;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Exceptions.ImageUpload;
using UMA.Domain.Exceptions.Extensions;

namespace UMA.Application.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IPasswordHasherService _passwordHasherSevice;
        private readonly IJwtTokenService _jwtTokenService;

        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, IAzureBlobStorageService azureBlobStorageService, IPasswordHasherService passwordHasherService, IJwtTokenService jwtTokenService,IConfiguration config)
        {
            _userRepository = userRepository;
            _azureBlobStorageService=azureBlobStorageService;
            _jwtTokenService =  jwtTokenService;
            _passwordHasherSevice = passwordHasherService;

            _config = config;
        }
        public async Task<UserResponse> GetUserAsync(GetUserRequest request)
        {
            var user = await _userRepository.GetUserByIDAsync(request.UserID);          
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            TokenValidation(request.UserID, request.Email);

            return user.ToDto();
        }
        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user != null)
            {
                throw new EmailAlreadyExistsException();
            }
            user = new User
            {
                ID = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = _passwordHasherSevice.Hash(request.Password)
            };

            await _userRepository.AddUserAsync(user);

            return user.ToDto();
        }

        public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            TokenValidation(request.UserID, request.Email);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            if (request.Password != null)
            {
                user.Password = _passwordHasherSevice.Hash(request.Password);
            }

            await _userRepository.UpdateUserAsync(user);

            return user.ToDto();
        }
        public async Task<UploadPictureResponse> UploadProfilePictureAsync(UploadPictureRequest request)
        {          
            var user = await _userRepository.GetUserByIDAsync(request.UserID);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            TokenValidation(request.UserID, request.Email);

            ImageValidation(request.ProfilePicture);

            try
            {
                var uploadedFile = await _azureBlobStorageService.UploadFilesAsync(request.ProfilePicture);
                user.ProfilePictureUrl = uploadedFile.Uri;
            }
            catch (Exception ex)
            {
                throw new AzureStorageException(ex.InnerException.Message);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            return new UploadPictureResponse { 
                UserID = user.ID,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
        private void TokenValidation(Guid userID, string email)
        {
            var userFromTokenClaim = _jwtTokenService.GetValueFromTokenClaims();
            if (userID != userFromTokenClaim.ID || email.ToLower() != userFromTokenClaim.Email.ToLower())
            {
                throw new UnauthorizedUserException();
            }
        }
        private void ImageValidation(IFormFile imageFile) 
        {
            var maximumImageSizeinMB = _config.GetValue<long>("ImageUploadConfig:MaximumImageSizeinMB");
            long imageSizeInBytes = maximumImageSizeinMB * 1024 * 1024;
            if (imageFile.Length > imageSizeInBytes)
            {
                throw new InvalidImageSizeException(maximumImageSizeinMB.ToString());
            }

            var imageFileNameWithExt = Path.GetFileName(imageFile.FileName).ToUpper();
            var fileFormat = Path.GetExtension(imageFileNameWithExt).TrimStart('.');
            var allowedFileFormat = _config["ImageUploadConfig:AllowedImageFormat"];
            string[] fileFormats = allowedFileFormat.Split('/');
            if (!fileFormats.Contains(fileFormat))
            {
                throw new InvalidImageFormatException(allowedFileFormat.ToString());
            }
        }
    }
}
