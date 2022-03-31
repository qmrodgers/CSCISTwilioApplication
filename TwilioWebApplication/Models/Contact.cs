using Microsoft.EntityFrameworkCore;
namespace TwilioWebApplication.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string EmployeePhone { get; set; }
        public bool Favorite { get; set; }
    }
}
