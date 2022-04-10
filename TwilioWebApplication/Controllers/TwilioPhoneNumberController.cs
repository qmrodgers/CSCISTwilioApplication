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
using Twilio.TwiML;
using Microsoft.AspNetCore.Identity;

namespace TwilioWebApplication.Controllers
{
    

    [ApiController]
    [Route("Api/")]
    public class TwilioPhoneNumberController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly WebApplicationContext _db;

        public TwilioPhoneNumberController(WebApplicationContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;


        }


        [Route("Get/All")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetTwilioPhoneNumbers()
        {

            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            TwilioClient.Init(user.TwilioSID, user.TwilioSecretKey);


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
        public async Task<JsonResult> GetNumber(string callerNumber, string twilioNumber, string messageData)
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            TwilioClient.Init(user.TwilioSID, user.TwilioSecretKey);

            Regex e164formatTest = new Regex(@"^\+?[1 - 9]\d{1,14}");

            //check for number formatting
            //if (!e164formatTest.IsMatch(twilioNumber) | !e164formatTest.IsMatch(callerNumber)) return Problem($"{twilioNumber} is not a correctly formatted associated Twilio Phone Number", null, 422);

            Employee? emp = (from Employee e in _db.Employees where e.AssignedNumber == twilioNumber select e).First();

            //check if an Employee exists with an assigned twilio number matching the request.
            //if (emp == null) return Problem($"No employee found with this assigned Twilio Phone Number");


            ApiResponseModel response = new ApiResponseModel(emp);
            response.TwilioSecret = user.TwilioSecretKey;
            response.TwilioSid = user.TwilioSID;
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


        [HttpPost]
        [Route("Get/Voicemail")]
        public JsonResult GetVoicemailList(string employeeNumber, string twilioNumber)
        {


            var recordings = new Dictionary<string, string>();
            int counter = 1;
            foreach (Call call in _db.Calls)
            {
                
                if (call.Employee.PhoneNumber == employeeNumber)
                {
                    recordings.Add($"recording {counter}", call.RecordingId);
                    counter++;
                }

            }

            return Json(recordings);
            

            
        }

        [HttpGet]
        [Route("Call")]
        public ActionResult PostCallLog(string clientNumber, string employeeNumber, string sessionId, string twilioNumber, CallType? type, string? recordingSid = null)
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            TwilioClient.Init(user.TwilioSID, user.TwilioSecretKey);

            Call call = new Call();
            call.CallDate = DateTime.Now;
            call.ClientNumber = clientNumber;
            call.TwilioNumber = twilioNumber;
            call.Employee = (from Employee e in _db.Employees where e.PhoneNumber == employeeNumber select e).First();
            call.RecordingId = recordingSid;

            //CallType is sent in HttpRequest
            try
            {
                call.CallType = (CallType)type;
            }
            catch (Exception ex)
            {
                return BadRequest(new Exception("Please indicate a value in parameter 'type': 0 for missed, 1 for received, 2 for sent, 3 for failed"));
            }
            
             
            _db.Calls.Add(call);
            _db.SaveChanges();          
            return Ok();



        }


        [HttpPost]
        [Route("EmployeeToClient")]
        public ActionResult PostEmployeeCallLog(string calledNumber, string employeeNumber, string twilioNumber)
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            TwilioClient.Init(user.TwilioSID, user.TwilioSecretKey);

            Call call = new Call();
            call.CallDate = DateTime.Now;
            call.CallType = CallType.Sent;
            call.ClientNumber = calledNumber;
            call.TwilioNumber = twilioNumber;
            call.Employee = (from Employee e in _db.Employees where e.PhoneNumber == employeeNumber select e).First();
            _db.Calls.Add(call);
            _db.SaveChanges();
            var messagingResponse = new MessagingResponse();

            return Ok();



        }




    }


    
        
    
}
