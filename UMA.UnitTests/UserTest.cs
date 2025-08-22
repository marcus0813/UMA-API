using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UMA.Domain.Entities;

namespace UMA.UnitTests
{
    public class UserTest
    {
        [Theory]
        [InlineData("")]
        public async Task CreateUserAsync_UserBodyParams_ReturnCreatedUser(string request)
        {
            var user = JsonSerializer.Deserialize<User>(request);

            //call the application then pass in the thing 

            Assert.IsType<User>(user);
            Assert.NotNull(user);
            Assert.Equal(new Guid("4C13BDF2-12CB-486D-BD2C-24C23D916A6D"), user.ID);
            Assert.Equal("Marcus", user.FirstName);
            Assert.Equal("Kok", user.LastName);
            Assert.Equal("marcus.kok@email.com", user.Email);
            Assert.Equal("https://umaimagestorage.blob.core.windows.net/umaprofilepicture/Claim_Dental.jpeg", user.ProfilePictureUrl);
        }

        [Theory]
        [InlineData("")]
        public async Task CreateUserAsync_UserBodyParams_ThrowException(string request)
        {

        }

        [Theory]
        [InlineData("")]
        public async Task UpdateUserAsync_UserBodyParams_ReturnUpdatedUser(string request)
        {

        }

        [Theory]
        [InlineData("")]
        public async Task UpdateUserAsync_UserBodyParams_ThrowException(string request)
        {

        }


        [Theory]
        [InlineData("")]
        public async Task UploadProfileAsync_UserBodyParams_ReturnUrlString(string request)
        {

        }


        [Theory]
        [InlineData("")]
        public async Task UploadProfileAsync_UserBodyParams_ThrowException(string request)
        {

        }



    }
}
