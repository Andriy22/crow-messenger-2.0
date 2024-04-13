using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    public class OnlineStatusService : IOnlineStatusService
    {
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<Chat> _chatRepository;

        public OnlineStatusService(IRepository<AppUser> userRepository, IRepository<Chat> chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }
       
        public async Task<List<string>> GetUsersIdsToBroadcastAsync(string userId)
        {
            var userIds = await _chatRepository
                .GetQueryable(x => x.Users.Any(x => x.UserId == userId))
                .Include(x => x.Users)
                .SelectMany(x => x.Users.Select(x => x.UserId))
                .ToListAsync();

            userIds = userIds.Distinct().ToList();

            userIds.Remove(userId);

            return userIds;
        }
    }
}
