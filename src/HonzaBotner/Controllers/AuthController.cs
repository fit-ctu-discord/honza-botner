using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace HonzaBotner.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private const string AuthIdCookieName = "honza-botner-auth-id";
        private readonly IAuthorizationService _authorizationService;
        private string RedirectUri => Url.ActionLink(nameof(Callback));

        public AuthController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet("Authenticate/{code}")]
        public async Task<ActionResult> Authenticate(string code)
        {
            bool verificationExists = await _authorizationService.VerificationExistsAsync(code);
            if (!verificationExists)
            {
                // Verification already requested
                return BadRequest();
            }

            Response.Cookies.Append(AuthIdCookieName, code);

            string uri = await _authorizationService.GetAuthLink(RedirectUri);
            return Redirect(uri);
        }

        [HttpGet(nameof(Callback))]
        public async Task<ActionResult> Callback()
        {
            if (!Request.Cookies.TryGetValue(AuthIdCookieName, out string? verificationId)
            || !Request.Query.TryGetValue("code", out StringValues codes))
            {
                return BadRequest();
            }

            string? code = codes.Any() ? codes[0] : null;
            if (string.IsNullOrEmpty(verificationId) || string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            string accessToken = await _authorizationService.GetAccessTokenAsync(code, RedirectUri);
            string userName = await _authorizationService.GetUserNameAsync(accessToken);

            bool auth = await _authorizationService.AuthorizeAsync(accessToken, userName, verificationId);

            return Ok(auth);
        }
    }
}
