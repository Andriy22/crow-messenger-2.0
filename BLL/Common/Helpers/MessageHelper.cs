using DAL.Entities;
using DAL.Enums;

namespace BLL.Common.Helpers
{
    public static class MessageHelper
    {
        public static Message CreateSystemMessage(string message)
        {
            return new Message
            {
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                MessageType = MessageType.Text,
                Text = message,
            };
        }
    }
}
