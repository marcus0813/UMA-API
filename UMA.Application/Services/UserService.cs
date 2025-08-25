using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UMA.Application.Interfaces;
using UMA.Domain.Entities;
using UMA.Shared.DTOs.Common;
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
        private readonly IJwTokenService _jwTokenService;

        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, IAzureBlobStorageService azureBlobStorageService, IPasswordHasherService passwordHasherService, IJwTokenService jwTokenService, IConfiguration config)
        {
            _userRepository = userRepository;
            _azureBlobStorageService=azureBlobStorageService;
            _jwTokenService =  jwTokenService;
            _passwordHasherSevice = passwordHasherService;

            _config = config;
        }
        public async Task<UserResponse> GetUserAsync(GetUserRequest request)
        {

            //Check user exists, throw exception if user not found
            var user = await _userRepository.GetUserByIDAsync(request.UserID);
            if (user == null)
            {
                throw new UnauthorizedUserException();
            }

            //Check if user has existing token and verify the token owned by same user
            if (string.IsNullOrEmpty(user.RefreshToken) || !_jwTokenService.VerifyUserTokenClaims(request.UserID, request.Email, user.RefreshToken))
            {
                throw new UnauthorizedUserException();
            }

            //Convert Entity to Dto
            return new UserResponse
            {
                User = user.ToDto()
            };
        }
        public async Task CreateUserAsync(CreateUserRequest request)
        {
            //Check email exists, throw exception if email has been taken
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user != null)
            {
                throw new EmailAlreadyExistsException();
            }

            //Create user Entity for db insertion
            user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = _passwordHasherSevice.Hash(request.Password),
            };

            //Integrate with DB for user creation
            await _userRepository.AddUserAsync(user);
        }
        public async Task UpdateUserAsync(UpdateUserRequest request)
        {
            //Check user exists, throw exception if user not found
            var user = await _userRepository.GetUserByIDAsync(request.UserID);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            //Check if user has existing token and verify the token owned by same user
            if (string.IsNullOrEmpty(user.RefreshToken) || !_jwTokenService.VerifyUserTokenClaims(request.UserID, request.Email, user.RefreshToken))
            {
                throw new UnauthorizedUserException();
            }

            //Set to updated values
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            //Update if only password has changes on it
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = _passwordHasherSevice.Hash(request.Password);
            }

            //Integrate with DB, to update user
            await _userRepository.UpdateUserAsync(user);
        }
        public async Task<UploadPictureResponse> UploadProfilePictureAsync(UploadPictureRequest request)
        {
            //Check user exists, throw exception if user not found
            var user = await _userRepository.GetUserByIDAsync(request.UserID);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            //Check if user has existing token and verify the token owned by same user
            if (string.IsNullOrEmpty(user.RefreshToken) || !_jwTokenService.VerifyUserTokenClaims(request.UserID, request.Email, user.RefreshToken))
            {
                throw new UnauthorizedUserException();
            }

            //Validate profile picture file size and profile picture file format 
            ImageValidation(request.ProfilePicture);

            //Integrate with custom azure service then return uploaded image file url
            //Throw exception if image file failed to upload to Azure Storage
            try
            {
                var uploadedFile = await _azureBlobStorageService.UploadFilesAsync(request.ProfilePicture, request.UserID);
                user.ProfilePictureUrl = uploadedFile.Uri;
            }
            catch (Exception ex)
            {
                throw new AzureStorageException(ex.InnerException.Message);
            }

            user.UpdatedAt = DateTime.UtcNow;

            //Integrate with DB to update selected user image url 
            await _userRepository.UpdateUserAsync(user);
            return new UploadPictureResponse
            {
                UserID = user.ID,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        private void ImageValidation(IFormFile imageFile)
        {
            //Check image file size, throw exception if file size exceeds maximum
            //Configurable in appsettings
            var maximumImageSizeinMB = _config.GetValue<long>("ImageUploadConfig:MaximumImageSizeinMB");
            long imageSizeInBytes = maximumImageSizeinMB * 1024 * 1024;
            if (imageFile.Length > imageSizeInBytes)
            {
                throw new InvalidImageSizeException(maximumImageSizeinMB.ToString());
            }

            //Check image file format, throw exception if file format is not allowed
            //Configurable in appsettings

            string[] contentTypeSplit = imageFile.ContentType.Split('/');
            var imageFileFormat = contentTypeSplit[1].ToUpper();
            var allowedFileFormat = _config["ImageUploadConfig:AllowedImageFormat"];
            string[] fileFormats = allowedFileFormat.Split('/');
            if (!fileFormats.Contains(imageFileFormat))
            {
                throw new InvalidImageFormatException(allowedFileFormat.ToString());
            }
        }
    }
}
