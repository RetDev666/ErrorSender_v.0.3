using Domain.Models;
using ErrSendApplication.DTO;

namespace ErrSendApplication.Interfaces
{
    public interface ITelegramService
    {
        Task<SendErrorToTelegramResponse> SendErrorAsync(ErrorReport errorReport);
    }
} 