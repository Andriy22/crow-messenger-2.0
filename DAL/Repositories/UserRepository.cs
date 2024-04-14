using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DefaultDbContext _context;
        public UserRepository(DefaultDbContext context)
        {
             _context = context;
        }

        public Task<AppUser?> GetUserByIdAsync(string id) => _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<AppUser?> GetUserByNickNameAsync(string nickName) =>
            await _context.Users.FirstOrDefaultAsync(x => x.NickName.ToLower() == nickName.ToLower());

        public async Task SetUserOnlineDate(string userId, DateTime time)
        {
            await _context.Users.Where(x => x.Id == userId).ExecuteUpdateAsync(x => x.SetProperty(p => p.LastOnline, time));
        }
    }
}
