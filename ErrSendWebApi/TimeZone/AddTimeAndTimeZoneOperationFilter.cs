using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ErrSendWebApi.TimeZone
{
    public class AddTimeAndTimeZoneOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Перевіряємо чи існує відповідь 200 та чи має вона контент
            if (operation.Responses.ContainsKey("200") && 
                operation.Responses["200"].Content != null &&
                operation.Responses["200"].Content.ContainsKey("application/json"))
            {
                var content = new OpenApiObject
                {
                    ["time"] = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    ["timezone"] = new OpenApiString(GetKyivTimeZone())
                };

                operation.Responses["200"].Content["application/json"].Example = content;
            }
        }

        private string GetKyivTimeZone()
        {
            try
            {
                // Намагаємося отримати часовий пояс Києва
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev").DisplayName;
            }
            catch
            {
                try
                {
                    // Альтернативний варіант для різних систем
                    return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv").DisplayName;
                }
                catch
                {
                    // Якщо не вдається знайти - повертаємо UTC+2
                    return "UTC+02:00 Kyiv";
                }
            }
        }
    }
}
