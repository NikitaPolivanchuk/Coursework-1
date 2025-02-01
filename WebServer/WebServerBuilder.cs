using System;
using Logging;
using Webserver.Controllers;
using Webserver.DependencyInjection;
using Webserver.Services;

namespace Webserver
{
    public class WebServerBuilder
    {
        private readonly WebServerOptions options = new();

        public ServiceCollection Services => options.Services;

        public IConfigurationProvider Configuration => new ConfigurationProvider(options.ConfigPath);

        public WebServerBuilder SetHostUrl(string url)
        {
            options.HostUrl = url;
            return this;
        }

        public WebServerBuilder SetHostDir(string dir)
        {
            options.HostDir = dir;
            return this;
        }

        public WebServerBuilder SetConfigPath(string path)
        {
            options.ConfigPath = path;
            return this;
        }

        public WebServerBuilder SetLogger(ILogger logger)
        {
            options.Logger = logger;
            return this;
        }

        public WebServerBuilder AddController<T>() where T : Controller
        {
            options.Controllers.Add(typeof(T));
            return this;
        }

        public IServiceCollectionProvider BuildServiceProvider()
        {
            return options.Services.Build();
        }

        public WebServer Build()
        {
            return new WebServer(options);
        }
    }
}
