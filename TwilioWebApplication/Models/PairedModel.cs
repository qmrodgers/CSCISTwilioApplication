using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TwilioWebApplication.Models
{
    public class PairedModel
    {
        public IEnumerable<TwilioPhoneNumber> TwilioPhoneNumbers { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
    }

    public class PostNewNumberModel
    {
        [MaxLength(2)]
        [DisplayName("Phone Number Company of Origin")]
        public string? ISOCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
