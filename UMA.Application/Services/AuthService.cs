using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UMA.Application.Interfaces;
using UMA.Domain.Entities;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Repositories;
using UMA.Domain.Services;
using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Request;

namespace UMA.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwTokenService _jwTokenService;

        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository userRepository, IPasswordHasherService passwordHasher, IJwTokenService jwTokenService,IConfiguration config ,IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwTokenService = jwTokenService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TokenResponse> VerifyLogin(LoginRequest request)
        {
            //Check user exists, throw exception if user not found
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            //Hashing password to check if matched with password from DB
            //throw exception if not matched
            if (!_passwordHasher.Verify(request.Password, user.Password))
            {
                throw new InvalidCrendentialsException();
            }

            //Return both access and refresh token as token response
            return await GenerateToken(user, request.LoginTime);
        }

        public async Task<TokenResponse> RefreshAcess(RefreshRequest request)
        {
            //Check if both user ID and email are valid from token
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var jwTokenIDFromToken = currentUser.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userIDFromToken = currentUser.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var emailFromToken = currentUser.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            if (jwTokenIDFromToken == null || userIDFromToken == null || emailFromToken == null)
            { 
                throw new UnauthorizedUserException();
            }

            //Check user exists, throw exception if user not found
            var user = await _userRepository.GetUserByIDAsync(new Guid(userIDFromToken));
            var emailExists = await _userRepository.GetUserByEmailAsync(emailFromToken);
            if (user == null || emailExists == null)
            {
                throw new UserNotFoundException();
            }

            //Check if request token is matched with refresh token from DB
            var refreshTokenFromDB = user.RefreshToken;
            if (refreshTokenFromDB != request.Token)
            { 
                throw new UnauthorizedUserException();
            }

            //Get expiry date from refresh token, check refresh token expires, throw exception token expired
            var refreshToken = _jwTokenService.GetRefreshTokenValue(request.Token);
            if (refreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                throw new TokenExpiredException();
            }

            //Return both access and refresh token as token response
            return await GenerateToken(user, request.LoginTime);
        }

        private async Task<TokenResponse> GenerateToken(User user, DateTime loginTime)
        {
            //Generate refresh token and store in DB
            string refreshToken = _jwTokenService.GenerateRefreshToken(user.ID, user.Email, loginTime);
            user.RefreshToken = refreshToken;
            await _userRepository.UpdateUserAsync(user);

            //Get Value from refresh Token
            TokenDto refreshTokenDto = _jwTokenService.GetRefreshTokenValue(refreshToken); 

            //Generate access token
            string accessToken = _jwTokenService.GenerateAccessToken(user.ID, user.Email, loginTime, refreshTokenDto.JwTokenID);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public void SetTokenIntoCookies(HttpContext context, TokenResponse token)
        {
            int tokenValidityMinutes = _config.GetValue<int>("Jwt:TokenValidityMinutes");
            context.Response.Cookies.Append("accessToken", token.AccessToken,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(tokenValidityMinutes),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                }
            );

            int refreshTokenExpiryDays = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays");
            context.Response.Cookies.Append("refreshToken", token.RefreshToken, 
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                }
            );
        }
    }
}
