using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMA.Domain.Entities;
using UMA.Domain.Repositories;
using UMA.Infrastructure.Persistence;

namespace UMA.Infrastructure.Persistence.Repositories
{
    public class JwtTokenRepository : IJwtTokenRepository
    {
        private readonly AppDbContext _context;

        public JwtTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<JwtToken?> GetJwtTokenByUserIDAsync(Guid userID)
        {
            return await _context.JwtToken.FirstOrDefaultAsync(u => u.UserID == userID);
        }
        public async Task AddJwtTokenAsync(JwtToken jwtToken)
        {
            await _context.JwtToken.AddAsync(jwtToken);
            await _context.SaveChangesAsync();
        }

        //public async Task UpdateUserAsync(User user)
        //{
        //    _context.Users.Update(user);
        //    await _context.SaveChangesAsync();
        //}
    }
}
