
using DAL.Entities;
using DAL.Repositories;
using System.Net;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using BLL.Common.Accounts.Dtos;

namespace BLL.Services.Implementations
{
    public class AccountsService : IAccountsService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;

        public AccountsService(IUserRepository userRepository, UserManager<AppUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task CreateAccountAsync(CreateUserDto model)
        {
            var user = await _userRepository.GetUserByNickNameAsync(model.NickName);

            if (user is not null)
            {
                throw new HttpRequestException("User already exists", null, HttpStatusCode.BadRequest);
            }

            await _userManager.CreateAsync(new AppUser { NickName = model.NickName, UserName = model.NickName, ProfileImg = "" }, model.Password);
        }
    }
}
