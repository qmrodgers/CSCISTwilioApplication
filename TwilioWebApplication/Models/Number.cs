using System.ComponentModel.DataAnnotations;

namespace TwilioWebApplication.Models
{
    public class Number
    {
        [Key]
        public string TwilioNumber { get; set; }
        public string? FriendlyName { get; set; }
        public Employee? Employee { get; set; }



        /// <summary>
        /// might scrap these constructors
        /// </summary>
        public Number()
        {
            Employee = null;
        }
        public Number(Employee employee)
        {
            Employee = employee;
        }

    }
}
