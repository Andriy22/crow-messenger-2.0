using BLL.Common.Dtos.Chat;
using DAL.Enums;

namespace BLL.Services.Abstractions
{
    public interface IChatService
    {
        Task<List<ChatResult>> GetAllChatsAsync(string userId);
        Task<ChatResult> GetChatByIdAsync(long chatId, string userId);
        Task<ChatResult> CreateChatAsync(string title, ChatType chatType, string userId);
        Task<ChatResult> AddUsersToChatAsync(long chatId, List<string> accountIds, string userId);
        Task<ChatResult> RemoveUsersFromChatAsync(long chatId, List<string> accountIds, string userId);
        Task DeleteChatByIdAsync(long chatId, string userId);
        Task<ChatResult> LeaveChatAsync (long chatId, string userId);
    }
}
