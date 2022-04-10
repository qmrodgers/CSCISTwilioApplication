using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TwilioWebApplication.Models
{

    public class TwilioPhoneNumber
    {
        [Key]
        public string PhoneNumber { get; set; }
        public string FriendlyName { get; set; }
        public string TwilioSID { get; set; }

    }
    public class IncomingFromTwilio
    {
        public string CallingNumber { get; set; }
        public string TwilioNumber { get; set; }
    }
}
