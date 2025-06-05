namespace ErrSendWebApi.Middleware
{
    public static class TelegramErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseTelegramErrorReporting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TelegramErrorMiddleware>();
        }
    }
} 