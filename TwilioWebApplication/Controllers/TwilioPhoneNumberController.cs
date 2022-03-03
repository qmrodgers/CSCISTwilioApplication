using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Base;
using TwilioWebApplication.Data;

namespace TwilioWebApplication.Controllers
{
    [ApiController]
    [Route("Api/")]
    public class TwilioPhoneNumberController : ControllerBase
    {
        private WebApplicationContext _db;

        public TwilioPhoneNumberController(WebApplicationContext db)
        {
            _db = db;
            string SID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.User);
            string Secret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.User);

            TwilioClient.Init(SID, Secret);
        }


        [Route("Get/")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetTwilioPhoneNumbers()
        {

            

            var phoneNumbers = IncomingPhoneNumberResource.Read();

            List<string> phoneNumbersList = new List<string>();

            foreach (var phoneNumber in phoneNumbers)
            {

                
                phoneNumbersList.Add(phoneNumber.PhoneNumber.ToString());
            }

            return Ok(phoneNumbersList);
        }

        [Route("Get/{twilioNumber}")]
        [HttpGet]
        public ActionResult<ReturnedNumberInformation> GetNumber([FromBody] IncomingFromTwilio fromTwilio)
        {
            throw new NotImplementedException();
            Employee emp = null;
            ReturnedNumberInformation returnedNumberInformation = new ReturnedNumberInformation();

            
            //temp functionality

            var emplist = (from e in _db.Employees where e.PhoneNumber == fromTwilio.CallingNumber select e).ToList();

            if (emplist.Count == 1)
            {
                
            }
            foreach (Employee e in _db.Employees)
            {
                if(e.PhoneNumber == fromTwilio.CallingNumber)
                {
                    emp = e;
                }
            }
            if (emp != null)
            {

            }

        }
        
    }
}
