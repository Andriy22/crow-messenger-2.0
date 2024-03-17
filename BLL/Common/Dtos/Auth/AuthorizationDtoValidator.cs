using FluentValidation;

namespace BLL.Common.Dtos.Auth
{
    public class AuthorizationDtoValidator : AbstractValidator<AuthorizationDto>
    {
        public AuthorizationDtoValidator()
        {
            RuleFor(x => x.NickName).NotEmpty().MinimumLength(3);
        }
    }
}