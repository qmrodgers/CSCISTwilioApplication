using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;

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


        [Route("Numbers")]
        public IActionResult Numbers()
        {
            IEnumerable<PurchasedNumber> phoneNumberList = _db.PurchasedNumbers;
            return View(phoneNumberList);
        }

        [Route("Employees")]
        public IActionResult Employees()
        {
            IEnumerable<Employee> employeeList = _db.Employees;
            return View(employeeList);
        }

    }
}
