using System.Threading.Tasks;
using Aiia.Sample.Extensions;
using Aiia.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aiia.Sample.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly IAiiaService _aiiaService;

        public MobileApiController(IAiiaService aiiaService)
        {
            _aiiaService = aiiaService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetAccessToken([FromBody] TokenInput input)
        {
            if (!string.IsNullOrWhiteSpace(input.Code) && !string.IsNullOrWhiteSpace(input.RefreshToken))
            {
                return BadRequest($"Only {nameof(input.Code)} or {nameof(input.RefreshToken)} can be specified");
            }

            if (string.IsNullOrWhiteSpace(input.Code) && string.IsNullOrWhiteSpace(input.RefreshToken))
            {
                return BadRequest($"Either {nameof(input.Code)} or {nameof(input.RefreshToken)} has be specified");
            }

            var result = input.Code.IsSet()
                             ? await _aiiaService.ExchangeCodeForAccessToken(input.Code)
                             : await _aiiaService.RefreshAccessToken(input.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
        }

        [HttpGet("connect")]
        public IActionResult GetConnectUrl()
        {
            return Redirect(_aiiaService.GetAuthUri(null).ToString());
        }

        public class TokenInput
        {
            public string Code { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
