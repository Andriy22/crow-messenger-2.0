using BLL.Common.Dtos.Profile;
using DAL.Enums;

namespace BLL.Common.Dtos.Chat
{
    public class MessageUserResult : AppUserResult
    {
        public UserChatRoleType Role { get; set; }
    }
}
