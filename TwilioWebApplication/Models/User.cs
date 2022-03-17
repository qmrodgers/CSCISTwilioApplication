
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TwilioWebApplication.Models
{
    public class User
    {
        [Key]
        public string UserEmailID { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TwilioSID { get; set; }
        public string TwilioSecretKey { get; set; }

    }
}
