using System;
using System.Linq;
using System.Threading.Tasks;
using HonzaBotner.Discord.Services.Options;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HonzaBotner.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : BaseController
    {
        private const string AuthIdCookieName = "honza-botner-auth-id";
        private const string RolesPoolCookieName = "honza-botner-roles-pool";
        private readonly IAuthorizationService _authorizationService;
        private string RedirectUri => Url.ActionLink(nameof(Callback));

        public AuthController(IAuthorizationService authorizationService, IOptions<InfoOptions> options) : base(options)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet("Authenticate/{userId}/{pool}")]
        public async Task<ActionResult> Authenticate(ulong userId, string pool)
        {
            Response.Cookies.Append(AuthIdCookieName, userId.ToString());
            Response.Cookies.Append(RolesPoolCookieName, pool);

            string uri = await _authorizationService.GetAuthLinkAsync(RedirectUri);
            return Redirect(uri);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(nameof(Callback))]
        public async Task<ActionResult> Callback()
        {
            if (!Request.Cookies.TryGetValue(AuthIdCookieName, out string? userIdString)
                || !Request.Cookies.TryGetValue(RolesPoolCookieName, out string? pool)
                || !Request.Query.TryGetValue("code", out StringValues codes))
            {
                return BadRequest();
            }

            string? code = codes.Any() ? codes[0] : null;
            if (!ulong.TryParse(userIdString, out ulong userId) || string.IsNullOrEmpty(code) ||
                !GetRolesPool(pool, out RolesPool rolesPool))
            {
                return BadRequest();
            }

            try
            {
                string accessToken = await _authorizationService.GetAccessTokenAsync(code, RedirectUri);
                string userName = await _authorizationService.GetUserNameAsync(accessToken);

                string message = await _authorizationService.AuthorizeAsync(accessToken, userName, userId, rolesPool) switch
                {
                    IAuthorizationService.AuthorizeResult.OK => "Successfully authenticated.",
                    IAuthorizationService.AuthorizeResult.Failed => "Authentication failed.",
                    IAuthorizationService.AuthorizeResult.DifferentMember =>
                        "Authentication failed because you are registered with another auth code or another user already uses your auth code.",
                    IAuthorizationService.AuthorizeResult.UserMapError =>
                        "Authentication failed due to UserMap service failure.",
                    _ => throw new ArgumentOutOfRangeException()
                };

                return Page(message, 200);
            }
            catch (InvalidOperationException e)
            {
                return Page(e.Message, 500);
            }
        }

        private bool GetRolesPool(string? value, out RolesPool rolesPool)
        {
            switch (value?.ToLowerInvariant())
            {
                case "auth":
                    rolesPool = RolesPool.Auth;
                    break;
                case "staff":
                    rolesPool = RolesPool.Staff;
                    break;
                default:
                    rolesPool = RolesPool.Auth;
                    return false;
            }

            return true;
        }
    }
}
