namespace BLL.Common.Dtos.Auth
{
    public class AuthorizationResult
    {
        public required string AccessToken { get; set; }
        public required string TokenType { get; set;}
    }
}
