using Microsoft.AspNetCore.Mvc;
using UMA.Application.Interfaces;
using UMA.Shared.DTOs.Request;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
using Serilog;
using System.Text.Json;
using UMA.Shared.DTOs.Common;

namespace UMA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Log.Information("RequestBody : {@request}", request);

                var result = await _authService.VerifyLogin(request);

                Log.Information("{email} logged in", request.Email);
                
                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"Email : {request.Email} \n {ex.Message}");

                return NotFound(ex.Message);
            }
            catch (InvalidCrendentialsException ex)
            {
                Log.Error(ex, $"Email : {request.Email} \n {ex.Message}");

                return Unauthorized(ex.Message);
            }

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccess([FromBody] RefreshRequest request)
        {
            try
            {
                Log.Information("RequestBody : {@request}", request);

                var result = await _authService.RefreshAcess(request);

                Log.Information("Token Refresh : {@tokens}", result.RefreshToken);
                
                return Ok(result);
            }
            catch (UnauthorizedUserException ex)
            {
                Log.Error(ex, "RequestBody : {@request} \n {errMsg} ", request, ex.Message);

                return Unauthorized(ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, "RequestBody : {@request} \n {errMsg} ", request, ex.Message);

                return NotFound(ex.Message);
            }
            catch (TokenExpiredException ex)
            {
                Log.Error(ex, "RequestBody : {@request} \n {errMsg} ", request, ex.Message);

                return NotFound(ex.Message);
            }

        }
    }
}
