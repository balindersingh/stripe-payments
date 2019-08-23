using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace StripeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private IConfiguration configuration;
        public StripeController(IConfiguration configuration){
            this.configuration = configuration;
        }
        // GET api/stripe
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "createpayment", "confirmpayment" };
        }

        // GET api/stripe/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/stripe/createpayment
        [Route("[action]")]
        [HttpPost]
        public object CreatePayment([FromBody] CardInfo cardRequest)
        {
            return (new StripeInfoProvider(this.configuration)).createPayment(cardRequest);
        }
        // POST api/stripe/confirmpayment
        [Route("[action]")]
        [HttpPost]
        public object ConfirmPayment([FromBody] ConfirmPaymentInfo confirmPaymentInfo)
        {
            return (new StripeInfoProvider(this.configuration)).confirmPayment(confirmPaymentInfo);
        }
    }
}
