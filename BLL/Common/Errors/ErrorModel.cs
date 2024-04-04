using System.Text.Json.Serialization;

namespace BLL.Common.Errors
{
    public class ErrorModel
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        
        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }
}
