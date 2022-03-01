using Microsoft.EntityFrameworkCore;
namespace TwilioWebApplication.Models
{
    public class Employee
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int EmployeeID { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AssignedNumber { get; set; }
    }
}
