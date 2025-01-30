
namespace Webserver.Content
{
    public class Redirect : IActionResult
    {
        internal string Url { get; private set; }

        internal Redirect(string url)
        {
            Url = url;
        }

        public Task ExecuteResultAsync(WebServer server)
        {
            server.Response.Redirect(Url);
            return Task.CompletedTask;
        }
    }
}
