﻿using DAL.Enums;

namespace BLL.Common.Dtos.Chat
{
    public class MessageResult
    {
        public long Id { get; set; }
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public required MessageUserResult Sender { get; set; }
        public required List<MessageAttachmentResult> Attachments { get; set; }
        public required DateTime CreatedAt { get; set; }
        public long ChatId { get; set; }
        public DateTime? SeenAt { get; set; }
    }
}