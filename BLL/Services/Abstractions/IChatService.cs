using BLL.Common.Dtos.Chat;

namespace BLL.Services.Abstractions
{
    public interface IChatService
    {
        Task<MessageResult> SavePrivateChatMessageAsync(MessageDto message);
        Task<MessageResult> SaveChatMessageAsync(MessageDto message);
        Task DeleteMessageAsync(long messageId, string userId);
        Task<List<MessageResult>> GetMessagesWithUserAsync(string userId, string ownerId);
        Task<List<MessageResult>> GetChatMessagesAsync(long chatId, string ownerId);
        Task<List<ChatResult>> GetAllChatsAsync(string userId);
        Task<ChatResult> GetChatByIdAsync(long chatId, string userId);
    }
}
