using FluentValidation;

namespace BLL.Common.Dtos.Profile
{
    public class UpdateStatusModel
    {
        public string? Status { get; set; }
    }

    public class UpdateStatusModelValidator: AbstractValidator<UpdateStatusModel>
    {
        public UpdateStatusModelValidator()
        {
            RuleFor(x => x.Status).MaximumLength(50);
        }
    }
}
