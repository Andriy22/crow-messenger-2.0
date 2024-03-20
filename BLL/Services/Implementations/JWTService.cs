using BLL.Services.Abstractions;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services.Implementations;

public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;

    public JWTService(
        IConfiguration configuration,
        UserManager<AppUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public string CreateToken(AppUser user)
    {
        var identity = GetIdentity(user);
        var now = DateTime.UtcNow;

        var key = _configuration["Jwt:Key"] ?? "JustStrongSecret";

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        var jwt = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            notBefore: now,
            claims: identity.Claims,
            expires: now.AddDays(14),
            signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return encodedJwt;
    }

    // TODO:
    // MAKE THIS METHOD ASYNC,
    // .Result IS UNSAFE AND CAN CAUSE ERRORS
    private ClaimsIdentity GetIdentity(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, user.Id)
        };

        var roles = _userManager.GetRolesAsync(user).Result.ToList();
       
        foreach (var el in roles) claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, el));
       
        claims.Add(new Claim("email", user?.Email ?? "NotSpecified"));
        var claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

        return claimsIdentity;
    }
}

