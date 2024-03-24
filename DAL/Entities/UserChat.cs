using DAL.Enums;
using DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    public class UserChat: IEntity
    {
        public long Id { get; set; }
        [ForeignKey("User")] public required string UserId { get; set; }
        public virtual AppUser? User { get; set; }
        [ForeignKey("Chat")] public long ChatId { get; set; }
        public virtual Chat? Chat { get; set; }
        public UserChatRoleType Role { get; set; }
    }
}
