using ErrSendApplication.DTO;
using MediatR;

namespace ErrSendApplication.Proceses
{
    public class SendErrorToTelegramCommand : IRequest<SendErrorToTelegramResponse>
    {
        public ErrorReportDto ErrorReport { get; set; } = new();
    }
} 