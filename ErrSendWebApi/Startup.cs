﻿using ErrSendApplication;
using ErrSendApplication.Common.Configs;
using ErrSendApplication.Interfaces;
using ErrSendApplication.Mappings;
using ErrSendPersistensTelegram;
using ErrSendWebApi.ExceptionMidlevare;
using ErrSendWebApi.Middleware;
using ErrSendWebApi.Middleware.Culture;
using ErrSendWebApi.Serviсe;
using ErrSendWebApi.TimeZone;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace ErrSendWebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // JWT configuration from environment variables
            var tokenConfig = new TokenConfig
            {
                TokenKey = Environment.GetEnvironmentVariable("JWT_TOKEN_KEY") ?? 
                    (_environment.IsDevelopment() ? "development-key-that-is-at-least-32-characters-long" : 
                        throw new InvalidOperationException("JWT_TOKEN_KEY environment variable is not set in production")),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ErrorSenderApi",
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "https://localhost:5001",
                ExpiryInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60")
            };

            services.Configure<TokenConfig>(config =>
            {
                config.TokenKey = tokenConfig.TokenKey;
                config.Issuer = tokenConfig.Issuer;
                config.Audience = tokenConfig.Audience;
                config.ExpiryInMinutes = tokenConfig.ExpiryInMinutes;
            });

            //Додаємо профілі зборок в ДІ конвеєр через автомапер.
            services.AddAutoMapper(config =>
            {
                config.AddProfile(new AssemblyMappingProfile(Assembly.GetExecutingAssembly()));
                config.AddProfile(new AssemblyMappingProfile(typeof(IHttpClientWr).Assembly));
            });

            //Підключаємо зборки в ДІ через Медіатр
            services.AddApplication(Configuration);
            services.AddPersistenceTelegram(Configuration);
            services.AddControllers();
            
            // JWT Authentication configuration
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenConfig.Issuer,
                    ValidAudience = tokenConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.TokenKey))
                };
            });

            //Політика підключення із всіх а не тільки через ІдентітіСрв не працювало б якщо ми б спробували із 1с наприклад підключитись.
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowAnyOrigin();
                });
            });

            services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                c.SwaggerDoc("v0.3", new OpenApiInfo
                {
                    Title = "ErrorSender API",
                    Version = "v.0.3",
                    Description = "API для відправки помилок в Telegram групу"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Авторизація",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введіть JWT Token у форматі: Bearer {token}"
                });

                c.OperationFilter<AddTimeAndTimeZoneOperationFilter>(); // Додавання кастомного фільтра

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                          },
                         new string[] {}
                    }
                });
            });

            services.AddSingleton<ICurrentService, CurrentService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IJwtService, JwtService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Встановлюємо порожній WebRootPath, щоб не шукати wwwroot папку
            env.WebRootPath = string.Empty;

            //Отримує ІР компа із якого прийшов запит але додатково налаштовуємо в конфігах NGINX.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.RoutePrefix = string.Empty;
                config.SwaggerEndpoint("swagger/v1/swagger.json", "ErrorSender API v1");
            });

            // Додаємо Telegram error reporting middleware (перед іншими exception handlers)
            app.UseTelegramErrorReporting();
            
            app.UseCustomExceptionHandler();
            app.UseCulture();
            app.UseRouting();
            
            // HTTPS redirection та CORS - підключення із всіх джерел
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowAll");

            //Додаємо авторизацію і аутентифікацію по токену.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
