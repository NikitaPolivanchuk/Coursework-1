namespace Webserver.Content
{
    public interface IActionResult
    {
        Task ExecuteResultAsync(WebServer server);
    }
}
