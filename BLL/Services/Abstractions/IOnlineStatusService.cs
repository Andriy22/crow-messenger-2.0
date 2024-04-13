namespace BLL.Services.Abstractions
{
    public interface IOnlineStatusService
    {
        public Task<List<string>> GetUsersIdsToBroadcastAsync(string userId);
    }
}
