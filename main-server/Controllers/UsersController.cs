using BLL.Common.Dtos.Chat;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet("get-users-by-nickname")]
        public async Task<ActionResult<List<MessageUserResult>>> GetUsersByNicknameAsync(string nickname)
        {
            return Ok(await _usersService.GetUsersByNicknameAsync(nickname));
        }
    }
}
