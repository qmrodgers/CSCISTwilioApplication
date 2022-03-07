using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace TwilioWebApplication.Controllers
{
    [Route("Manage/")]
    public class EmployeeController : Controller
    {
        private readonly WebApplicationContext _db;
        private readonly List<Company> _companylist;
        private readonly List<Employee> _employeelist;
        private List<TwilioPhoneNumber> _phoneNumbers;




        public EmployeeController(WebApplicationContext db)
        {
            _db = db;
            _companylist = _db.Companies.ToList();
            _employeelist = _db.Employees.ToList();
            _phoneNumbers = TwilioPhoneNumber.GetTwilioPhoneNumbers().ToList(); //needs to be changed, this constructor is called every time we go to a route in the controller

        }



        

        [Route("Employees")]
        public IActionResult Employees()
        {
            IEnumerable<Employee> Employees = _db.Employees;
            return View(Employees);
        }
        // Page that shows twilio numbers
        [Route("Numbers")]
        public IActionResult Numbers()
        {
            CombinedModel combinedModel = new CombinedModel();
            combinedModel.Employees = _db.Employees;
            combinedModel.TwilioPhoneNumbers = _phoneNumbers;
            return View(combinedModel);
        }

        //Action to send User to AddNewEmployee View (Page)
        [Route("Employees/Create")]
        public IActionResult AddNewEmployee()
        {
            ViewData["companies"] = _companylist;
            ViewData["employees"] = _employeelist;
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers.ToList();
            foreach (Employee e in _db.Employees)
            {
                if (e.AssignedNumber != null)
                {
                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == e.AssignedNumber);
                }
            }
            ViewData["numbers"] = _phoneNumbers;
            return View();
        }

        // Posts empModel object to here when User submits form to add new employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Employees/Create")]
        public IActionResult AddNewEmployee(AddEmployeeModel empModel)
        {
            

            ViewData["companies"] = _companylist;
            ViewData["employees"] = _employeelist;
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers.ToList();
            foreach (Employee e in _db.Employees)
            {
                if (e.AssignedNumber != null)
                {
                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == e.AssignedNumber);
                }
            }
            ViewData["numbers"] = filteredPhoneNumbers;



            if (ModelState.ErrorCount == 1 && empModel.ReturnedCompanyID != "") //custom ModelState check: Our dropdown list on the submission page always throws a model state error, even when correct. So if there's only 1 error it is still actually valid. We see if the dropdown is valid by making sure the ReturnedCompanyID isn't blank.
            {
                
                Company company = (from Company c in _db.Companies where c.CompanyID == Convert.ToInt16(empModel.ReturnedCompanyID) select c).First(); // LINQ method
                empModel.Employee.Company = company;
                
                _db.Employees.Add(empModel.Employee);
                _db.SaveChanges();

                //ensures the filteredPhoneNumbers are up to date
                if(empModel.Employee.AssignedNumber != null)
                {
                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == empModel.Employee.AssignedNumber);
                    ViewData["numbers"] = filteredPhoneNumbers;
                }
                
                
                ModelState.Clear();
                ViewData["submitted"] = $"Successfully added {empModel.Employee.FirstName} {empModel.Employee.LastName} as an employee of {company.CompanyName}";
                
                return View();
            }
            ViewData["failed"] = "Server Validation failed";
            return View(empModel);
            
        }
        

    }
}
