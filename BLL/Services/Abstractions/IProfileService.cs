using Microsoft.AspNetCore.Http;

namespace BLL.Services.Abstractions
{
    public interface IProfileService
    {
        public Task<string> UpdateProfileImageAsync(IFormFile file, string userId);
    }
}
