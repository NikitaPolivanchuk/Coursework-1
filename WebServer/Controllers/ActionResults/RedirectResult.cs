
using Logging;

namespace Webserver.Controllers.Content
{
    public class RedirectResult : IActionResult
    {
        internal string Url { get; private set; }
        public ILogger Logger { get; set; }

        internal RedirectResult(string url)
        {
            Url = url;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            context.Response.Redirect(Url);
            return Task.CompletedTask;
        }
    }
}
