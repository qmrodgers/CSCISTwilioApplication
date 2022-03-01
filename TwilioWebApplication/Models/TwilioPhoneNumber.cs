using Twilio;
using Twilio.Rest.Api.V2010.Account;
namespace TwilioWebApplication.Models
{
    public class TwilioPhoneNumber
    {
        public string PhoneNumber { get; set; }
        public string FriendlyName { get; set; }

        public static IEnumerable<TwilioPhoneNumber> GetTwilioPhoneNumbers()
        {
            List<TwilioPhoneNumber> TwilioPhoneNumbers = new List<TwilioPhoneNumber>();
            string SID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.User);
            string Secret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.User);

            TwilioClient.Init(SID, Secret);

            var phoneNumbers = IncomingPhoneNumberResource.Read();

            foreach (var phoneNumber in phoneNumbers)
            {
                TwilioPhoneNumbers.Add(new TwilioPhoneNumber
                {
                    PhoneNumber = phoneNumber.PhoneNumber.ToString(),
                    FriendlyName = phoneNumber.FriendlyName.ToString()
                });
            }


            return TwilioPhoneNumbers;

        }
    }
}
