using BLL.Common.Accounts.Dtos;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace main_server.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsService _accountsService;

        public AccountsController(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccountAsync(CreateUserDto model)
        {
            await _accountsService.CreateAccountAsync(model);

            return Ok();
        }
    }
}
