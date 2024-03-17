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

        public async Task<AppUser?> GetUserByNickNameAsync(string nickName) =>
            await _context.Users.FirstOrDefaultAsync(x => x.NickName.ToLower() == nickName.ToLower());
    }
}
