using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TwilioWebApplication.Models
{
    public class User
    {
        [Key]
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
