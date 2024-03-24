using BLL.Common.Dtos.Chat;

namespace BLL.Services.Abstractions;
public interface IMessageService
{
    Task<MessageResult> SavePrivateChatMessageAsync(MessageDto message);
    Task<MessageResult> SaveChatMessageAsync(MessageDto message);
    Task DeleteMessageAsync(long messageId, string userId);
    Task<List<MessageResult>> GetMessagesWithUserAsync(string userId, string ownerId);
    Task<List<MessageResult>> GetChatMessagesAsync(long chatId, string ownerId);
}
