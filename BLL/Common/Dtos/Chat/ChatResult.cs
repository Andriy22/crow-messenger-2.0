using DAL.Enums;

namespace BLL.Common.Dtos.Chat
{
    public class ChatResult
    {
        public long Id { get; set; }
        public ChatType ChatType { get; set; }
        public string? Title { get; set; }
        public required string ProfileImg { get; set; }
        public required List<MessageUserResult> Users { get; set; }
    }
}
