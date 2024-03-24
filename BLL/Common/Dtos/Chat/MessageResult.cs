using DAL.Enums;

namespace BLL.Common.Dtos.Chat
{
    public class MessageResult
    {
        public long Id { get; set; }
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public MessageUserResult? Sender { get; set; }
        public required List<MessageAttachmentResult> Attachments { get; set; }
        public required DateTime CreatedAt { get; set; }
        public long ChatId { get; set; }
        public long ReplyMessageId { get; set; }
        public bool IsSystem { get; set; }
        public DateTime? SeenAt { get; set; }
    }
}
