namespace BLL.Common.Dtos.Profile
{
    public class AppUserResult
    {
        public required string Id { get; set; }
        public required string NickName { get; set; }
        public required string ProfileImg { get; set; }
        public string? Bio { get; set; }
        public string? Status { get; set; }
        public DateTime? LastOnline { get; set; }
    }
}
