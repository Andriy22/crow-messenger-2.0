using BLL.Common.Constants;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BLL.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IFileService _fileService;
        private readonly IRepository<AppUser> _userRepository;

        public ProfileService(IFileService fileService, IRepository<AppUser> userRepository)
        {
            _fileService = fileService;
            _userRepository = userRepository;
        }

        public async Task<string> UpdateProfileImageAsync(IFormFile file, string userId)
        {
            var user = await _userRepository.GetQueryable(x => x.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new HttpRequestException("User doesn't exist", null, HttpStatusCode.BadRequest);
            }

            var fileName = await _fileService.SaveFileAsync(file, FileConstants.UsersFiles);

            user.ProfileImg = fileName;
            
            _userRepository.Edit(user);

            return fileName;
        }
    }
}
