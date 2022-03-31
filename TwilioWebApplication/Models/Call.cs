using Microsoft.EntityFrameworkCore;

namespace TwilioWebApplication.Models
{
    public enum CallType
    {
        Missed, Received, Sent
    }
    public class Call
    {
        public int Id { get; set; }
        public string? ClientNumber { get; set; }
        public string? EmployeeNumber { get; set; }

        public string TwilioNumber { get; set; }
        public string? RecordingId { get; set; }
        public string? SessionId { get; set; }
        public CallType CallType { get; set; }
        public DateTime CallDate { get; set; }
        public Employee Employee { get; set; }



    }
}
