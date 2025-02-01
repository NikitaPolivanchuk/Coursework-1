using Logging;

namespace Webserver.Controllers.Content
{
    public interface IActionResult
    {
        ILogger Logger { get; set; }
        Task ExecuteResultAsync(ActionContext context);
    }
}
