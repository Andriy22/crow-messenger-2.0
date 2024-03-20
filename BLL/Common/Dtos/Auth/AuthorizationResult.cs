namespace BLL.Common.Dtos.Auth
{
    public class AuthorizationResult
    {
        public required string Id { get; set; }
        public required string NickName { get; set; }
        public required string ProfileImg { get; set; }
        public required string AccessToken { get; set; }
        public required string TokenType { get; set;}
    }
}
