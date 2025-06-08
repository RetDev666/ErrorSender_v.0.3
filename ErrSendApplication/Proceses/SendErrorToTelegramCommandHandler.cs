using AutoMapper;
using Domain.Models;
using ErrSendApplication.DTO;
using ErrSendApplication.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErrSendApplication.Proceses
{
    public class SendErrorToTelegramCommandHandler : IRequestHandler<SendErrorToTelegramCommand, SendErrorToTelegramResponse>
    {
        private readonly ITelegramService telegramService;
        private readonly IMapper mapper;
        private readonly ILogger<SendErrorToTelegramCommandHandler> logger;

        public SendErrorToTelegramCommandHandler(
            ITelegramService telegramService,
            IMapper mapper,
            ILogger<SendErrorToTelegramCommandHandler> logger)
        {
            this.telegramService = telegramService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<SendErrorToTelegramResponse> Handle(SendErrorToTelegramCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Помилка надсилання на адресу Telegram: {ErrorMessage}", request.ErrorReport.ErrorMessage);

                var errorReport = mapper.Map<ErrorReport>(request.ErrorReport);
                errorReport.Timestamp = DateTime.UtcNow;

                var result = await telegramService.SendErrorAsync(errorReport);

                logger.LogInformation("Помилка успішно відправлена в Telegram: {MessageId}", result.TelegramMessageId);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send error to Telegram");
                return new SendErrorToTelegramResponse
                {
                    IsSuccess = false,
                    Message = $"Failed to send error to Telegram: {ex.Message}"
                };
            }
        }
    }
} 