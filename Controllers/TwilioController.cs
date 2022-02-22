using Microsoft.AspNetCore.Mvc;
using TwilioAPI.Models;
using TwilioAPI.Data;
namespace TwilioAPI.Controllers {
    //api/twilioapi
    [Route("api/twilio/")]
    [ApiController]
    public class TwilioApiController : ControllerBase
    {
        private readonly MockTwilioRepo _repository = new MockTwilioRepo();
        [HttpGet]
        public ActionResult<IEnumerable<TwilioApi>> GetAll()
        {
            var commandItems = _repository.GetAll();
            return Ok(commandItems);
        }
        [HttpGet("getNumber/")]
        public ActionResult<TwilioApi> GetOutboundNumber([FromForm]string callingNumber)
        {   
            Console.WriteLine(callingNumber);
            var commandItem = _repository.GetOutboundNumber(callingNumber);
            return Ok(commandItem);
        }

        
    }
}