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
        private readonly string TwilioSID;
        private readonly string TwilioSecret;




        public EmployeeController(WebApplicationContext db)
        {
            _db = db;
            _companylist = _db.Companies.ToList();
            _employeelist = _db.Employees.ToList();
            _phoneNumbers = _db.TwilioPhoneNumbers.ToList();
            TwilioSID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.Machine);
            TwilioSecret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.Machine);
            TwilioClient.Init(TwilioSID, TwilioSecret);
        }


        /// Past this point are most controllers meant to manage employees, numbers, and companies
        /// 
        /// 
        /// 
        /// Below are employee actions (which return web pages) for the controller
        

        [Route("Employees")]
        public IActionResult Employees(bool employeeDeleted = false) //employeeDeleted is set to true when HttpDelete is called on an employee
        {
            IEnumerable<Employee> Employees = _db.Employees;
            if (employeeDeleted) ViewData["deleted"] = "Employee successfully deleted.";
            return View(Employees);
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
            ViewData["numbers"] = filteredPhoneNumbers;
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


            //custom ModelState check: Our dropdown list on the submission page always throws a model state error, even when correct. So if there's only 1 error it is still actually valid. We see if the dropdown is valid by making sure the ReturnedCompanyID isn't blank.
            if (ModelState.ErrorCount == 1 && empModel.ReturnedCompanyID != null) 
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
        //Action to send User to EditEmployee View (Page)
        [Route("Employees/Edit")]
        public IActionResult EditEmployee(int empId, bool failedValidation=false) //failedValidation is set to true by the HttpPost if an error occurred (or the user didn't select company
        {
            ViewData["companies"] = _companylist;
            AddEmployeeModel addEmpModel = new AddEmployeeModel { Employee = (from Employee e in _employeelist where e.EmployeeID == empId select e).FirstOrDefault() };
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers.ToList();
            foreach (Employee e in _db.Employees)
            {
                if (e.AssignedNumber != null && e.EmployeeID != empId)
                {
                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == e.AssignedNumber);
                }
            }
            if (failedValidation) ViewData["failed"] = "Server Validation failed. Did you select the employee's company?";
            ViewData["numbers"] = _phoneNumbers;
            return View(addEmpModel);
        }

        // Posts empModel object to here when User submits form to edit an employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Employees/Edit")]
        public IActionResult EditEmployee(AddEmployeeModel empModel, int empId)
        {


            //custom ModelState check: Our dropdown list on the submission page always throws a model state error, even when correct. So if there's only 1 error it is still actually valid. We see if the dropdown is valid by making sure the ReturnedCompanyID isn't blank.
            if (ModelState.ErrorCount == 1 && empModel.ReturnedCompanyID != null)
            {

                Company company = (from Company c in _db.Companies where c.CompanyID == Convert.ToInt16(empModel.ReturnedCompanyID) select c).First(); // LINQ method
                empModel.Employee.Company = company;

                Employee dbEmp = _db.Employees.Where(e => e.EmployeeID == empId).First();

                //update employee
                dbEmp.PhoneNumber = empModel.Employee.PhoneNumber;
                dbEmp.FirstName = empModel.Employee.FirstName;
                dbEmp.LastName = empModel.Employee.LastName;
                dbEmp.Email = empModel.Employee.Email;
                dbEmp.AssignedNumber = empModel.Employee.AssignedNumber;
                dbEmp.Company = company;

                _db.SaveChanges();



                ModelState.Clear();
                

                return RedirectToAction("Employees");
            }
            return RedirectToAction("EditEmployee", new { empId = empId, failedValidation = true }) ;

        }

        //Action to send User to DeleteEmployee View (Page)
        [Route("Employees/Delete")]
        public IActionResult DeleteEmployee(int empId)
        {

            Employee emp = (from Employee e in _employeelist where e.EmployeeID == empId select e).FirstOrDefault();

            return View(emp);
        }

        // Deletes employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Employees/Delete")]
        public IActionResult DeleteEmployeeAtId(int empId)
        {

            Employee emp = _db.Employees.Find(empId);
            if (emp == null)
            {
                return NotFound();
            }
            _db.Employees.Remove(emp);
            _db.SaveChanges();

            return RedirectToAction("Employees");

        }

        /// End of Employee Views
        /// 
        /// </summary>
        /// Start of Number Views


        // Page that shows twilio numbers
        [Route("Numbers")]
        public IActionResult Numbers()
        {



            CombinedModel combinedModel = new CombinedModel();
            combinedModel.Employees = _db.Employees;
            combinedModel.TwilioPhoneNumbers = _db.TwilioPhoneNumbers;
            return View(combinedModel);
        }
        [Route("Numbers/RefreshNumbers")]
        public IActionResult RefreshNumbers()
        {
            System.Diagnostics.Debug.WriteLine("made it");
            var phoneNumbers = IncomingPhoneNumberResource.Read();
            List<TwilioPhoneNumber> updatedNumbers = new List<TwilioPhoneNumber>();
            foreach (var x in phoneNumbers)
            {
                TwilioPhoneNumber number = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where x.PhoneNumber.ToString() == t.PhoneNumber select t).FirstOrDefault();
                if (number == null)
                {
                    _db.TwilioPhoneNumbers.Add(new TwilioPhoneNumber { PhoneNumber = x.PhoneNumber.ToString(), FriendlyName = x.FriendlyName });
                }
                _db.SaveChanges();

            }
            foreach (TwilioPhoneNumber t in _db.TwilioPhoneNumbers)
            {

                if ((from IncomingPhoneNumberResource x in phoneNumbers where x.PhoneNumber.ToString() == t.PhoneNumber select x).Count() < 1)
                {
                    _db.TwilioPhoneNumbers.Remove(t);
                }
                _db.SaveChanges();
            }
            return RedirectToAction("Numbers");
        }


    }
}
