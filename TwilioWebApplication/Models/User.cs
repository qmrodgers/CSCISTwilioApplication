
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TwilioWebApplication.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TwilioSID { get; set; }
        public string TwilioSecretKey { get; set; }
        public string? brandURI { get; set; }
        public string? brandName { get; set; }

    }
}
