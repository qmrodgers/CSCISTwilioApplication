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


    }

    //supposed to get sent back to twilio http request
    public class ReturnedNumberInformation
    {
        //needs more implementation
        public string OutgoingNumber { get; set; }
        public Dictionary<string, string> CallLog { get; set; }
    }

    public class IncomingFromTwilio
    {
        public string CallingNumber { get; set; }
        public string TwilioNumber { get; set; }
    }
}
