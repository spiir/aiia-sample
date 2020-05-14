using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ViiaSample.Extensions;
using ViiaSample.Services;

namespace ViiaSample.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly IViiaService _viiaService;

        public MobileApiController(IViiaService viiaService)
        {
            _viiaService = viiaService;
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
                             ? await _viiaService.ExchangeCodeForAccessToken(input.Code)
                             : await _viiaService.RefreshAccessToken(input.RefreshToken);
            return Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
        }

        [HttpGet("connect")]
        public IActionResult GetConnectUrl()
        {
            return Redirect(_viiaService.GetAuthUri(null).ToString());
        }

        public class TokenInput
        {
            public string Code { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
