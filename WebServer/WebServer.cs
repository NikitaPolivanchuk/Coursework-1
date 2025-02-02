using System;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using Webserver.Controllers;
using Webserver.Controllers.Attributes;
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
                if (!TryGetController(controllerType, out var controller))
                {
                    continue;
                }

                router.AddCaller(controller!.GetType(), controller);

                string routeBase = GetRouteBase(controllerType);
                foreach (MethodInfo method in controller.GetType().GetMethods())
                {
                    ProcessMethod(controllerType, method, routeBase);
                }
            }
        }

        private bool TryGetController(Type controllerType, out Controller? controller)
        {
            controller = serviceProvider.GetService(controllerType) as Controller;
            return controller != null;
        }

        private string GetRouteBase(Type controllerType)
        {
            var routeBase = controllerType.Name.Replace("Controller", string.Empty);
            var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
            return routeAttribute?.Name ?? routeBase;
        }

        private void ProcessMethod(Type controllerType, MethodInfo method, string routeBase)
        {
            var methodAttribute = method.GetCustomAttribute<HttpMethodAttribute>(true);
            if (methodAttribute == null)
            {
                return;
            }

            var route = $"{routeBase}/{methodAttribute.Template ?? method.Name}";
            router.AddRoute(methodAttribute.Method.Method, route, controllerType, method);
        }

        private void ProcessMethod(Type controllerType, MethodInfo method, string routeBase)
        {
            var methodAttribute = method.GetCustomAttribute<HttpMethodAttribute>(true);
            if (methodAttribute == null)
            {
                return;
            }

            var route = $"{routeBase}/{methodAttribute.Template ?? method.Name}";
            router.AddRoute(methodAttribute.Method, route, controllerType, method);
        }

        public override async Task ProcessRequest()
        {
            var path = GetRequestPath();
            var session = sessionManager.GetSession(Request.RemoteEndPoint);

            CheckAuthorization(session);

            var inputParams = await GetInputParametersAsync();

            var response = await router.TryRoute(session, Request.HttpMethod, path, inputParams);

            session.UpdateLastConnection();

            if (response.Successful)
            {
                await HandleSuccessfulResponse(response);
            }
            else
            {
                await HandleNotFoundResponse(path);
            }
        }

        private string GetRequestPath()
        {
            var defaultPage = configurationProvider.GetSetting("DefaultPage") ?? "/Home/Index";
            var path = context.Request.Url?.LocalPath;

            if (string.IsNullOrEmpty(path) || path == "/")
            {
                path = defaultPage;
            }
            return path;
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


        private async Task<Dictionary<string, string>> GetInputParametersAsync()
        {
            var bodyParams = await FormDataParser.ParseAsync(Request);
            var queryParams = Request.Url?.Query.Replace("?", string.Empty).ToDictionary();

            var inputParams = new Dictionary<string, string>();

            if (bodyParams != null)
            {
                foreach ( var param in bodyParams )
                {
                    inputParams[param.Key] = param.Value;
                }
            }

            if (queryParams != null)
            {
                foreach( var param in queryParams )
                {
                    inputParams[param.Key] = param.Value;
                }
            }

            return inputParams;
        }

        private async Task HandleSuccessfulResponse(RoutingResult response)
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