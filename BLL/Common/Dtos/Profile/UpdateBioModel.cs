using FluentValidation;

namespace BLL.Common.Dtos.Profile
{
    public class UpdateBioModel
    {
        public string? Bio {  get; set; }
    }

    public class UpdateBioModelValidator: AbstractValidator<UpdateBioModel>
    {
        public UpdateBioModelValidator()
        {
            // we should not allow unlimited length for bio ;)
            RuleFor(x => x.Bio).MaximumLength(200);
        }
    }
}
