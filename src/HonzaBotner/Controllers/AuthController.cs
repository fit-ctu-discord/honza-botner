using System;
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

        [HttpGet("Authenticate/{userId}")]
        public async Task<ActionResult> Authenticate(ulong userId)
        {
            bool isUserVerified = await _authorizationService.IsUserVerified(userId);
            if (isUserVerified)
            {
                // User already verified
                return BadRequest();
            }

            Response.Cookies.Append(AuthIdCookieName, userId.ToString());

            string uri = await _authorizationService.GetAuthLinkAsync(RedirectUri);
            return Redirect(uri);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(nameof(Callback))]
        public async Task<ActionResult> Callback()
        {
            if (!Request.Cookies.TryGetValue(AuthIdCookieName, out string? userIdString)
            || !Request.Query.TryGetValue("code", out StringValues codes))
            {
                return BadRequest();
            }

            string? code = codes.Any() ? codes[0] : null;
            if (!ulong.TryParse(userIdString, out ulong userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            string accessToken;
            string userName;
            try
            {
                accessToken = await _authorizationService.GetAccessTokenAsync(code, RedirectUri);
                userName = await _authorizationService.GetUserNameAsync(accessToken);
            }
            catch (InvalidOperationException e)
            {
                Response.StatusCode = 400;
                return Content(e.Message, "text/html");
            }

            bool auth = await _authorizationService.AuthorizeAsync(accessToken, userName, userId);

            return Ok(auth);
        }
    }
}
