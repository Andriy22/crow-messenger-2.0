using DAL.Entities;

namespace BLL.Services.Abstractions;
public interface IJWTService
{
    string CreateToken(AppUser user);
}
