using BLL.Common.Errors;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace API.Middlewares
{
    public class UseCustomErrorModelInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            var failures = result.Errors
                .Select(error => new ValidationFailure(error.PropertyName, SerializeError(error)));

            return new ValidationResult(failures);
        }

        private static string SerializeError(ValidationFailure failure)
        {
            var error = new ErrorModel { Code = failure.ErrorCode, Message = failure.ErrorMessage };
            return JsonSerializer.Serialize(error);
        }
    }
}
