using DAL.Entities;

namespace DAL.Repositories
{
    public interface IUserRepository
    {
        public Task<AppUser?> GetUserByNickNameAsync(string nickName);
    }
}
