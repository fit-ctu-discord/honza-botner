using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HonzaBotner
{
    public class ReverseProxyHttpsEnforcer {
        private const string ForwardedProtoHeader = "X-Forwarded-Proto";
        private readonly RequestDelegate _next;

        public ReverseProxyHttpsEnforcer(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx) {
            IHeaderDictionary h = ctx.Request.Headers;
            if (h[ForwardedProtoHeader] == string.Empty || h[ForwardedProtoHeader] == "https") {
                await _next(ctx);
            } else if (h[ForwardedProtoHeader] != "https") {
                string withHttps = $"https://{ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}";
                ctx.Response.Redirect(withHttps);
            }
        }
    }

    public static class ReverseProxyHttpsEnforcerExtensions {
        public static IApplicationBuilder UseReverseProxyHttpsEnforcer(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ReverseProxyHttpsEnforcer>();
        }
    }
}
