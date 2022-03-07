using Microsoft.EntityFrameworkCore;

namespace TwilioWebApplication.Models
{
    public enum CallType
    {
        Missed, Received, Sent
    }
    public class CallLog
    {
        public int Id { get; set; }
        public string CallFrom { get; set; }
        public string CallTo { get; set; }
        public CallType CallType { get; set; }
        public DateTime CallDate { get; set; }
        public bool Favorite { get; set; }
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }



    }
}
