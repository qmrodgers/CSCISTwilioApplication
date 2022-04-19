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
        UserManager<User> _userManager;
        private readonly WebApplicationContext _db;
        private List<Company> _companylist;
        private List<Employee> _employeelist;
        private List<User> users;
        private List<TwilioPhoneNumber> _phoneNumbers;
        

        public TwilioPhoneNumberController(WebApplicationContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
            _companylist = _db.Companies.ToList();
            _employeelist = _db.Employees.ToList();
            users = _db.Users.ToList();


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
        public JsonResult GetNumber(string callerNumber, string twilioNumber, string messageData)
        {
            // user and database context


            Regex e164formatTest = new Regex(@"^\+?[1 - 9]\d{1,14}");

            //check for number formatting
            //if (!e164formatTest.IsMatch(twilioNumber) | !e164formatTest.IsMatch(callerNumber)) return Problem($"{twilioNumber} is not a correctly formatted associated Twilio Phone Number", null, 422);

            Employee? emp = (from Employee e in _db.Employees where e.AssignedNumber == twilioNumber select e).First();

            

            Company c = emp.Company;

            var user = (from User u in users where c.User.Id == u.Id select u).First();

            





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


        [HttpGet]
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
        [Route("Get/CallLogs")]
        public JsonResult GetCallLogs(string twilioNumber, string employeeNumber)
        {
            string temp = "hello";

            List<Call> receivedCalls = new List<Call>();
            List<Call> oldMissedCalls = new List<Call>();
            List<Call> newMissedCalls = new List<Call>();
            List<Call> outgoingCalls = new List<Call>();
            List<Call> failedCalls = new List<Call>();


            //sort calls by date
            IEnumerable<Call> dbCalls = _db.Calls.OrderByDescending(call => call.CallDate);


            foreach (Call call in dbCalls)
            {
                if (call.Employee.PhoneNumber == employeeNumber)
                {
                    //assign calls to proper categories
                    switch(call.CallType)
                    {
                        case CallType.Missed:
                            if ((DateTime.Now - call.CallDate).TotalDays < 30 && call.IsRead == false) newMissedCalls.Add(call);
                            else oldMissedCalls.Add(call);
                            break;
                        case CallType.Sent:
                            if ((DateTime.Now - call.CallDate).TotalDays < 30 && call.IsRead == false) outgoingCalls.Add(call);
                            break;
                        case CallType.Received:
                            if ((DateTime.Now - call.CallDate).TotalDays < 30 && call.IsRead == false) receivedCalls.Add(call);
                            break;
                        default: if ((DateTime.Now - call.CallDate).TotalDays < 30 && call.IsRead == false) failedCalls.Add(call); 
                            break;
                    }
                }
            }




            //import logs into custom class for correct Json conversion
            ApiCallLog receivedCallsLog = new ApiCallLog(receivedCalls);
            ApiCallLog oldMissedCallsLog = new ApiCallLog(oldMissedCalls);
            ApiCallLog newMissedCallsLog = new ApiCallLog(newMissedCalls);
            ApiCallLog outgoingCallsLog = new ApiCallLog(outgoingCalls);
            ApiCallLog failedCallsLog = new ApiCallLog(failedCalls);

            //get most recent number for redial
            string? redial = (from c in newMissedCallsLog.CallLogs orderby c.CallDate descending select c.ClientNumber).FirstOrDefault();

            //wrap objects in anonymous variable to Serialize for Twilio
            var test = new
            {
                receivedCalls = receivedCallsLog.CallLogs,
                oldMissedCalls = oldMissedCallsLog.CallLogs,
                newMissedCalls = newMissedCallsLog.CallLogs,
                outgoingCalls = outgoingCallsLog.CallLogs,
                failedCalls = failedCallsLog.CallLogs
            };

            //serialize anonymous object
            string testJson = JsonConvert.SerializeObject(test);
            
            return Json(test);
        }

        [HttpGet]
        [Route("Call")]
        public ActionResult PostCallLog(string clientNumber, string employeeNumber, string sessionId, string twilioNumber, CallType? type, string? recordingSid = null, string? recordingUrl = null)
        {


            Call call = new Call();
            call.CallDate = DateTime.Now;
            call.ClientNumber = clientNumber;
            call.TwilioNumber = twilioNumber;
            call.Employee = (from Employee e in _db.Employees where e.PhoneNumber == employeeNumber select e).First();
            call.RecordingId = recordingSid;
            call.AccountId = sessionId;
            call.RecordingUrl = recordingUrl;

            call.IsRead = false;

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
        [Route("Post/ReadCall")]
        public ActionResult MarkCallAsRead(string callId)
        {
            int cid;
            try
            {
                cid = int.Parse(callId);

            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }



            Call? call = _db.Calls.Where(c => c.Id == cid).First();

            try
            {
                
                if (call is null) return BadRequest(new NullReferenceException());
                call.IsRead = true;
                _db.SaveChanges();
            } 
            catch(Exception ex)
            {
                return StatusCode(500);
            }
            


            return Ok();



        }
        



        // These requests are important for call Queue's to work properly.
        // files in wwwroot/xml/ should be edited with upmost care

        [HttpGet]
        [Route("Xml/Enqueue")]
        public ContentResult Enqueue(string AccountSid, string FlowSid, string DomainUrl, string EmployeeNumber) {
            string xml = System.IO.File.ReadAllText("wwwroot/xml/enqueue.xml");
            xml = xml.Replace("{{AccountSid}}", AccountSid);
            xml = xml.Replace("{{FlowSid}}", FlowSid);
            xml = xml.Replace("{{DomainUrl}}", DomainUrl);
            xml = xml.Replace("{{EmployeeNumber}}", EmployeeNumber);
            return Content(xml, "text/xml");
        }

        [HttpGet]
        [Route("Xml/dialQueue")]
        public ContentResult dialQueue(string EmployeeNumber)
        {
            string xml = System.IO.File.ReadAllText("wwwroot/xml/dialQueue.xml");
            xml = xml.Replace("{{EmployeeNumber}}", EmployeeNumber);
            return Content(xml, "text/xml");
        }



    }


    
        
    
}
