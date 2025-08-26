using Microsoft.AspNetCore.Mvc;
using Serilog;
using UMA.Application.Interfaces;
using UMA.Shared.DTOs.Request;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
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

                _authService.SetTokenIntoCookies(HttpContext, result);

                Log.Information("{email} logged in", request.Email);
                
                return Ok(result.AccessToken);
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

        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshAccess()
        {
            try
            {
                // Get the cookie from the request
                if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                {
                    throw new TokenExpiredException();
                }

                RefreshRequest request = new RefreshRequest
                {
                    Token = refreshToken
                };

                var result = await _authService.RefreshAcess(request);

                //remove cookies once trigger
                _authService.SetTokenIntoCookies(HttpContext, result, true);

                Log.Information("Token Refresh : {@tokens}", result.RefreshToken);
                
                return Ok(result.AccessToken);
            }
            catch (UnauthorizedUserException ex)
            {
                Log.Error(ex, "RequestBody : {@request} \n {errMsg} ", ex.Message);

                return Unauthorized(ex.Message);
            }
            catch (TokenExpiredException ex)
            {
                Log.Error(ex, "RequestBody : {@request} \n {errMsg} ", ex.Message);

                return NotFound(ex.Message);
            }

        }
    }
}
