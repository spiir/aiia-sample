using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("connect")]
        public IActionResult GetConnectUrl()
        {
            return Redirect(_viiaService.GetAuthUri(null).ToString());
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetAccessToken([FromBody] TokenInput input)
        {
            var result = await _viiaService.ExchangeCodeForAccessToken(input.Code);
            return Ok(new {accessToken = result.AccessToken, refreshToken = result.RefreshToken});
        }

        public class TokenInput {
            public string Code {get;set;}
            public string RefreshToken {get;set;}
        }
    }
}