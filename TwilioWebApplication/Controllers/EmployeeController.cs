using Microsoft.AspNetCore.Mvc;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Twilio.Rest.Studio.V2;

namespace TwilioWebApplication.Controllers
{
    [Authorize]
    [Route("Manage/")]
    public class EmployeeController : Controller
    {

        private IWebHostEnvironment _webHostEnvironment;
        private readonly WebApplicationContext _db;
        private List<Company> _companylist;
        private List<Employee> _employeelist;
        private List<TwilioPhoneNumber> _phoneNumbers;
        UserManager<User> _userManager;






        public EmployeeController(WebApplicationContext db, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            //UserManager<User> hello = new UserManager<User>();

            _webHostEnvironment = environment;
            _db = db;
            _companylist = _db.Companies.Include(comp => comp.User).ToList();
            _employeelist = _db.Employees.Include(emp => emp.Company.User).ToList();
            _phoneNumbers = _db.TwilioPhoneNumbers.ToList();
            _userManager = userManager;



        }


        


        /// Past this point are most controllers meant to manage employees, numbers, and companies
        /// 
        /// 
        /// 
        /// Below are employee actions (which return web pages) for the controller
        

        [Route("Employees")]
        public IActionResult Employees(bool employeeDeleted = false) //employeeDeleted is set to true when HttpDelete is called on an employee
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            
            List<Employee>? Employees = (from e in _employeelist where e.Company.User.Id == user.Id select e).ToList();
            if (employeeDeleted) ViewData["deleted"] = "Employee successfully deleted.";
            List<int> companies = new List<int>();
            foreach (Employee employee in Employees)
            {
                if (!companies.Contains(employee.Company.CompanyID))
                {
                    companies.Add(employee.Company.CompanyID);
                }
            }
            ViewData["CompanyIds"] = companies;

            return View(Employees);
        }
        

        //Action to send User to AddNewEmployee View (Page)
        [Route("Employees/Create")]
        public IActionResult AddNewEmployee()
        {

            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            //_companylist = _companylist.Where(c => c.User.Id == user.Id) as List<Company>;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where t.TwilioSID == user.TwilioAccountSid select t).ToList();
            


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
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where t.TwilioSID == user.TwilioAccountSid select t).ToList();
            





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
            


            //custom ModelState check: Our dropdown list on the submission page always throws a model state error, even when correct. So if there's only 1 error it is still actually valid. We see if the dropdown is valid by making sure the ReturnedCompanyID isn't blank.
            if (ModelState.ErrorCount == 1 && empModel.ReturnedCompanyID != null) 
            {
                
                Company company = (from Company c in _companylist where c.CompanyID == Convert.ToInt16(empModel.ReturnedCompanyID) select c).First(); // LINQ method
                empModel.Employee.Company = company;
                try
                {
                    //Regex numberCheck = new Regex(@"^(\+1)?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$");
                    Regex numberCheck = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$");

                    MatchCollection numbermatches = numberCheck.Matches(empModel.Employee.PhoneNumber);
                    if (!numbermatches.Any())
                    {
                        throw new Exception("incorrect number format");
                    }
                    string e164number = "+";
                    if (numbermatches[0].Groups[1].Value == "") e164number += "1";
                    e164number += numbermatches[0].Groups[2].Value + numbermatches[0].Groups[3].Value + numbermatches[0].Groups[4].Value;
                    if (numbermatches[0].Groups[5].Value != "") e164number += "x" + numbermatches[0].Groups[5].Value;



                    empModel.Employee.PhoneNumber = e164number;

                    _db.Employees.Add(empModel.Employee);
                    _db.SaveChanges();

                    filteredPhoneNumbers.RemoveAll(x => x.PhoneNumber == empModel.Employee.AssignedNumber);
                }
                catch(Exception ex)
                {
                    ViewData["failed"] = "Server Validation failed. Did you format the employee's phone number correctly?";
                    return View(empModel);
                }


                ViewData["numbers"] = filteredPhoneNumbers;

                
                
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
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where t.TwilioSID == user.TwilioAccountSid select t).ToList();
            

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
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where t.TwilioSID == user.TwilioAccountSid select t).ToList();
            

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


            Employee emp = (from Employee e in _db.Employees where e.EmployeeID == empID select e).FirstOrDefault();

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
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// Start of Number Views


        // Page that shows twilio numbers
        [Route("Numbers")]
        public IActionResult Numbers()
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = _phoneNumbers.Where(p => p.TwilioSID == user.TwilioAccountSid).ToList();
            


            PairedModel combinedModel = new PairedModel();
            combinedModel.Employees = _employeelist;
            combinedModel.TwilioPhoneNumbers = _phoneNumbers;
            return View(combinedModel);
        }
        [Route("Numbers/AddNumber")]
        public IActionResult AddNumber()
        {
            User user = _userManager.GetUserAsync(User).Result;
            ISO3166.Country[] countries = ISO3166.Country.List;
            List<string> codes = new List<string>();
            foreach (ISO3166.Country c in countries)
            {
                codes.Add(c.TwoLetterCode);
            }
            ViewData["AccountSid"] = user.TwilioAccountSid;
            ViewData["AuthToken"] = user.TwilioAuthToken;
            ViewData["at"] = @"@";
            ViewData["CountryCodes"] = codes;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Numbers/AddNumber")]
        public IActionResult AddNumber(PostNewNumberModel p)
        {
            User user;
            // user and database context
            try
            {
                user = _userManager.GetUserAsync(User).Result;
            }
            catch(Exception ex)
            {
                return NotFound();
            }
            string path = this.Request.Headers.Origin;
            if (path.Contains("localhost"))
            {
                return View("Error");
            }
            //REMOVE AFTER IMPLEMENTATION

            path = path + "/Flow.json";
            Uri uri = new Uri(path);

            TwilioClient.Init(user.TwilioAccountSid, user.TwilioAuthToken);
            var incomingPhoneNumber = IncomingPhoneNumberResource.Create(
            phoneNumber: new Twilio.Types.PhoneNumber(p.PhoneNumber),
            smsApplicationSid: user.TwilioFlowSid
            ); ;
            ViewData["NumberAdded"] = "Successfully added phone Number!";
            return RedirectToAction("Numbers");
        }

        [Route("Numbers/RefreshFlow")]
        public IActionResult RefreshFlow()
        {
            User user = _userManager.GetUserAsync(User).Result;
            try
            {
                //Grabs the file Flow.json from the webroot folder! You can adjust it but be careful not to change necessary info.
                JObject objectJSON = JObject.Parse(System.IO.File.ReadAllText(@"wwwroot/Flow.json").Replace("_ReplaceURL_", "http://" + Request.Headers.Host));

                TwilioClient.Init(user.TwilioAccountSid, user.TwilioAuthToken);
                var flow = FlowResource.Create(
                commitMessage: "updated commit",
                friendlyName: $"Low Phone Volume IVR {DateTime.Now.ToShortDateString()}",
                status: FlowResource.StatusEnum.Published,
                definition: objectJSON
                );

                user.TwilioFlowSid = flow.Sid;
                _userManager.UpdateAsync(user);
                _db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"new flow sid is {flow.Sid}");
                System.Diagnostics.Debug.WriteLine(user.TwilioFlowSid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewData["Error"] = $"An error occurred while attempting to update Flow: {ex.Message}";
                return RedirectToAction("Numbers");
            }

            return RedirectToAction("RefreshNumbers");
        }

        [Route("Numbers/RefreshNumbers")]
        public IActionResult RefreshNumbers()
        {
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;

            TwilioClient.Init(user.TwilioAccountSid, user.TwilioAuthToken);

            List<IncomingPhoneNumberResource> phoneNumbers = IncomingPhoneNumberResource.Read().ToList();
            string path = $@"https://webhooks.twilio.com/v1/Accounts/{user.TwilioAccountSid}/Flows/{user.TwilioFlowSid}";

            Uri uri = new Uri(path);
            foreach (IncomingPhoneNumberResource phoneNumber in phoneNumbers)
            {
                IncomingPhoneNumberResource.Update(
                smsUrl: uri,
                voiceUrl: uri,
                pathSid: phoneNumber.Sid);
                     
            }
            
            //Go through each of User's numbers in database

            foreach ( TwilioPhoneNumber twilionumber in _db.TwilioPhoneNumbers.Where(dbnumber => dbnumber.TwilioSID == user.TwilioAccountSid))
            {
                IEnumerable<IncomingPhoneNumberResource> incomingNumber = phoneNumbers.Where(number => number.PhoneNumber.ToString() == twilionumber.PhoneNumber);
               //Check for any phone numbers that shouldn't still be in database
                if (!incomingNumber.Any()) //if db number not found in list of twilio numbers for account
                {
                    try
                    {
                        //delete phone number from any employees that somehow have it assigned.
                        foreach(Employee e in _db.Employees.Where(dbemployee => dbemployee.AssignedNumber == twilionumber.PhoneNumber))
                        {
                            e.AssignedNumber = null;
                        }
                        _db.TwilioPhoneNumbers.Remove(twilionumber); //delete number
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        ViewData["Error"] = "Could not add new Numbers to database";
                        return RedirectToAction("Numbers");
                    }

                    continue;
                }

                

                phoneNumbers.Remove(incomingNumber.First());
                
            }

            foreach(IncomingPhoneNumberResource incomingPhoneNumber in phoneNumbers)
            {
                try
                {
                    _db.TwilioPhoneNumbers.Add(new TwilioPhoneNumber() {
                        PhoneNumber = incomingPhoneNumber.PhoneNumber.ToString(),
                        FriendlyName = incomingPhoneNumber.FriendlyName,
                        TwilioSID = user.TwilioAccountSid
                    });

                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    ViewData["Error"] = "Could not add new Numbers to database";
                    return RedirectToAction("Numbers");
                }

            }

            if(_db.ChangeTracker.HasChanges()) {
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
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();



            IEnumerable<Company> Companies = _companylist;
            if (companyDeleted) ViewData["deleted"] = "Company successfully deleted.";
            ViewData["Employees"] = _db.Employees.ToList();
            return View(Companies);

        }

        //Action to send User to EditCompany View (Page)
        [Route("Companies/Edit")]
        public IActionResult EditCompany(int cID, bool failedValidation = false) //failedValidation is set to true by the HttpPost if an error occurred (or the user didn't select company
        {

            Company company = (from c in _db.Companies where c.CompanyID == cID select c).First();


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

            Company company = (from Company c in _db.Companies where c.CompanyID == cID select c).First();
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
                        e.Company = (from Company comp in _db.Companies where comp.CompanyID == c.CompanyID select comp).First();
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
            // user and database context
            User user = _userManager.GetUserAsync(User).Result;
            _companylist = (from Company c in _db.Companies where c.User.Id == user.Id select c).ToList();
            _employeelist = (from Employee e in _db.Employees where e.Company.User.Id == user.Id select e).ToList();


            _phoneNumbers = (from TwilioPhoneNumber t in _db.TwilioPhoneNumbers where t.TwilioSID == user.TwilioAccountSid select t).ToList();

            if (comp.CompanyName != null)
            {
                comp.User = (from User u in _db.Users where u.Id == user.Id select u).First();
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
