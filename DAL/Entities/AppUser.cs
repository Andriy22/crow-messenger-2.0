using Microsoft.AspNetCore.Identity;

namespace DAL.Entities
{
    public class AppUser : IdentityUser
    {
        public required string NickName { get; set; }
        public required string ProfileImg { get; set; }
    }
}
