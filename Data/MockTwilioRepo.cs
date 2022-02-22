using TwilioAPI.Models;
namespace TwilioAPI.Data {
    public class MockTwilioRepo : ITwilioRepo
    {
        public IEnumerable<TwilioApi> GetAll() {
            var numberSet = new List<TwilioApi> {
                new TwilioApi {Id=0, ClientNumber="1234", ClientName="Sample Client 1", EmployeeNumber="5678"},
                new TwilioApi {Id=0, ClientNumber="5555", ClientName="Sample Client 2", EmployeeNumber="8122334"},
                new TwilioApi {Id=0, ClientNumber="1234", ClientName="Sample Client 3", EmployeeNumber="5678"}
            };

            return numberSet;

        }

        public TwilioApi GetOutboundNumber(string callingNumber){
            return new TwilioApi {ClientNumber = callingNumber, EmployeeNumber = "812-677-8115", Id = 0, ClientName = "Shelby/Zach/Bryant"};
        }
    }
}