using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

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
            _companylist = (from Company c in _db.Companies where c.User.UserEmailID == "quaidrodgers13@hotmail.com" select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.UserEmailID == "quaidrodgers13@hotmail.com" select e).ToList();
            _phoneNumbers = _db.TwilioPhoneNumbers.ToList();
            TwilioSID = Environment.GetEnvironmentVariable("TwilioProject_SID", EnvironmentVariableTarget.User);
            TwilioSecret = Environment.GetEnvironmentVariable("TwilioProject_Secret", EnvironmentVariableTarget.User);
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
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers;
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
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers;
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
                
                Company company = (from Company c in _companylist where c.CompanyID == Convert.ToInt16(empModel.ReturnedCompanyID) select c).First(); // LINQ method
                empModel.Employee.Company = company;
                try
                {
                    Regex numberCheck = new Regex(@"^(\+1)?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$");
                    if (!numberCheck.IsMatch(empModel.Employee.PhoneNumber))
                    {
                        throw new Exception("incorrect number format");
                    }
                    empModel.Employee.PhoneNumber = Employee.FormatPhoneNumber(empModel.Employee.PhoneNumber);
                }
                catch(Exception ex)
                {
                    ViewData["failed"] = "Server Validation failed. Did you format the employee's phone number correctly?";
                    return View(empModel);
                }
                 
                
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
        public IActionResult EditEmployee(int empID) //failedValidation is set to true by the HttpPost if an error occurred (or the user didn't select company
        {
            ViewData["companies"] = _companylist;
            AddEmployeeModel addEmpModel = new AddEmployeeModel { Employee = (from Employee e in _employeelist where e.EmployeeID == empID select e).FirstOrDefault() };
            List<TwilioPhoneNumber> filteredPhoneNumbers = _phoneNumbers.ToList();
            foreach (Employee e in _db.Employees)
            {
                if (e.AssignedNumber != null && e.EmployeeID != empID)
                {
                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == e.AssignedNumber);
                }
            }
            if (TempData["failed"] is not null) ViewData["failed"] = TempData["failed"];
            //ViewData["numbers"] = _phoneNumbers;
            ViewData["numbers"] = filteredPhoneNumbers;
            return View(addEmpModel);
        }

        // Posts empModel object to here when User submits form to edit an employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Employees/Edit")]
        public IActionResult EditEmployee(AddEmployeeModel empModel, int empID)
        {


            //custom ModelState check: Our dropdown list on the submission page always throws a model state error, even when correct. So if there's only 1 error it is still actually valid. We see if the dropdown is valid by making sure the ReturnedCompanyID isn't blank.
            if (ModelState.ErrorCount == 1 && empModel.ReturnedCompanyID != null)
            {

                Company company = (from Company c in _companylist where c.CompanyID == Convert.ToInt16(empModel.ReturnedCompanyID) select c).First(); // LINQ method
                empModel.Employee.Company = company;

                Employee dbEmp = _db.Employees.Where(e => e.EmployeeID == empID).First();

                //update employee
                try
                {
                    Regex numberCheck = new Regex(@"^(\+1)?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$");
                    if (!numberCheck.IsMatch(empModel.Employee.PhoneNumber))
                    {
                        throw new Exception("incorrect number format");
                    }
                    dbEmp.PhoneNumber = Employee.FormatPhoneNumber(empModel.Employee.PhoneNumber);
                }
                catch (Exception ex)
                {
                    TempData["failed"] = "Server Validation failed. Please format your employee number in a regular U.S. format";
                    return RedirectToAction("EditEmployee", new { empID = empID, failedValidation = true });
                }
                
                dbEmp.FirstName = empModel.Employee.FirstName;
                dbEmp.LastName = empModel.Employee.LastName;
                dbEmp.Email = empModel.Employee.Email;
                dbEmp.AssignedNumber = empModel.Employee.AssignedNumber;
                dbEmp.Company = company;

                _db.SaveChanges();



                ModelState.Clear();
                

                return RedirectToAction("Employees");
            }
            TempData["failed"] = "Server Validation failed. Did you select the employee's company?";
            return RedirectToAction("EditEmployee", new { empID = empID, failedValidation = true }) ;

        }

        //Action to send User to DeleteEmployee View (Page)
        [Route("Employees/Delete")]
        public IActionResult DeleteEmployee(int empID)
        {

            Employee emp = (from Employee e in _employeelist where e.EmployeeID == empID select e).FirstOrDefault();

            return View(emp);
        }

        // Deletes employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Employees/Delete")]
        public IActionResult DeleteEmployeeAtId(int empID)
        {

            Employee emp = _db.Employees.Find(empID);
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
            combinedModel.Employees = _employeelist;
            combinedModel.TwilioPhoneNumbers = _db.TwilioPhoneNumbers;
            return View(combinedModel);
        }
        [Route("Numbers/RefreshNumbers")]
        public IActionResult RefreshNumbers()
        {

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

        /// End of Number Views
        /// 
        /// </summary>
        /// Start of Company Views
        /// 


        [Route("Companies")]
        public IActionResult Companies(bool companyDeleted = false) //employeeDeleted is set to true when HttpDelete is called on an employee
        {
            IEnumerable<Company> Companies = _companylist;
            if (companyDeleted) ViewData["deleted"] = "Company successfully deleted.";
            ViewData["Employees"] = _db.Employees.ToList();
            return View(Companies);

        }

        //Action to send User to EditCompany View (Page)
        [Route("Companies/Edit")]
        public IActionResult EditCompany(int cID, bool failedValidation = false) //failedValidation is set to true by the HttpPost if an error occurred (or the user didn't select company
        {

            Company company = (from c in _companylist where c.CompanyID == cID select c).First();


            if (failedValidation) ViewData["failed"] = "Server Validation failed";
            //ViewData["numbers"] = _phoneNumbers;

            return View(company);
        }

        // Posts empModel object to here when User submits form to edit an employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Companies/Edit")]
        public IActionResult EditCompany(Company company, int cID)
        {
            Company c = _db.Companies.Where(c => c.CompanyID == cID).First();

            //custom ModelState check: Ours does not return a companies assigned user which throws an error, which is corrected below
            if (ModelState.ErrorCount <= 1)
            {
                
                c.CompanyName = company.CompanyName;

                _db.SaveChanges();


                ModelState.Clear();

                return RedirectToAction("Companies");
            }
            return RedirectToAction("EditCompany", new { cID = cID, failedValidation = true });

        }
        //Action to send User to DeleteCompany View (Page)
        [Route("Companies/Delete")]
        public IActionResult DeleteCompany(int cID)
        {

            Company company = (from Company c in _companylist where c.CompanyID == cID select c).First();
            List<Company> companiesWithRemoved = _db.Companies.ToList();
            companiesWithRemoved.Remove(company);
            ViewData["companies"] = companiesWithRemoved;
            ViewData["employees"] = _db.Employees.ToList();
            return View(company);
        }


        // Deletes employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Companies/Delete")]
        public IActionResult DeleteCompanyAtId(Company? c, int cID)
        {

            Company company = _db.Companies.Find(cID);

            if (ModelState.Count == 4)
            {
                foreach (Employee e in _db.Employees)
                {
                    if (e.Company.CompanyID == cID)
                    {
                        e.Company = (from Company comp in _companylist where comp.CompanyID == c.CompanyID select comp).First();
                    }
                }
            }
            

            if (company == null)
            {
                return NotFound();
            }
            _db.Companies.Remove(company);
            _db.SaveChanges();



            return RedirectToAction("Companies");

        }

        //Action to send User to AddNewEmployee View (Page)
        [Route("Companies/Create")]
        public IActionResult AddNewCompany()
        {
            
            return View();
        }

        // Posts empModel object to here when User submits form to add new employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Companies/Create")]
        public IActionResult AddNewCompany(Company comp)
        {
            if (comp.CompanyName != null)
            {
                comp.User = (from User u in _db.Users where u.UserEmailID == "quaidrodgers13@hotmail.com" select u).First();
                _db.Companies.Add(comp);

                _db.SaveChanges();

                ViewData["submitted"] = $"Successfully added new company \"{comp.CompanyName}\".";
                return View();
            }
            else
            {
                ViewData["failed"] = "Failed to create new company";
                return View();
            }
            

        }


    }


}
