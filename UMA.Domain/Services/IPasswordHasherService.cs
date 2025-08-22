namespace UMA.Domain.Services
{
    public interface IPasswordHasherService
    {
        string Hash(string password);
        bool Verify(string pwdFromLogin, string pwdFromDB);
    }
}
