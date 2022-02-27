using Microsoft.EntityFrameworkCore;

namespace TwilioWebApplication.Models
{
    public class PurchasedNumber
    {
        public string PurchasedNumberID { get; set; }
        public string? FriendlyNumberName { get; set; }

    }
}
