using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioWebApplication.Models
{
    public class ApiResponseModel
    {
        //public Employee Employee { get; set; }
        public string TwilioSid { get; set; }
        public string TwilioSecret { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool callerIsEmployee { get; set; }
        public bool callAccepted { get; set; }
        List<string> previousContacts { get; set; }
        List<Call> tempcallLog { get; set; }

        public ApiResponseModel(Employee emp)
        {
            callerIsEmployee = false;
            //Employee = emp;
            firstName = emp.FirstName;
            lastName = emp.LastName;
            fullName = $"{firstName} {lastName}";
            email = emp.Email;
            phone = emp.PhoneNumber;
            callAccepted = false;
            

         
        }
        
    }

    public class ApiCallLog
    {


        public int TotalCount { get; set; }
        public List<CallLog> CallLogs = new List<CallLog>();

        public ApiCallLog(IEnumerable<Call> calls)
        {
            TotalCount = calls.Count();
            var numberparser = PhoneNumbers.PhoneNumberUtil.GetInstance();
            foreach (Call call in calls)
            {
                CallLog callLog = new CallLog();
                callLog.CallDate = call.CallDate.Date.ToString();
                callLog.CallId = call.Id;
                callLog.E164Number = call.ClientNumber;
                callLog.ClientNumber = numberparser.Format(numberparser.Parse(call.ClientNumber, null), PhoneNumbers.PhoneNumberFormat.NATIONAL);
                callLog.callType = call.CallType;
                callLog.RecordingId = call.RecordingId is not null ? call.RecordingId : null;
                callLog.SessionId = call.AccountId is not null ? call.AccountId : null;
                callLog.RecordingUrl = call.RecordingUrl is not null ? call.RecordingUrl : null;
                callLog.IsRead = call.IsRead;


                CallLogs.Add(callLog);
            }
            
            
        }

        public class CallLog
        {
            public bool IsRead { get; set; }
            public string CallDate { get; set; }
            public int CallId { get; set; }
            public string? ClientNumber { get; set; }
            public CallType callType { get; set; }
            public string? RecordingId { get; set; }
            public string? SessionId { get; set; }
            public string? RecordingUrl { get; set; }
            public string? E164Number { get; set; }
        }
    }
}

