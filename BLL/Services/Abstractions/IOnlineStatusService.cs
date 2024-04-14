namespace BLL.Services.Abstractions
{
    public interface IOnlineStatusService
    {
        public Task<List<string>> GetUsersIdsToBroadcastAsync(string userId);
        public Task SetUserLastOnline(string userId, DateTime? time);
    }
}
