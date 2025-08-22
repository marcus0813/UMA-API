using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMA.Domain.Services;

namespace UMA.Infrastructure.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string pwdFromLogin, string pwdFromDB) 
        {
            return BCrypt.Net.BCrypt.Verify(pwdFromLogin, pwdFromDB);
    }   }
}
