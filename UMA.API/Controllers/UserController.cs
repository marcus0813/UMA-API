using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UMA.Application.Interfaces;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Exceptions.ImageUpload;
using UMA.Domain.Exceptions.Extensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UMA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserResponse>> GetUser([FromQuery] GetUserRequest request)
        {
            try
            {
                var result = await _userService.GetUserAsync(request);

                Log.Information("User {Email} has been created", result.Email);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return NotFound(ex.Message);
            }
            catch (UnauthorizedUserException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return NotFound(ex.Message);
            }

        }

        [HttpPost("profile")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var result = await _userService.CreateUserAsync(request);

                Log.Logger.Information("User {Email} has been created", result.Email);

                return Ok(result);
            }
            catch (EmailAlreadyExistsException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.Email}");

                return Conflict(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(request);

                Log.Information("User {Email} has been created", result.Email);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return Unauthorized(ex.Message);
            }
        }


        [HttpPost("profile-picture")]
        public async Task<ActionResult<UserResponse>> UpdateProfilePicture([FromForm] UploadPictureRequest request)
        {
            try
            {
                var result = await _userService.UploadProfilePictureAsync(request);

                Log.Information("User {UserID} has updated profile picture", request.UserID);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return NotFound(ex.Message);
            }           
            catch (InvalidImageFormatException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return BadRequest(ex.Message);
            }
            catch (InvalidImageSizeException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return BadRequest(ex.Message);
            }
            catch (AzureStorageException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.UserID}");

                return BadRequest(ex.Message);
            }
        }

    }
}
