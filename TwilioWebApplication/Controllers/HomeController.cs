using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Twilio Account ID and Secret Key (Will later pull this info from Database)
        //private List<TwilioNumber> numbers;
        string TwilioSID = "SK4eb6e77fcda1d92d5650409bb5d1614d";
        string TwilioSecretKey = "CXoWbQUp6uzMtIOWlTjB1jgJO8i31umV";
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;


            TwilioClient.Init(TwilioSID, TwilioSecretKey);
            var incomingPhoneNumbers = IncomingPhoneNumberResource.Read(limit: 20);
            

            Console.WriteLine("This ran");
            foreach (var record in incomingPhoneNumbers)
            {
                Console.WriteLine("Hello");
                Console.WriteLine(record.PhoneNumber);
            }

            /*numbers = new List<TwilioNumber>()
            {

                

            };*/
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}