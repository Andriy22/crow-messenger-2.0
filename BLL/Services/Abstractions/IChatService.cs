using BLL.Common.Dtos.Chat;
using BLL.Common.Dtos.PrivateChat;

namespace BLL.Services.Abstractions
{
    public interface IChatService
    {
        Task<PrivateMessageResult> SavePrivateChatMessageAsync(PrivateMessageDto message);
        Task DeleteMessageAsync(long messageId, string userId);
        Task<List<PrivateMessageResult>> GetMessagesWithUserAsync(string userId, string ownerId);
        Task<List<ChatResult>> GetAllChatsAsync(string userId);
        Task<ChatResult> GetChatByIdAsync(long chatId);
    }
}
