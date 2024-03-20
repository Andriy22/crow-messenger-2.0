using DAL.Enums;
using DAL.Interfaces;

namespace DAL.Entities
{
    public class Chat: IEntity, ICreatedAt, IUpdatedAt, IDeletable
    {
        public long Id { get; set; }
        public required string ImagePath { get; set; }
        public string? Title { get; set; }
        public ChatType ChatType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsDeleted { get; set; } 
        public required ICollection<Message> Chats { get; set;}
        public required ICollection<UserChat> Users { get; set;}
    }
}
