using UMA.Domain.Entities;

namespace UMA.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIDAsync(Guid userID);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
