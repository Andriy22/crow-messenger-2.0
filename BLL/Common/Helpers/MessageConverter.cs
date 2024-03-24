using BLL.Common.Dtos.Chat;
using DAL.Entities;
using DAL.Enums;

namespace BLL.Common.Helpers
{
    public static class MessageConverter
    {
        public static MessageResult MessageToResult(Message dbMessage, AppUser? sender, UserChatRoleType role = UserChatRoleType.None)
        {
            return new MessageResult
            {
                Attachments = dbMessage.Attachments!.Select(a => new MessageAttachmentResult { AttachemntPath = a.AttachmentPath, Id = a.Id }).ToList(),
                Message = dbMessage.Text,
                MessageType = dbMessage.MessageType,
                Sender = new MessageUserResult
                {
                    Id = sender?.Id ?? "",
                    NickName = sender?.NickName ?? "",
                    ProfileImg = sender?.ProfileImg ?? "",
                    Role = role,
                },
                Id = dbMessage.Id,
                CreatedAt = dbMessage.CreatedAt,
                SeenAt = dbMessage.SeenAt,
                ChatId = dbMessage.ChatId,
                IsSystem = dbMessage.CreatedById == null,
            };
        }
    }
}
