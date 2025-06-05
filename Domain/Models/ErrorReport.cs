namespace Domain.Models
{
    public class ErrorReport
    {
        public int Id { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning, Info
        public string AdditionalInfo { get; set; } = string.Empty;
    }
} 