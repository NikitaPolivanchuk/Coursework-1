﻿using System.Net;
using System.Net.Mime;
using System.Reflection;
using Webserver.Controllers;
using Webserver.Controllers.Content;
using Webserver.DependencyInjection;
using Webserver.Routing;
using Webserver.Services;
using Webserver.Sessions;
using Webserver.Utility;

namespace Webserver
{
    public class WebServer : HttpServer.HttpServer
    {
        private readonly Router router;
        private readonly SessionManager sessionManager;
        private readonly WebServerOptions options;
        private readonly IServiceCollectionProvider serviceProvider;
        private readonly IConfigurationProvider configurationProvider;

        private static readonly Dictionary<string, string> RootFiles = new Dictionary<string, string>()
        {
            {".ico", "root"},
            {".css", "root/css"},
            {".js", "root/js"},
            {".jpg", "root" }
        };

        public WebServer(WebServerOptions options)
            : base(options.HostUrl, options.HostDir, options.Logger)
        {
            router = new Router();
            sessionManager = new SessionManager();
            this.options = options;

            var configurationProvider = new ConfigurationProvider(options.ConfigPath);
            this.configurationProvider = configurationProvider;

            options.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(() => new HttpContextAccessor(context));
            options.Services.AddSingleton<IConfigurationProvider, ConfigurationProvider>(() => configurationProvider);
            serviceProvider = options.Services.Build();
        }

        protected override void Start()
        {
            Controller.AbsolutePath = AbsolutePath("/");

            foreach (Type controllerType in options.Controllers)
            {
                if (serviceProvider.GetService(controllerType) is Controller controller)
                {
                    router.AddCaller(controller.GetType(), controller);

                    foreach (MethodInfo method in controller.GetType().GetMethods())
                    {
                        var attribute = method.GetCustomAttributes(typeof(EndpointAttribute), true)
                            .FirstOrDefault() as EndpointAttribute;

                        if (attribute != null)
                        {
                            router.AddRoute(attribute.Method, attribute.Route, controllerType, method);
                        }
                    }
                }
            }
        }

        public override async Task ProcessRequest()
        {
            var url = GetRequestUrl();
            var session = sessionManager.GetSession(context.Request.RemoteEndPoint);
            
            CheckAuthorization(session);

            var inputParams = await FormDataParser.ParseAsync(Request);

            var response = await router.TryRoute(session, new HttpMethod(context.Request.HttpMethod), url, inputParams);

            session.UpdateLastConnection();

            if (response.Successful)
            {
                await HandleSuccessfulResponse(response);
            }
            else
            {
                await HandleNotFoundResponse(url);
            }
        }

        private string GetRequestUrl()
        {
            var defaultPage = configurationProvider.GetSetting("DefaultPage") ?? "/Home/Index";
            var url = context.Request.RawUrl;

            if (string.IsNullOrEmpty(url) || url == "/")
            {
                url = defaultPage;
            }
            return url;
        }

        private void CheckAuthorization(Session session)
        {
            if (!int.TryParse(configurationProvider.GetSetting("SessionExpirationHours"), out var expirationTime))
            {
                expirationTime = Session.ExpirationHours;
                logger.Send("Session expiration is missing, using default value.");
            }

            if (session.IsExpired(expirationTime))
            {
                session.Authorized = false;
            }
        }

        private async Task HandleSuccessfulResponse(ResponseAction response)
        {
            if (response.Function == null || response.ReturnType == null)
            {
                await SendErrorAsync(HttpStatusCode.NotFound);
                return;
            }

            if (response.ReturnType == typeof(IActionResult))
            {
                var actionResult = (IActionResult)response.Function;
                actionResult.Logger = logger;
                var actionContext = new ActionContext()
                {
                    Request = Request,
                    Response = context.Response,
                    AbsolutePath = AbsolutePath(string.Empty)
                };

                await actionResult.ExecuteResultAsync(actionContext);
            }
            else
            {
                await SendStringAsync(response.Function.ToString()!, MediaTypeNames.Text.Html);
            }
        }

        private async Task HandleNotFoundResponse(string url)
        {
            if (RootFiles.ContainsKey(Path.GetExtension(url)))
            {
                await SendFileAsync(RootFiles[Path.GetExtension(url)] + url);
            }
            else
            {
                await SendErrorAsync(HttpStatusCode.NotFound);
            }
        }

        private async Task SendErrorAsync(HttpStatusCode statusCode)
        {
            var error = new ErrorResult(statusCode);
            var actionContext = new ActionContext()
            {
                Request = context.Request,
                Response = context.Response,
                AbsolutePath = AbsolutePath(string.Empty)
            };
            await error.ExecuteResultAsync(actionContext);
        }
    }
}