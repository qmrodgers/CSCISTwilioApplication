using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Base;
using TwilioWebApplication.Data;
using System.Text.RegularExpressions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Web;

namespace TwilioWebApplication.Controllers
{
    [ApiController]
    [Route("Api/")]
    public class TwilioPhoneNumberController : Controller
    {
        private WebApplicationContext _db;

        public TwilioPhoneNumberController(WebApplicationContext db)
        {
            _db = db;
            string SID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.Machine);
            string Secret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.Machine);

            TwilioClient.Init(SID, Secret);
        }


        [Route("Get/All")]
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

        [Route("Get")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetNumber(string callerNumber, string twilioNumber)
        {
           
            

            Employee emp = (from Employee e in _db.Employees where e.AssignedNumber == twilioNumber select e).First();

            List<string> tempList = new List<string>();
            tempList.Add(nameof(callerNumber) +":" + callerNumber);
            tempList.Add(twilioNumber);

            return Ok(tempList);

            //temp functionality
            /*
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
            */

        }

        [Route("Get/Employee")]
        [HttpGet]
        public JsonResult GetNumber(string callerNumber, string twilioNumber, string messageData)
        {
            Regex e164formatTest = new Regex(@"^\+?[1 - 9]\d{1,14}");

            //check for number formatting
            //if (!e164formatTest.IsMatch(twilioNumber) | !e164formatTest.IsMatch(callerNumber)) return Problem($"{twilioNumber} is not a correctly formatted associated Twilio Phone Number", null, 422);

            Employee? emp = (from Employee e in _db.Employees where e.AssignedNumber == twilioNumber select e).First();

            //check if an Employee exists with an assigned twilio number matching the request.
            //if (emp == null) return Problem($"No employee found with this assigned Twilio Phone Number");


            ApiResponseModel response = new ApiResponseModel(emp);
            // returns early without extra processing if caller is not an employee.
            if (emp.PhoneNumber != callerNumber) return Json(response);

            response.callerIsEmployee = true;
            var wrappedResponse = new
            {
                root = response
            };

 
            
            
            return Json(response); //needs more data
            

            //temp functionality
            /*
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
            */

        }

    }
}
