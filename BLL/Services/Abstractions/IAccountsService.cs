using BLL.Common.Accounts.Dtos;

namespace BLL.Services.Abstractions
{
    public interface IAccountsService
    {
        Task CreateAccountAsync(CreateUserDto model);
    }
}
