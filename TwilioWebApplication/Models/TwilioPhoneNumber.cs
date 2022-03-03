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
       /* public static List<string> GetTwilioCallLogs()
        {
            string SID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.User);
            string Secret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.User);

            TwilioClient.Init(SID, Secret);

            var calls = CallResource.Read(limit: 20);
            List<string> callloglist = new List<string>();
            foreach (var record in calls)
            {
                callloglist.Add(record.DateCreated.ToString());
                Console.WriteLine(record.Sid);
            }
        }
       */
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
