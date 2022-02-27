using Microsoft.EntityFrameworkCore;
namespace TwilioWebApplication.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string? TwilioSID { get; set; }
        public string? TwilioSecretKey { get; set; }
    }
}
