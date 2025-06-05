using Domain.Models;
using ErrSendApplication.DTO;
using ErrSendApplication.Proceses;
using Microsoft.AspNetCore.Mvc;

namespace ErrSendWebApi.Controllers
{
    /// <summary>
    /// Контролер для відправки помилок в Telegram
    /// </summary>
    public class ErrorController : BaseController
    {
        /// <summary>
        /// Відправити помилку в Telegram групу
        /// </summary>
        /// <param name="errorReport">Дані про помилку</param>
        /// <returns>Результат відправки</returns>
        [HttpPost]
        public async Task<IActionResult> SendToTelegram([FromBody] ErrorReportDto errorReport)
        {
            try
            {
                var command = new SendErrorToTelegramCommand
                {
                    ErrorReport = errorReport
                };

                var result = await Mediator.Send(command);

                var executionStatus = new ExecutionStatus();

                if (result.IsSuccess)
                {
                    executionStatus.Status = "OK";
                    executionStatus.ErrorCode = 200;
                    
                    return Ok(new
                    {
                        ExecutionStatus = executionStatus,
                        Data = result
                    });
                }
                else
                {
                    executionStatus.Status = "ER";
                    executionStatus.ErrorCode = 400;
                    executionStatus.Errors.Add(result.Message);
                    
                    return BadRequest(new
                    {
                        ExecutionStatus = executionStatus
                    });
                }
            }
            catch (Exception ex)
            {
                var executionStatus = new ExecutionStatus
                {
                    Status = "ER",
                    ErrorCode = 500
                };
                executionStatus.Errors.Add($"Внутрішня помилка сервера: {ex.Message}");

                return StatusCode(500, new
                {
                    ExecutionStatus = executionStatus
                });
            }
        }

        /// <summary>
        /// Відправити простий текст помилки в Telegram
        /// </summary>
        /// <param name="errorMessage">Текст помилки</param>
        /// <param name="source">Джерело помилки</param>
        /// <param name="severity">Важливість (Error, Warning, Info)</param>
        /// <returns>Результат відправки</returns>
        [HttpPost]
        public async Task<IActionResult> SendSimpleError(
            [FromQuery] string errorMessage, 
            [FromQuery] string source = "API", 
            [FromQuery] string severity = "Error")
        {
            var errorReport = new ErrorReportDto
            {
                ErrorMessage = errorMessage,
                Source = source,
                Severity = severity
            };

            return await SendToTelegram(errorReport);
        }
    }
} 