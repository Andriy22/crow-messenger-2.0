using BLL.Common.Dtos.Profile;
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
            var userId = User!.Identity!.Name;

            return Ok(await _profileService.UpdateProfileImageAsync(file, userId!));
        }

        [HttpGet("me")]
        public async Task<ActionResult<AppUserResult>> GetMeAsync()
        {
            var userId = User!.Identity!.Name;

            return Ok(await _profileService.GetUserProfileAsync(userId!));
        }

        [HttpPatch("update-bio")]
        public async Task<IActionResult> UpdateBioAsync(UpdateBioModel model)
        {
            var userId = User!.Identity!.Name;

            await _profileService.UpdateBioAsync(model.Bio, userId!);

            return Ok();
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateStatusAsync(UpdateStatusModel model)
        {
            var userId = User!.Identity!.Name;

            await _profileService.UpdateStatusAsync(model.Status, userId!);

            return Ok();
        }
    }
}
