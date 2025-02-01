using Logging;
using Webserver.DependencyInjection;

namespace Webserver
{
    public class WebServerOptions
    {
        public string? HostUrl { get; set; }

        public string? HostDir { get; set; }

        public string? ConfigPath { get; set; }

        public ILogger? Logger { get; set; }

        public ServiceCollection Services { get; set; } = new();

        public List<Type> Controllers { get; set; } = [];
    }
}
