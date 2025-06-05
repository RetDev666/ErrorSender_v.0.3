using Domain.Models;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace ErrSendWebApi.ExceptionMidlevare
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            var exStat = new ExecutionStatus();
            exStat.Status = "ER";
            exStat.ErrorCode = (int)code;

            switch (exception)
            {
                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    exStat.ErrorCode = (int)code;
                    foreach (var error in validationException.Errors)
                    {
                        exStat.Errors.Add(error.ErrorMessage);
                    }
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case SecurityTokenException:
                    code = HttpStatusCode.Unauthorized;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Недійсний токен безпеки");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Доступ заборонено");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case ArgumentNullException argNullEx:
                    code = HttpStatusCode.BadRequest;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add($"Обов'язковий параметр відсутній: {argNullEx.ParamName}");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case ArgumentException argEx:
                    code = HttpStatusCode.BadRequest;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add($"Невірний параметр: {argEx.Message}");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case InvalidOperationException:
                    code = HttpStatusCode.Conflict;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Операція не може бути виконана в поточному стані");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case TimeoutException:
                    code = HttpStatusCode.RequestTimeout;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Час очікування вичерпано");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                case NotImplementedException:
                    code = HttpStatusCode.NotImplemented;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Функціональність не реалізована");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;

                default:
                    code = HttpStatusCode.InternalServerError;
                    exStat.ErrorCode = (int)code;
                    exStat.Errors.Add("Внутрішня помилка сервера");
                    result = JsonSerializer.Serialize(new { ExecutionStatus = exStat });
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
