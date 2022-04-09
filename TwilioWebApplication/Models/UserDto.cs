namespace TwilioWebApplication.Models
{
    public class UserDto
    {
        public string UserEmailID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TwilioSID { get; set; }
        public string TwilioSecretKey { get; set; }
    }
}
