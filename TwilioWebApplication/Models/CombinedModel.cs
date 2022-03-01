namespace TwilioWebApplication.Models
{
    public class CombinedModel
    {
        public IEnumerable<TwilioPhoneNumber> TwilioPhoneNumbers { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
    }
}
