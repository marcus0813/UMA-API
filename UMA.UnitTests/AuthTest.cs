using Moq;
using Microsoft.Extensions.Configuration;
using UMA.Application.Interfaces;
using UMA.Domain.Services;
using UMA.Domain.Repositories;
using UMA.Domain.Entities;
using UMA.Infrastructure.Services;
using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Response;
using UMA.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace UMA.UnitTests
{
    public class AuthTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _config;

        private readonly DateTime loginTime = DateTime.UtcNow;

        public AuthTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _passwordHasherServiceMock = new Mock<IPasswordHasherService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Theory]
        [InlineData("886419C0-69AB-4BEE-91B0-18A707632432", "marcus.kok@email.com")]
        public void GenerateAccessToken_EmailAndUserID_ReturnValidJwtToken(Guid userID, string email)
        {

            SetupHttpAccessorMock(userID.ToString(), email);

            var jwtTokenService = new JwtTokenService(_config, _httpContextAccessor.Object);

            var token = jwtTokenService.GenerateAccessToken(userID, email, loginTime);

            Assert.NotNull(token);
            Assert.IsType<string>(token);
        }

        [Theory]
        [InlineData("886419C0-69AB-4BEE-91B0-18A707632432", "marcus.kok@email.com")]
        public void GenerateRefreshToken_EmailAndUserID_ReturnValidJwtToken(Guid userID, string email)
        {

            var jwtTokenService = new JwtTokenService(_config, _httpContextAccessor.Object);

            var token = jwtTokenService.GenerateRefreshToken(userID, email, loginTime);

            Assert.NotNull(token);
            Assert.IsType<string>(token);
        }

        [Theory]
        [InlineData("marcus.kok@email.com", "marcuskok123")]
        public async Task VerifyLogin_EmailAndPwd_ReturnValidJwtTokenAsync(string email, string password)
        {
            Guid userID = new Guid("4C13BDF2-12CB-486D-BD2C-24C23D916A6D");
            User user = new User
            {
                ID = userID,
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$LB9HbCHkEleGIDuoRrpyGe58krqx4bMjIF.a5SJWqPaRy7saOFhoi",
                ProfilePictureUrl = "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg",

            };
            _userRepositoryMock.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _passwordHasherServiceMock.Setup(s => s.Verify(password, user.Password)).Returns(true);

            SetupHttpAccessorMock(userID.ToString(), email);

            JwtTokenDto expectedToken = new JwtTokenDto();
            var jwtTokenService = new JwtTokenService(_config, _httpContextAccessor.Object);
            expectedToken = jwtTokenService.GenerateAccessToken(userID, email, loginTime);
            _jwtTokenServiceMock.Setup(s => s.GenerateAccessToken(userID, email, loginTime)).Returns(expectedToken);

            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwtTokenServiceMock.Object);

            var result = await authService.VerifyLogin(email, password);

            Assert.NotNull(result);
            Assert.IsType<LoginResponse>(result);
        }

        [Theory]
        [InlineData("invalid.email@email.com", "invalid123")] //User not found
        [InlineData("marcus.kok@email.com", "invalid123")] //Invalid Credentials
        public async Task VerifyLogin_EmailAndPwd_ThrowExceptions(string email, string password)
        {
            Guid userID = new Guid("4C13BDF2-12CB-486D-BD2C-24C23D916A6D");
            User user = new User
            {
                ID = userID,
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$LB9HbCHkEleGIDuoRrpyGe58krqx4bMjIF.a5SJWqPaRy7saOFhoi",
                ProfilePictureUrl = "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg",

            };
            _userRepositoryMock.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _passwordHasherServiceMock.Setup(s => s.Verify(password, user.Password)).Returns(true);

            SetupHttpAccessorMock(userID.ToString(), email);
            JwtTokenDto expectedToken = new JwtTokenDto();
            var jwtTokenService = new JwtTokenService(_config,_httpContextAccessor.Object );
            expectedToken = jwtTokenService.GenerateAccessToken(userID, email, loginTime);
            _jwtTokenServiceMock.Setup(s => s.GenerateRefreshToken(userID, email, loginTime)).Returns(expectedToken);

            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwtTokenServiceMock.Object);

            var result = await authService.VerifyLogin(email, password);

            Assert.IsNotType<LoginResponse>(result);
            //Assert.Throws(result);

        }

        private void SetupHttpAccessorMock(string userID, string email)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userID),
            new Claim(ClaimTypes.Email, email)
        };
            var identity = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.NameId, userID),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                });

            var user = new ClaimsPrincipal(identity);

            _httpContextAccessor.Setup(accessor => accessor.HttpContext.User).Returns(user);
        }
    }
}

