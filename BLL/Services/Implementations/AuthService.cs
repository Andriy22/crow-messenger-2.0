using BLL.Common.Dtos.Auth;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IJWTService _jWTService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;

        public AuthService(UserManager<AppUser> userManager, 
                           IJWTService jWTService, 
                           IUserRepository userRepository)
        {
            _userManager = userManager;
            _jWTService = jWTService;
            _userRepository = userRepository;
        }

        public async Task<AuthorizationResult> AuthorizationAsync(AuthorizationDto model)
        {
            var user = await _userRepository.GetUserByNickNameAsync(model.NickName);

            if (user == null)
            {
                throw new HttpRequestException("User not found", null, HttpStatusCode.NotFound);
            }

            var isLoggined = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isLoggined)
            {
                throw new HttpRequestException("Password is wrong", null, HttpStatusCode.NotFound);
            }

            var access_token = _jWTService.CreateToken(user);

            return new AuthorizationResult
            {
                AccessToken = access_token,
                AccountId = user.Id,
                NickName = model.NickName,
                ProfileImg = user.ProfileImg,
                TokenType = "bearer"
            };
        }
    }
}
