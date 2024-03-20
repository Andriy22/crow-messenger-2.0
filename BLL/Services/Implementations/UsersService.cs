using AutoMapper;
using BLL.Common.Dtos.Chat;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly IRepository<AppUser> _usersRepository;
        private readonly IMapper _mapper;

        public UsersService(IRepository<AppUser> usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;
        }

        public async Task<List<MessageUserResult>> GetUsersByNicknameAsync(string nickname)
        {
            var users = await _usersRepository.GetQueryable()
                .Where(x => EF.Functions.Like(x.NickName.ToLower(), $"%{nickname.ToLower()}%"))
                .ToListAsync();

            return _mapper.Map<List<MessageUserResult>>(users);
        }
    }
}
