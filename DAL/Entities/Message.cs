using DAL.Enums;
using DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class Message : IEntity, IDeletable, ICreatedAt, IUpdatedAt, ISeenAt
    {
        public long Id { get; set; }
        public string? Text { get; set; }
        public MessageType MessageType { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime? SeenAt { get; set; }
        public long? ReplyMessageId { get; set; }
        public long ChatId { get; set; }
        public virtual Chat? Chat { get; set; }
        [ForeignKey("CreatedBy")] public required string CreatedById { get; set; }
        public virtual AppUser? CreatedBy { get; set; }
        public virtual required ICollection<MessageAttachment> Attachments { get; set; }
    }
}
