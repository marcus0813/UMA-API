using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using UMA.Domain.Services;
using UMA.Domain.Repositories;
using UMA.Domain.Entities;
using UMA.Domain.Exceptions.Login;
using UMA.Domain.Exceptions.User;
using UMA.Infrastructure.Services;
using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;
using UMA.Application.Services;
namespace UMA.UnitTests
{
    public class AuthTest
    {
        private readonly Mock<IJwTokenService> _jwTokenServiceMock;
        private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _config;

        public AuthTest()
        {
            _jwTokenServiceMock = new Mock<IJwTokenService>();
            _passwordHasherServiceMock = new Mock<IPasswordHasherService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Theory]
        [InlineData("marcus.kok@email.com", "marcuskok123", "FA7CFBB5-911A-418D-A39D-101DBD10C00B", "Marcus", "Kok", "marcus.kok@email.com","$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        [InlineData("alfred.kok@email.com", "alfredkok123", "72F04621-E341-453D-B596-7427DA8BDD98", "Alfred", "Kok", "alfred.kok@email.com","$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        public async Task VerifyLogin_EmailAndPwd_ReturnValidJwTokenAsync(string email, string password, string userID, string firstName, string lastName,string emailFromDB, string hashedPassword, string profilePictureUrl)
        {
            //Setup request body params
            LoginRequest request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            //Set Existing User
            User user = new User
            {
                ID = new Guid(userID),
                FirstName = firstName,
                LastName = lastName,
                Email = emailFromDB,
                Password = hashedPassword,
                ProfilePictureUrl = profilePictureUrl,

            };

            //Setup all Mock Services
            SetupAllMockServices(user, new Guid(userID), emailFromDB, password);

            //Create instance using config and mock services 
            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config, _httpContextAccessor.Object);

            //Verify user existence and credentials, return both access and refresh token
            var result = await authService.VerifyLogin(request);

            //Result shall be not null, token response type, not empty for both access and refresh token
            Assert.NotNull(result);
            Assert.IsType<TokenResponse>(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
        }

        [Fact]
        public async Task VerifyLogin_EmailAndPwd_ThrowUserNotFoundException()
        {
            //Set values
            string email = "invalid.email@email.com"; //invalid param
            string password = "invalid123";

            //Setup request body params
            LoginRequest request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            //Setup user 
            User user = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$LB9HbCHkEleGIDuoRrpyGe58krqx4bMjIF.a5SJWqPaRy7saOFhoi",
                ProfilePictureUrl = "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg",
            };

            //Setup all Mock Services
            SetupAllMockServices(user, user.ID, user.Email, password);

            //Create instance using config and mock services 
            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config, _httpContextAccessor.Object);

            //Verify user existence and credentials, throw user not found exception due to invalid email
            var exception = await Assert.ThrowsAsync<UserNotFoundException>(async () => await authService.VerifyLogin(request));

            //Result shall be same with the user not found exception message
            Assert.Equal("User not exists.", exception.Message); 
        }

        [Fact]
        public async Task VerifyLogin_EmailAndPwd_ThrowInvalidCredentialsExceptions()
        {
            //Set values
            string email = "marcus.kok@email.com";
            string password = "invalid123"; //invalid param

            //Setup request body params
            LoginRequest request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            //Setup user 
            User user = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$LB9HbCHkEleGIDuoRrpyGe58krqx4bMjIF.a5SJWqPaRy7saOFhoi",
                ProfilePictureUrl = "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg",

            };

            //Setup all Mock Services
            SetupAllMockServices(user, user.ID, email, password);

            //Create instance using config and mock services 
            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config, _httpContextAccessor.Object);

            //Verify user existence and credentials, throw invalid creds exception due to invalid passsword
            var exception = await Assert.ThrowsAsync<InvalidCrendentialsException>(async () => await authService.VerifyLogin(request));

            //Result shall be same with the invalid creds exception message
            Assert.Equal("Invalid Credentials, please try again.", exception.Message);
        }

        [Theory]
        [InlineData("FA7CFBB5-911A-418D-A39D-101DBD10C00B", "Marcus", "Kok", "marcus.kok@email.com", "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy")]
        [InlineData("72F04621-E341-453D-B596-7427DA8BDD98", "Alfred", "Kok", "alfred.kok@email.com", "$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2")]
        public async Task RefreshAccess_RefreshToken_ReturnValidJwTokenAsync(string userID, string firstName, string lastName, string emailFromDB, string hashedPassword)
        {
            //Set Existing User
            User user = new User
            {
                ID = new Guid(userID),
                FirstName = firstName,
                LastName = lastName,
                Email = emailFromDB,
                Password = hashedPassword,
            };

            //Setup all Mock Services
            SetupAllMockServices(user, new Guid(userID), emailFromDB, string.Empty);

            //Setup request body params
            RefreshRequest request = new RefreshRequest
            {
                Token = _jwTokenServiceMock.Object.GenerateRefreshToken(user.ID, user.Email, DateTime.UtcNow)
            };

            //Create instance using config and mock services 
            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config, _httpContextAccessor.Object);

            //Verify user existence and credentials, return both access and refresh token
            var result = await authService.RefreshAcess(request);

            //Result shall be not null, token response type, not empty for both access and refresh token
            Assert.NotNull(result);
            Assert.IsType<TokenResponse>(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
        }

        [Fact]
        public async Task RefreshAccess_RefreshToken_ThrowUnauthorizedUserExceptions()
        {
            //Set Existing User
            User user = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all Mock Services
            SetupAllMockServices(user, user.ID, user.Email, string.Empty);

            //Setup request body params
            RefreshRequest request = new RefreshRequest
            {
                Token = "fake refresh token"
            };

            //Create instance using config and mock services 
            var authService = new AuthService(_userRepositoryMock.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config, _httpContextAccessor.Object);

            //Verify user existence and credentials, throw unauthorized user exception due to invalid refresh token
            var exception = await Assert.ThrowsAsync<UnauthorizedUserException>(async () => await authService.RefreshAcess(request));

            //Result shall be same with the unauthorized user exception message
            Assert.Equal("Unauthorized User, please try login again.", exception.Message);
        }

        private void SetupAllMockServices(User user, Guid userID, string email, string password)
        {
            //Setup Twt Service mock 1st, to get refresh token
            SetupJwTokenServiceMock(userID, email);

            //Set the user refresh token from DB side, so wont trigger unauthorized exception
            user.RefreshToken = _jwTokenServiceMock.Object.GenerateRefreshToken(user.ID, user.Email, DateTime.UtcNow);

            //Proceed to create mock services
            SetupHttpAccessorMock(userID, email, user.RefreshToken);
            SetupUserRepositoryMock(user);
            SetupPasswordHasherServiceMock(password, user.Password);
        }

        private void SetupHttpAccessorMock(Guid userID, string email, string refreshToken)
        {
            //Get jwTokenID from generated refresh token
            JwTokenService jwTokenService = new JwTokenService(_config, _httpContextAccessor.Object);
            TokenDto refreshTokenDto = jwTokenService.GetRefreshTokenValue(refreshToken);

            //Set values for claims, simulating real token claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, refreshTokenDto.JwTokenID),
                new Claim(JwtRegisteredClaimNames.NameId, userID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email)
            };

            //Set expected claims identity
            var identity = new ClaimsIdentity(claims);

            //Set expected Return result 
            var user = new ClaimsPrincipal(identity);

            //Setup mock service for expected input and output
            _httpContextAccessor.Setup(accessor => accessor.HttpContext.User).Returns(user);
        }

        private void SetupUserRepositoryMock(User user)
        {
            //Setup mock service for expected input and output
            _userRepositoryMock.Setup(s => s.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            _userRepositoryMock.Setup(s => s.GetUserByIDAsync(user.ID)).ReturnsAsync(user);
        }

        private void SetupPasswordHasherServiceMock(string password, string hashedPassword)
        {
            //Verify login password and DB password, check whether is matched anot
            PasswordHasherService passwordHasherService = new PasswordHasherService();
            bool result = passwordHasherService.Verify(password,hashedPassword);

            //Setup mock service for expected input and output
            _passwordHasherServiceMock.Setup(s => s.Verify(password, hashedPassword)).Returns(result);
        }

        private void SetupJwTokenServiceMock(Guid userID, string email)
        {
            var expectedAccessToken = string.Empty;
            var expectedRefreshToken = string.Empty;

            //Create instance using config and mock services 
            var jwTokenService = new JwTokenService(_config, _httpContextAccessor.Object);

            //Set expected result for refresh token
            expectedRefreshToken = jwTokenService.GenerateRefreshToken(userID, email, DateTime.UtcNow);
            //Setup mock service for expected input and output
            _jwTokenServiceMock.Setup(s => s.GenerateRefreshToken(userID, email, It.IsAny<DateTime>())).Returns(expectedRefreshToken);

            //Decode refresh token to get jwtokenID
            var refreshToken = jwTokenService.GetRefreshTokenValue(expectedRefreshToken);

            //Set expected result for access token
            expectedAccessToken = jwTokenService.GenerateAccessToken(userID, email, DateTime.UtcNow, refreshToken.JwTokenID);
            
            //Setup mock service for expected input and output
            _jwTokenServiceMock.Setup(s => s.GenerateAccessToken(userID, email, It.IsAny<DateTime>(), refreshToken.JwTokenID)).Returns(expectedAccessToken);
            _jwTokenServiceMock.Setup(s => s.GetRefreshTokenValue(expectedRefreshToken))
                               .Returns(new TokenDto { 
                                                        JwTokenID = refreshToken.JwTokenID,
                                                        ExpiryDate= refreshToken.ExpiryDate,
                                                      }
                               );
        }
    }
}

