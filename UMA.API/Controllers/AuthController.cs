using Microsoft.AspNetCore.Mvc;
using UMA.Application.Interfaces;
using UMA.Shared.DTOs.Request;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
                var result = await _authService.VerifyLogin(request.Email, request.Password);

                Log.Information("{Email} logged in", request.Email);

                return Ok(result);
            }
            catch (UserNotFoundException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.Email}");

                return NotFound(ex.Message);
            }
            catch (InvalidCrendentialsException ex)
            {
                Log.Error(ex, $"{ex.Message} {request.Email}");

                return Unauthorized(ex.Message);
            }

        }
    }
}
