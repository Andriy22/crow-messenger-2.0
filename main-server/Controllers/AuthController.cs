using BLL.Common.Dtos.Auth;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authServe;

        public AuthController(IAuthService authServe)
        {
            _authServe = authServe;
        }

        [HttpPost("authorize")]
        public async Task<ActionResult<AuthorizationResult>> AuthorizeAsync(AuthorizationDto model)
        {
            return Ok(await _authServe.AuthorizationAsync(model));
        }
    }
}
