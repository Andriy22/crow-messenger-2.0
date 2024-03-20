using BLL.Common.Accounts.Dtos;
using FluentValidation;

namespace API.Common.Accounts.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator() 
        {
            RuleFor(x => x.NickName).NotEmpty().MinimumLength(4);
            RuleFor(x => x.Password).MinimumLength(6);
        }
    }
}
