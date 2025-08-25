using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UMA.Application.Interfaces;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Exceptions.ImageUpload;
using UMA.Domain.Exceptions.Extensions;

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
                Log.Information("RequestBody : {@request}", request);

                var result = await _userService.GetUserAsync(request);

                Log.Information("User {email} has been created", result.User.Email);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return NotFound(ex.Message);
            }
            catch (UnauthorizedUserException ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return NotFound(ex.Message);
            }

        }

        [HttpPost("profile")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                Log.Information("RequestBody : {@request}", request);

                var result = await _userService.CreateUserAsync(request);

                Log.Information("User {email} has been created", request.Email);

                return Ok(result);
            }
            catch (EmailAlreadyExistsException ex)
            {
                Log.Error(ex, $"Email : {request.Email} \n {ex.Message}");

                return Conflict(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                Log.Information("RequestBody : {@request}", request);

                await _userService.UpdateUserAsync(request);

                Log.Information("User {email} has been created", request.Email);

                return Ok();
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"UserID : {request.UserID} \n {ex.Message}");

                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex, $"UserID : {request.UserID} \n {ex.Message}");

                return Unauthorized(ex.Message);
            }
        }


        [HttpPost("profile-picture")]
        public async Task<ActionResult<UserResponse>> UpdateProfilePicture([FromForm] UploadPictureRequest request)
        {
            try
            {
                Log.Information("RequestBody : {@request}", request);

                var result = await _userService.UploadProfilePictureAsync(request);

                Log.Information("User {UserID} has updated profile picture", result.UserID);

                return Ok(result.ProfilePictureUrl);
            }
            catch (UnauthorizedUserException ex)
            {
                Log.Error(ex, $"UserID : {request.UserID} \n {ex.Message}");

                return Unauthorized(ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"{request.UserID} \n {ex.Message} ");

                return NotFound(ex.Message);
            }           
            catch (InvalidImageFormatException ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return BadRequest(ex.Message);
            }
            catch (InvalidImageSizeException ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return BadRequest(ex.Message);
            }
            catch (AzureStorageException ex)
            {
                Log.Error(ex, $"UserID :  {request.UserID}  \n  {ex.Message}");

                return BadRequest(ex.Message);
            }
        }

    }
}
