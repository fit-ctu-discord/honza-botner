using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HonzaBotner.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IndexModel(ILogger<IndexModel> logger)
        {
        }
    }
}
