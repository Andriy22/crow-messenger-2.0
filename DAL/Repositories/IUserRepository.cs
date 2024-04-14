using DAL.Entities;

namespace DAL.Repositories
{
    public interface IUserRepository
    {
        public Task<AppUser?> GetUserByNickNameAsync(string nickName);
        public Task<AppUser?> GetUserByIdAsync(string id);
        public Task SetUserOnlineDate(string userId, DateTime time);
    }
}
