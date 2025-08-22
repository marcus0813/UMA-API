using UMA.Application.Interfaces;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Repositories;
using UMA.Domain.Services;
using UMA.Shared.DTOs.Common;

namespace UMA.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(IUserRepository userRepository, IPasswordHasherService passwordHasher, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<JwtTokenResponseDto> VerifyLogin(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            if (!_passwordHasher.Verify(password, user.Password))
            {
                throw new InvalidCrendentialsException();
            }

            DateTime loginTime = DateTime.UtcNow;
            JwtTokenDto accessToken = _jwtTokenService.GenerateAccessToken(user.ID, user.Email, loginTime);
            JwtTokenDto refershToken = _jwtTokenService.GenerateRefreshToken(user.ID, user.Email, loginTime);
            return new JwtTokenResponseDto 
            { 
                AccessToken = accessToken,
                RefreshToken = refershToken,
            };
        }
    }
}
