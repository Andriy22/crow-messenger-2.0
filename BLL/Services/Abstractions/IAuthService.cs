using BLL.Common.Dtos.Auth;
using System.Net;

namespace BLL.Services.Abstractions
{
    public interface IAuthService
    {
        Task<AuthorizationResult> AuthorizationAsync(AuthorizationDto model);
    }
}
