﻿using Microsoft.AspNetCore.Mvc;
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
        private List<Employee> employees;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            string TwilioSID = "SK4eb6e77fcda1d92d5650409bb5d1614d";
            string TwilioSecretKey = "CXoWbQUp6uzMtIOWlTjB1jgJO8i31umV";

            TwilioClient.Init(TwilioSID, TwilioSecretKey);

            var incomingPhoneNumbers = IncomingPhoneNumberResource.Read();

            foreach (var record in incomingPhoneNumbers)
            {
                Console.WriteLine(record.PhoneNumber);
            }

            employees = new List<Employee>()
            {
                new Employee() { EmployeeID = 1, FirstName = "Steve", LastName = "Harrington", Email = "steve@steve.net", PersonalPhone = "111-123-1234", TwilioPhone = "123456789"}
            };
        }

        public IActionResult Index()
        {
            return View(employees);
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