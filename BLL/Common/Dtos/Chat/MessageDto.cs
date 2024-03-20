using DAL.Enums;
using Microsoft.AspNetCore.Http;

namespace BLL.Common.Dtos.Chat
{
    public class MessageDto
    {
        public long? Id { get; set; }
        public string? Message { get; set; }
        public string? ReceiverId { get; set; }
        public long? ChatId { get; set; }
        public string? SenderId { get; set; }
        public MessageType MessageType { get; set; }
        public long? ReplyMessageId { get; set; }
        public List<IFormFile>? Attachments { get; set; } = new List<IFormFile>();
    }
}
