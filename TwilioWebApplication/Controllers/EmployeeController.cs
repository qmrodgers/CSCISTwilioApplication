using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioWebApplication.Controllers
{
    [Route("Manage/")]
    public class EmployeeController : Controller
    {
        private readonly WebApplicationContext _db;





        public EmployeeController(WebApplicationContext db)
        {
            _db = db;
            


        }



        

        [Route("Employees")]
        public IActionResult Employees()
        {
            IEnumerable<Employee> Employees = _db.Employees;
            return View(Employees);
        }

        [Route("Numbers")]
        public IActionResult Numbers()
        {
            CombinedModel combinedModel = new CombinedModel();
            combinedModel.Employees = _db.Employees;
            combinedModel.TwilioPhoneNumbers = TwilioPhoneNumber.GetTwilioPhoneNumbers();
            return View(combinedModel);
        }

        [Route("Employees/Create")]
        public IActionResult CreateEmployees()
        {
            return View();
        }


        [HttpPost]
        [Route("Employees/Create")]
        public IActionResult CreateEmployees(Employee emp)
        {
            return View();
        }


    }
}
