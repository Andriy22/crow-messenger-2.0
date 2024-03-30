using BLL.Common.Errors;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace API.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var errors = new List<ErrorModel>();

            switch (exception)
            {
                case HttpRequestException customHttpException:
                    {
                        code = customHttpException.StatusCode ?? HttpStatusCode.BadRequest;
                        errors.Add(new ErrorModel { Code = code.ToString(), Message = customHttpException.Message });
                        break;
                    }
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            if (!errors.Any())
            {
                return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = exception.Message, trace = exception.StackTrace, requestId = Activity.Current.Id }));
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(errors));
        }
    }
}
