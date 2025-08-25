using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UMA.Application.Services;
using UMA.Domain.Entities;
using UMA.Domain.Exceptions.User;
using UMA.Domain.Exceptions.ImageUpload;
using UMA.Domain.Repositories;
using UMA.Domain.Services;
using UMA.Infrastructure.Services;
using UMA.Shared.DTOs.Common;
using UMA.Shared.DTOs.Request;
using UMA.Shared.DTOs.Response;

namespace UMA.UnitTests
{
    public class UserTest
    {
        private readonly Mock<IAzureBlobStorageService> _azureBlobStorageService;
        private readonly Mock<IJwTokenService> _jwTokenServiceMock;
        private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _config;
        
        public UserTest()
        {
            _azureBlobStorageService = new Mock<IAzureBlobStorageService>();
            _jwTokenServiceMock = new Mock<IJwTokenService>();
            _passwordHasherServiceMock = new Mock<IPasswordHasherService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Theory]
        [InlineData("FA7CFBB5-911A-418D-A39D-101DBD10C00B", "marcuskok123", "marcus.kok@email.com", "FA7CFBB5-911A-418D-A39D-101DBD10C00B", "Marcus", "Kok", "marcus.kok@email.com", "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        [InlineData("72F04621-E341-453D-B596-7427DA8BDD98", "alfredkok123", "alfred.kok@email.com", "72F04621-E341-453D-B596-7427DA8BDD98", "Alfred", "Kok", "alfred.kok@email.com", "$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        public async Task GetUserAsync_UserBodyParams_ReturnCreatedUser(string userID, string password,string email, string expectedUserID, string expectedFirstName, string expectedLastName, string expectedEmail, string expectedPassword, string expectedProfilePhotoUrl) 
        {
            //Setup request body params
            GetUserRequest request = new GetUserRequest{
                UserID = new Guid(userID),
                Email  = email
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid(expectedUserID),
                FirstName = expectedFirstName,
                LastName = expectedLastName,
                Email = expectedEmail,
                Password = expectedPassword,
                ProfilePictureUrl = expectedProfilePhotoUrl,
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, new Guid(expectedUserID), expectedEmail, password, expectedProfilePhotoUrl);

            //Bypass claims checking authentication error
            BypassClaimsVerification(new Guid(userID), email);
            
            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Get user detail from DB 
            var result = await userService.GetUserAsync(request); 

            //Result shall be not null,user response type and same user info with expected user 
            Assert.NotNull(result);
            Assert.IsType<UserResponse>(result);
            Assert.Equal(expectedFirstName, result.User.FirstName);
            Assert.Equal(expectedLastName, result.User.LastName);
            Assert.Equal(email, result.User.Email);
        }

        [Theory]
        [InlineData("B2C533B5-1A18-49D8-9714-241A793BF1C2", "marcuskok123", "marcus.kok@email.com", "FA7CFBB5-911A-418D-A39D-101DBD10C00B", "Marcus", "Kok", "marcus.kok@email.com", "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        [InlineData("8E76B2D7-D4EB-455A-AFF3-12F5B1385979", "alfredkok123", "alfred.kok@email.com", "72F04621-E341-453D-B596-7427DA8BDD98", "Alfred", "Kok", "alfred.kok@email.com", "$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        public async Task GetUserAsync_UserBodyParams_ThrowNotFoundException(string userID, string password, string email, string expectedUserID, string expectedFirstName, string expectedLastName, string expectedEmail, string expectedPassword, string expectedProfilePhotoUrl)
        {
            //Setup request body params
            GetUserRequest request = new GetUserRequest
            {
                UserID = new Guid(userID),
                Email = email
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid(expectedUserID),
                FirstName = expectedFirstName,
                LastName = expectedLastName,
                Email = expectedEmail,
                Password = expectedPassword,
                ProfilePictureUrl = expectedProfilePhotoUrl,
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, new Guid(expectedUserID), expectedEmail, password, expectedProfilePhotoUrl);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Get user detail from DB, return user not found exception due to invalid user ID
            var exception = await Assert.ThrowsAsync<UserNotFoundException>(async () => await userService.GetUserAsync(request));

            //Result shall be same with the user not found exception message
            Assert.Equal("User not exists.", exception.Message);
        }

        [Theory]
        [InlineData("FA7CFBB5-911A-418D-A39D-101DBD10C00B", "marcuskok123", "marcus.kok@email.com", "FA7CFBB5-911A-418D-A39D-101DBD10C00B", "Marcus", "Kok", "marcus.kok@email.com", "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        [InlineData("72F04621-E341-453D-B596-7427DA8BDD98", "alfredkok123", "alfred.kok@email.com", "72F04621-E341-453D-B596-7427DA8BDD98", "Alfred", "Kok", "alfred.kok@email.com", "$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2", "https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg")]
        public async Task GetUserAsync_UserBodyParams_ThrowUnauthorizedUserException(string userID, string password, string email, string expectedUserID, string expectedFirstName, string expectedLastName, string expectedEmail, string expectedPassword, string expectedProfilePhotoUrl)
        {
            //Setup request body params
            GetUserRequest request = new GetUserRequest
            {
                UserID = new Guid(userID),
                Email = email
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid(expectedUserID),
                FirstName = expectedFirstName,
                LastName = expectedLastName,
                Email = expectedEmail,
                Password = expectedPassword,
                ProfilePictureUrl = expectedProfilePhotoUrl,
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, new Guid(expectedUserID), expectedEmail, password, expectedProfilePhotoUrl);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Get user detail from DB, return user not found exception due to token claims invalid
            var exception = await Assert.ThrowsAsync<UnauthorizedUserException>(async () => await userService.GetUserAsync(request));

            //Result shall be same with the user not found exception message
            Assert.Equal("Unauthorized User, please try login again.", exception.Message);
        }


        [Theory]
        [InlineData("Testuser", "One", "testuser1@email.com", "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy")]
        [InlineData("Testuser", "Two", "testuserr2@email.com", "$2b$10$mLLxSd4kZ8iCe.kfHpjBg.Ut86nral23eMlb45zshKHq10dlQQVL2")]
        public async Task CreateUserAsync_UserBodyParams_ReturnTokenResponse(string firstName, string lastName, string email, string password)
        {
            //Setup request body params
            CreateUserRequest request = new CreateUserRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
            };

            //No User found in DB, all will be empty
            User expectedUser = new User
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, email, password, string.Empty);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Create user then return both access and refresh token
            var result = await userService.CreateUserAsync(request);

            //Result shall be not null,token response type and both access and refresh token shalln't be null
            Assert.NotNull(result);
            Assert.IsType<TokenResponse>(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
        }

        [Fact]
        public async Task CreateUserAsync_UserBodyParams_ThrowEmailAlreadyExistsException()
        {
            //Set Values
            string firstName = "Test";
            string lastName = "User";
            string email = "marcus.kok@email.com";
            string existedEmail = email;
            string password = "marcuskok123";
            string hashedPassword  = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy";

            //Setup request body params
            CreateUserRequest request = new CreateUserRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
            };

            //Setup expected user
            User expectedUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = existedEmail,
                Password = hashedPassword,
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, password, string.Empty);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Create user, will throw email already exists execption due to email already exists 
            var exception = await Assert.ThrowsAsync<EmailAlreadyExistsException>(async () => await userService.CreateUserAsync(request));

            //Result shall be same with the email already exists exception message
            Assert.Equal("Email already exists.", exception.Message);
        }

        [Theory]
        [InlineData("Danish", "Legend", "danishlegend123")]
        [InlineData("Michael", "Lebron", "michaellebron123")]
        public async Task UpdateUserAsync_UserBodyParams_ReturnUpdatedUser(string firstName, string lastName, string password)
        {
            //Set Values
            Guid userID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B");
            string email = "marcus.kok@email.com";

            //Setup request body params
            UpdateUserRequest request = new UpdateUserRequest
            {
                UserID = userID,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = userID,
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, password, string.Empty);

            //Bypass claims checking authentication error
            BypassClaimsVerification(userID, email);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);
            
            //Update user editable info then return token
            await userService.UpdateUserAsync(request);
        }

        [Fact]
        public async Task UpdateUserAsync_UserBodyParams_ThrowUserNotFoundException()
        {
            //Set Values
            Guid userID = new Guid();  //Fake User ID
            string firstName = "Danish";
            string lastName = "Marcus"; 
            string email = "marcus.kok@email.com";
            string password = "danishmarcus123";

            //Setup request body params
            UpdateUserRequest request = new UpdateUserRequest
            {
                UserID = userID,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, password, string.Empty);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Create user then throw user not found exception due to user not found
            var exception = await Assert.ThrowsAsync<UserNotFoundException>(async () => await userService.UpdateUserAsync(request));

            //Result shall be same with the user not found exception message
            Assert.Equal("User not exists.", exception.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_UserBodyParams_ThrowUnauthorizedUserException()
        {
            //Set Values
            Guid userID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B");  //Fake User ID
            string firstName = "Danish";
            string lastName = "Marcus";
            string email = "marcus.kok@email.com";
            string password = "danishmarcus123";

            //Setup request body params
            UpdateUserRequest request = new UpdateUserRequest
            {
                UserID = userID,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = userID,
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, password, string.Empty);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Create user then throw unauthorized user exception due to token claims invalid
            var exception = await Assert.ThrowsAsync<UnauthorizedUserException>(async () => await userService.UpdateUserAsync(request));

            //Result shall be same with the unauthorized user exception message
            Assert.Equal("Unauthorized User, please try login again.", exception.Message);
        }

        [Theory]
        [InlineData("png",1 , "FA7CFBB5-911A-418D-A39D-101DBD10C00B", "marcus.kok@email.com", "http://wwww.images.com/marcus.png")]
        [InlineData("jpg",2 , "72F04621-E341-453D-B596-7427DA8BDD98", "alfred.kok@email.com", "http://wwww.images.com/alfred.png")]
        public async Task UploadProfileAsync_UserBodyParams_ReturnUrlString(string fileFormat, int fileSize, string userIDString, string email, string profilePictureUrl)
        {
            //Set Values
            Guid userID = new Guid(userIDString);

            //Set Mock Image file with valid params
            Mock<IFormFile> imageFile = SetupFakeIFormFileMock(fileFormat, fileSize);

            UploadPictureRequest request = new UploadPictureRequest
            {
                ProfilePicture = imageFile.Object,
                UserID = userID,
                Email= email,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = userID,
                FirstName = "Marcus",
                LastName = "Kok",
                Email = email,
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
                ProfilePictureUrl = profilePictureUrl
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, string.Empty, string.Empty);

            //Bypass claims checking authentication error
            BypassClaimsVerification(userID, email);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Update user editable info then return token
            var result = await userService.UploadProfilePictureAsync(request);

           Assert.NotNull(result);
           Assert.Equal("", result.ProfilePictureUrl);
        }

        [Fact]
        public async Task UploadProfileAsync_UserBodyParams_ThrowUserNotFoundException()
        {
            //Set Values
            Guid userID = Guid.NewGuid(); //Invalid UserID
            string email = "marcus.kok@email.com";
            string fileFormat = "PNG";
            int fileSize = 1; 

            //Set Mock Image file with valid params
            Mock<IFormFile> imageFile = SetupFakeIFormFileMock(fileFormat, fileSize);

            UploadPictureRequest request = new UploadPictureRequest
            {
                ProfilePicture = imageFile.Object,
                UserID = userID,
                Email = email,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, string.Empty, string.Empty);

            //Bypass claims checking authentication error
            BypassClaimsVerification(userID, email);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Upload profile picture then throw user not found exception due to invalid user ID
            var exception = await Assert.ThrowsAsync<UserNotFoundException>(async () => await userService.UploadProfilePictureAsync(request));

            //Result shall be same with the user not found exception message
            Assert.Equal("User not exists.", exception.Message);
        }

        [Fact]
        public async Task UploadProfileAsync_UserBodyParams_ThrowInvalidImageFormatException()
        {
            //Set Values
            Guid userID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B");
            string email = "marcus.kok@email.com";
            string fileFormat = "GIF"; //Invalid Param
            int fileSize = 1; 

            //Set Mock Image file with valid params
            Mock<IFormFile> imageFile = SetupFakeIFormFileMock(fileFormat, fileSize);

            //Setup request body params
            UploadPictureRequest request = new UploadPictureRequest
            {
                ProfilePicture = imageFile.Object,
                UserID = userID,
                Email = email,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, string.Empty, string.Empty);

            //Bypass claims checking authentication error
            BypassClaimsVerification(userID, email);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Upload profile picture then throw invalid file format exception due to request image file format not allowed
            var exception = await Assert.ThrowsAsync<InvalidImageFormatException>(async () => await userService.UploadProfilePictureAsync(request));

            //Result shall be same with the invalid file format exception message
            Assert.Equal("Only JPG/JPEG/PNG Format is allowed.", exception.Message);
        }

        [Fact]
        public async Task UploadProfileAsync_UserBodyParams_ThrowInvalidImageSizeException()
        {
            //Set Values
            Guid userID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B");
            string email = "marcus.kok@email.com";
            string fileFormat = "PNG";
            int fileSize = 4; //Invalid Param

            //Set Mock Image file with valid params
            Mock<IFormFile> imageFile = SetupFakeIFormFileMock(fileFormat, fileSize);

            UploadPictureRequest request = new UploadPictureRequest
            {
                ProfilePicture = imageFile.Object,
                UserID = userID,
                Email = email,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, string.Empty, string.Empty);

            //Bypass claims checking authentication error
            BypassClaimsVerification(userID, email);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Upload profile picture then throw invalid image file size exception due to file size exceeds 2MB 
            var exception = await Assert.ThrowsAsync<InvalidImageSizeException>(async () => await userService.UploadProfilePictureAsync(request));

            //Result shall be same with the invalid image file size exception message
            Assert.Equal("Image size exceeds the limit of 2MB.", exception.Message);
        }
        [Fact]
        public async Task UploadProfileAsync_UserBodyParams_ThrowUnauthorizedUserException()
        {
            //Set Values
            Guid userID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B");
            string email = "marcus.kok@email.com";
            string fileFormat = "PNG";
            int fileSize = 1; 

            //Set Mock Image file with valid params
            Mock<IFormFile> imageFile = SetupFakeIFormFileMock(fileFormat, fileSize);

            UploadPictureRequest request = new UploadPictureRequest
            {
                ProfilePicture = imageFile.Object,
                UserID = userID,
                Email = email,
            };

            //Setup expected user
            User expectedUser = new User
            {
                ID = new Guid("FA7CFBB5-911A-418D-A39D-101DBD10C00B"),
                FirstName = "Marcus",
                LastName = "Kok",
                Email = "marcus.kok@email.com",
                Password = "$2b$10$mZbjZH1tGibllQ6.SF3Mm.Ms6q0gGexpRgbaLynDAjMvrUCzCJLoy",
            };

            //Setup all mock services
            SetupAllMockServices(expectedUser, expectedUser.ID, expectedUser.Email, string.Empty, string.Empty);

            //Create instance using config and mock services 
            var userService = new UserService(_userRepositoryMock.Object, _azureBlobStorageService.Object, _passwordHasherServiceMock.Object, _jwTokenServiceMock.Object, _config);

            //Upload profile picture then throw unauthorized user exception due to token claims invalid 
            var exception = await Assert.ThrowsAsync<UnauthorizedUserException>(async () => await userService.UploadProfilePictureAsync(request));

            //Result shall be same with the unauthorized user exception message
            Assert.Equal("Unauthorized User, please try login again.", exception.Message);
        }


        private void SetupAllMockServices(User user, Guid userID, string email, string password, string profilePictureUrl)
        {
            //Setup Twt Service mock 1st, to get refresh token
            SetupJwTokenServiceMock(userID, email);

            //Set the user refresh token, so wont trigger unauthorized exception
            user.RefreshToken = _jwTokenServiceMock.Object.GenerateRefreshToken(user.ID, email ,DateTime.UtcNow);

            //Proceed to create mock services
            SetupHttpAccessorMock(userID, email, user.RefreshToken);
            SetupUserRepositoryMock(user);
            SetupPasswordHasherServiceMock(password, user.Password);
            SetupAzureBlobStorageService(profilePictureUrl);
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
            _jwTokenServiceMock.Setup(s => s.GenerateRefreshToken(It.IsAny<Guid>(), email, It.IsAny<DateTime>())).Returns(expectedRefreshToken);

            //Decode refresh token to get jwtokenID
            var refreshToken = jwTokenService.GetRefreshTokenValue(expectedRefreshToken);

            //Set expected result for access token
            expectedAccessToken = jwTokenService.GenerateAccessToken(userID, email, DateTime.UtcNow, refreshToken.JwTokenID);

            //Setup mock service for expected input and output
            _jwTokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<Guid>(), email, It.IsAny<DateTime>(), refreshToken.JwTokenID)).Returns(expectedAccessToken);
            _jwTokenServiceMock.Setup(s => s.GetRefreshTokenValue(expectedRefreshToken))
                               .Returns(new TokenDto
                                           {
                                               JwTokenID = refreshToken.JwTokenID,
                                               ExpiryDate = refreshToken.ExpiryDate,
                                           }
                                        );
        }

        private void SetupHttpAccessorMock(Guid userID, string email, string refreshToken)
        {
            //mock jwTokenID for testing purpose
            //Get jwTokenID from generated refresh token
            JwTokenService jwTokenService = new JwTokenService(_config, _httpContextAccessor.Object);
            TokenDto refreshTokenDto = jwTokenService.GetRefreshTokenValue(refreshToken);

            //Set values for claims, simulating real token claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, refreshTokenDto.JwTokenID),
                new Claim(JwtRegisteredClaimNames.Sub, userID.ToString()),
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
            //Setup mock service for expected input and output
            _passwordHasherServiceMock.Setup(s => s.Verify(password, hashedPassword)).Returns(true);
        }     

        private Mock<IFormFile> SetupFakeIFormFileMock(string fileFormat, int fileSize)
        {
            //Set values for claims and create mock IFormFile, simulating real Image file
            var mockFile = new Mock<IFormFile>();

            //Set IFormFile properties, based on provided file format and file size
            var fileName = $"test.{ fileFormat }";
            var fileSizeInMb = fileSize;
            var fileBytes = new byte[fileSizeInMb * 1024 * 1024];
            var ms = new MemoryStream(fileBytes);

            //Setup mock service for expected input and output
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.ContentType).Returns($"image/{fileFormat}");

            return mockFile;
        }

        private void SetupAzureBlobStorageService(string profilePictureUrl)
        {
            //Setup mock service for expected input and output
            _azureBlobStorageService.Setup(s => s.UploadFilesAsync(It.IsAny<IFormFile>())).ReturnsAsync(new BlobDto{ 
                Uri = profilePictureUrl,
            });
        }

        private void BypassClaimsVerification(Guid userID, string email)
        {
            //Setup mock service for VerifyUserTokenClaims method, bypass authentication error
            
            //Get expected refresh token
            string refreshTokenString = _jwTokenServiceMock.Object.GenerateRefreshToken(userID, email, DateTime.UtcNow);

            //Verify token claims based on mocked services refresh token 
            JwTokenService jwTokenService = new JwTokenService(_config, _httpContextAccessor.Object);
            bool verifyResult = jwTokenService.VerifyUserTokenClaims(userID, email, refreshTokenString);

            //Setup mock service for expected input and output
            _jwTokenServiceMock.Setup(s => s.VerifyUserTokenClaims(userID, email, refreshTokenString)).Returns(verifyResult);
        }
    }
}
