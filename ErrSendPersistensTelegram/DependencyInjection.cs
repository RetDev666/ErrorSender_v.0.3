using ErrSendApplication.Common.Configs;
using ErrSendApplication.Interfaces;
using ErrSendPersistensTelegram.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErrSendPersistensTelegram
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceTelegram(this IServiceCollection services, IConfiguration configuration)
        {
            // Конфігурація Telegram
            services.Configure<TelegramConfig>(configuration.GetSection("Telegram"));

            services.AddHttpClient<IHttpClientWr, StandartHttpClient>()
                            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                             {
                                 ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                             })
                            .ConfigureHttpClient(client =>
                            {
                                var serverUrl = configuration.GetValue<string>("serverUrl") ?? "http://localhost:5001/";
                                client.BaseAddress = new Uri(serverUrl);
                            });

            // Реєструємо Telegram сервіс
            services.AddScoped<ITelegramService, TelegramService>();

            return services;
        }
    }
}
