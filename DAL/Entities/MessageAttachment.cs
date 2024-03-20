using DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class MessageAttachment : IEntity, ICreatedAt, IUpdatedAt
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public required string AttachmentPath { get; set; }
        [ForeignKey("CreatedBy")] public required string CreatedById { get; set; }
        public virtual AppUser? CreatedBy { get; set; }
        public virtual Message? Message { get; set; }
    }
}
