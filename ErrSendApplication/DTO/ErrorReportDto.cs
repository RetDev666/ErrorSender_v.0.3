using AutoMapper;
using Domain.Models;
using ErrSendApplication.Mappings;

namespace ErrSendApplication.DTO
{
    public class ErrorReportDto : IMapWith<ErrorReport>
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
        public string AdditionalInfo { get; set; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ErrorReportDto, ErrorReport>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.Ignore());

            profile.CreateMap<ErrorReport, ErrorReportDto>();
        }
    }
    
    public class SendErrorToTelegramResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TelegramMessageId { get; set; } = string.Empty;
    }
} 