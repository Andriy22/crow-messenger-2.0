using AutoMapper;
using BLL.Common.Constants;
using BLL.Common.Dtos.Profile;
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

        private readonly IMapper _mapper;

        public ProfileService(IFileService fileService, IRepository<AppUser> userRepository, IMapper mapper)
        {
            _fileService = fileService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<AppUserResult> GetUserProfileAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);

            return _mapper.Map<AppUserResult>(user);
        }

        public async Task UpdateBioAsync(string? bio, string userId)
        {
            var user = await GetUserByIdAsync(userId);

            user.BIO = bio;

            await _userRepository.EditAsync(user);
        }

        public async Task<string> UpdateProfileImageAsync(IFormFile file, string userId)
        {
            var user = await GetUserByIdAsync(userId);

            var fileName = await _fileService.SaveFileAsync(file, FileConstants.UsersFiles);

            user.ProfileImg = fileName;

            await _userRepository.EditAsync(user);

            return fileName;
        }

        public async Task UpdateStatusAsync(string? status, string userId)
        {
            var user = await GetUserByIdAsync(userId);

            user.Status = status;

            await _userRepository.EditAsync(user);
        }

        private async Task<AppUser> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetQueryable(x => x.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new HttpRequestException("User doesn't exist", null, HttpStatusCode.BadRequest);
            }

            return user;
        }
    }
}
