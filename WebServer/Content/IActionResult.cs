using Logging;

namespace Webserver.Content
{
    public interface IActionResult
    {
        ILogger Logger { get; set; }
        Task ExecuteResultAsync(ActionContext context);
    }
}
