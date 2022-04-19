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
        //RecordingId and AccountId are used to gather voicemail
        public string? RecordingId { get; set; }
        public string? AccountId { get; set; }
        public string? RecordingUrl { get; set; }
        public CallType CallType { get; set; }
        public DateTime CallDate { get; set; }
        public Employee Employee { get; set; }
        public bool IsRead { get; set; }



    }
}
