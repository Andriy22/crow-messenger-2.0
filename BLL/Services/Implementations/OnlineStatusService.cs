using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    public class OnlineStatusService : IOnlineStatusService
    {
        private readonly IRepository<Chat> _chatRepository;
        private readonly IUserRepository _userRepository;

        public OnlineStatusService(IRepository<Chat> chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
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

        public async Task SetUserLastOnline(string userId, DateTime? time)
        {
            var date = time ?? DateTime.Now;

            await _userRepository.SetUserOnlineDate(userId, date);
        }
    }
}
