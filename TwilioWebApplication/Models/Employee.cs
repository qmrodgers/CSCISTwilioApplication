using Microsoft.EntityFrameworkCore;
namespace TwilioWebApplication.Models
{
    public class Employee
    {
        public string? FirstName { get; set; }
        public string LastName { get; set; }
        public int EmployeeID { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? AssignedNumber { get; set; }
        public DateTime? LastCall { get; set; }
        public Company Company { get; set; }


        public static string FormatPhoneNumber(string number)
        {
            string tempNum = number;
            char[] charactersToRemove = new char[] { ' ', '(', ')', '-' };
            foreach (char c in charactersToRemove)
            {
                tempNum = tempNum.Replace($"{c}", "");
            }

            while(true)
            {
                if (tempNum[0] != '0') break;

                tempNum = tempNum.Remove(0, 1);
            }

            if(tempNum.Length == 10)
            {
                tempNum = tempNum.Insert(0, "+1");
                return tempNum;
            }

            if(tempNum.Contains("+1"))
            {
                return tempNum;
            }

            throw new Exception("error in formatting phone number");
        }
    }
}
