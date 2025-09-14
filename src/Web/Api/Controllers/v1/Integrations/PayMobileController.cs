using System.Threading.Tasks;
using CleanTemplate.Api.Auth;
using CleanTemplate.Api.External;
using CleanTemplate.ApiFramework.Tools;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanTemplate.Api.Controllers.v1.Integrations
{
    [ApiVersion("1")]
    public class PayMobileController : BaseControllerV1
    {
        private readonly IPayMobileClient _client;
        public PayMobileController(IPayMobileClient client)
        {
            _client = client;
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Policy = nameof(PayMobileTransferScope))]
        [HttpGet("paymobile/balance")]
        [SwaggerOperation("Check account balance from PayMobile (example of service-to-service call)")]
        public async Task<IActionResult> GetBalance([FromQuery] string accountId)
        {
            var json = await _client.CheckBalanceAsync(accountId);
            return new ApiResult<string>(json);
        }

        // Helper constant for readability in attributes
        public const string PayMobileTransferScope = "Scope:paymobile.transfer";
    }
}

