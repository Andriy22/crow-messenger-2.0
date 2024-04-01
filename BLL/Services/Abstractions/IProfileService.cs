using BLL.Common.Dtos.Profile;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Abstractions
{
    public interface IProfileService
    {
        public Task<string> UpdateProfileImageAsync(IFormFile file, string userId);
        public Task UpdateStatusAsync(string? status, string userId);
        public Task UpdateBioAsync(string? bio, string userId);
        public Task<AppUserResult> GetUserProfileAsync(string userId);
    }
}
