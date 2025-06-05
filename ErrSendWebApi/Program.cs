using Serilog.Events;
using Serilog;

namespace ErrSendWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Відключаємо StaticWebAssets перед ініціалізацією
            Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "");
            
            // Створюємо конфігурацію для читання налаштувань
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build();

            // Конфігурація Serilog з можливістю логування в БД
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.File("Logs/ErrorSender-.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                // TODO: Додати логування в БД коли буде підключена БД
                // .WriteTo.MSSqlServer(connectionString, "Logs")
                .CreateLogger();

            try
            {
                Log.Information("Starting ErrorSender API...");
                var host = CreateHostBuilder(args).Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Конфігурація URL адрес - тепер з підтримкою HTTPS у production
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (env == "Development")
                    {
                        webBuilder.UseUrls("http://localhost:5001");
                    }
                    else
                    {
                        webBuilder.UseUrls("http://localhost:5001", "https://localhost:5002");
                    }
                    
                    // Відключаємо статичні веб-ресурси повністю
                    webBuilder.UseSetting("webroot", "");
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Відключаємо StaticWebAssets у конфігурації
                    context.Configuration["UseStaticWebAssets"] = "false";
                });
    }
}
