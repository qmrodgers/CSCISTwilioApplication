using TwilioAPI.Models;
namespace TwilioAPI.Data {
    public interface ITwilioRepo {
        IEnumerable<TwilioApi> GetAll();
        TwilioApi GetOutboundNumber(string callingNumber);
    }
}