using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioWebApplication.Models
{
    public class ApiResponseModel
    {
        //public Employee Employee { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool callerIsEmployee { get; set; }
        List<string> previousContacts { get; set; }
        List<CallLog> tempcallLog { get; set; }

        public ApiResponseModel(Employee emp)
        {
            callerIsEmployee = false;
            //Employee = emp;
            firstName = emp.FirstName;
            lastName = emp.LastName;
            email = emp.Email;
            phone = emp.PhoneNumber;
            

         
        }
        
        public void collectFromTwilio()
        {
            string SID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.Machine);
            string Secret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.Machine);
            TwilioClient.Init(SID, Secret);

            var calls = CallResource.Read();
        }
        
    }
}
