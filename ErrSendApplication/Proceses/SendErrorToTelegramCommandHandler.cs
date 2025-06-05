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
        private readonly ITelegramService _telegramService;
        private readonly IMapper _mapper;
        private readonly ILogger<SendErrorToTelegramCommandHandler> _logger;

        public SendErrorToTelegramCommandHandler(
            ITelegramService telegramService,
            IMapper mapper,
            ILogger<SendErrorToTelegramCommandHandler> logger)
        {
            _telegramService = telegramService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SendErrorToTelegramResponse> Handle(SendErrorToTelegramCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending error to Telegram: {ErrorMessage}", request.ErrorReport.ErrorMessage);

                var errorReport = _mapper.Map<ErrorReport>(request.ErrorReport);
                errorReport.Timestamp = DateTime.UtcNow;

                var result = await _telegramService.SendErrorAsync(errorReport);

                _logger.LogInformation("Error sent to Telegram successfully: {MessageId}", result.TelegramMessageId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send error to Telegram");
                return new SendErrorToTelegramResponse
                {
                    IsSuccess = false,
                    Message = $"Failed to send error to Telegram: {ex.Message}"
                };
            }
        }
    }
} 