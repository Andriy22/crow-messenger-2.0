using DAL.Enums;

namespace BLL.Common.Dtos.Chat
{
    public class MessageUserResult
    {
        public required string Id { get; set; }
        public required string NickName { get; set; }
        public required string ProfileImg { get; set; }
        public UserChatRoleType Role { get; set; }
    }
}
