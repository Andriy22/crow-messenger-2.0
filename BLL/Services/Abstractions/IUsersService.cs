using BLL.Common.Dtos.Chat;

namespace BLL.Services.Abstractions
{
    public interface IUsersService
    {
        Task<List<MessageUserResult>> GetUsersByNicknameAsync(string nickname);
    }
}
