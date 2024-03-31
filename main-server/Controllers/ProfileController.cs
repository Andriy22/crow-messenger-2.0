using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/profile")]
    [Authorize]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost("change-image")]
        public async Task<ActionResult<string>> ChangeImageAsync([FromForm] IFormFile file)
        {
            var userId = User?.Identity?.Name ?? throw new Exception("Unauthorized");

            return Ok(await _profileService.UpdateProfileImageAsync(file, userId));
        } 
    }
}
