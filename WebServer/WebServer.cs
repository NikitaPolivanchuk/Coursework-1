using Logging;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using Webserver.Content;
using Webserver.Services;
using Webserver.Utility;

namespace Webserver
{
    public class WebServer : HttpServer.HttpServer
    {
        private readonly Router router;
        private readonly SessionManager sessionManager;
        private readonly IConfigProvider configProvider;

        private readonly Dictionary<Type, object> services;

        private readonly Queue<Type> controllersToAdd;
        private readonly Queue<(Type, Type)> servicesToAdd;

        private static readonly Dictionary<string, string> RootFiles = new Dictionary<string, string>()
        {
            {".ico", "root"},
            {".css", "root/css"},
            {".js", "root/js"},
            {".jpg", "root" }
        };

        public WebServer(string hostUrl, string hostDir, string configPath, ILogger? logger = null)
            : base(hostUrl, hostDir, logger)
        {
            router = new Router();
            sessionManager = new SessionManager();

            services = new Dictionary<Type, object>();

            servicesToAdd = new Queue<(Type, Type)>();
            controllersToAdd = new Queue<Type>();

            ConfigProvider.FilePath = Path.Combine(hostDir, configPath);
            configProvider = new ConfigProvider();

            AddService<IConfigProvider, ConfigProvider>();
        }

        public string? GetConfig(string key)
        {
            return configProvider.GetSetting(key);
        }

        public void AddService<I, S>() where S : I
        {
            if (!typeof(I).IsInterface)
            {
                throw new ArgumentException();
            }
            servicesToAdd.Enqueue((typeof(I), typeof(S)));
        }

        public void AddController<T>() where T : Controller
        {
            controllersToAdd.Enqueue(typeof(T));
        }

        private bool TryInvokeObject(Type type, out object? obj)
        {
            services[typeof(IHttpContextAccessor)] = new HttpContextAccessor(context);

            foreach (ConstructorInfo constructor in type.GetConstructors())
            {
                List<object> constructorParams = new List<object>();

                foreach (var parameter in constructor.GetParameters())
                {
                    if (!parameter.ParameterType.IsInterface)
                    {
                        throw new ArgumentException();
                    }

                    if (services.ContainsKey(parameter.ParameterType))
                    {
                        constructorParams.Add(services[parameter.ParameterType]);
                    }
                }
                obj = constructor.Invoke(constructorParams.ToArray());
                return true;
            }
            obj = null;
            return false;
        }

        protected override void Start()
        {
            while (servicesToAdd.Count > 0)
            {
                var v = servicesToAdd.Dequeue();
                Type interfaceType = v.Item1;
                Type implementationType = v.Item2;

                if (TryInvokeObject(implementationType, out object? instance))
                {
                    services[interfaceType] = instance!;
                }
            }

            Controller.AbsolutePath = AbsolutePath("/");

            while (controllersToAdd.Count > 0)
            {
                Type controllerType = controllersToAdd.Dequeue();
                if (TryInvokeObject(controllerType, out object? controllerObj))
                {
                    Controller controller = (Controller)controllerObj!;
                    router.AddCaller(controller.GetType(), controller);
                    
                    foreach (MethodInfo method in controller.GetType().GetMethods())
                    {
                        var attributes = method.GetCustomAttributes(typeof(Endpoint), true)
                            .Select(attr => attr as Endpoint)
                            .FirstOrDefault();

                        if (attributes != null)
                        {
                            router.AddRoute(attributes.Method, attributes.Route, controllerType, method);
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
            var defaultPage = GetConfig("DefaultPage") ?? "/Home/Index";
            var url = context.Request.RawUrl;

            if (string.IsNullOrEmpty(url) || url == "/")
            {
                url = defaultPage;
            }
            return url;
        }

        private void CheckAuthorization(Session session)
        {
            if (!int.TryParse(GetConfig("SessionExpirationHours"), out var expirationTime))
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