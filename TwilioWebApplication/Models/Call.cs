using Microsoft.EntityFrameworkCore;

namespace TwilioWebApplication.Models
{
    public enum CallType
    {
        Missed = 0, Received = 1, Sent = 2, Failed = 3
    }
    public class Call
    {
        public int Id { get; set; }
        public string? ClientNumber { get; set; }
        public string TwilioNumber { get; set; }
        public string? RecordingId { get; set; }
        public string? SessionId { get; set; }
        public CallType CallType { get; set; }
        public DateTime CallDate { get; set; }
        public Employee Employee { get; set; }



    }
}
