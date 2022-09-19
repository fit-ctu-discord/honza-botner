using HonzaBotner.Discord.Services.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HonzaBotner.Controllers;

public abstract class BaseController : Controller
{
    private readonly IOptions<InfoOptions> _options;

    protected BaseController(IOptions<InfoOptions> options)
    {
        _options = options;
    }

    protected ActionResult Page(string message, int code)
    {
        Response.StatusCode = code;

        bool success = code is >= 200 and < 300;

        string content = string.Format(
            System.IO.File.ReadAllText("Static/auth.html"),
            success ? string.Empty : "statement--error",
            message,
            _options.Value.RepositoryUrl,
            _options.Value.IssueTrackerUrl
        );

        return Content(content, "text/html");
    }
}
