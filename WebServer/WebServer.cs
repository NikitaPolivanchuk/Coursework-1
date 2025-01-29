using Logging;
using System.Net;
using System.Reflection;
using System.Text;
using Webserver.Content;
using Webserver.Utility;

namespace Webserver
{
    public class WebServer : HttpServer.HttpServer
    {
        private readonly Router router;
        private readonly SessionManager sessionManager;
        private readonly ConfigProvider configProvider;

        private readonly Dictionary<Type, object> services;

        private readonly Queue<Type> controllersToAdd;
        private readonly Queue<(Type, Type, bool)> servicesToAdd;

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

            servicesToAdd = new Queue<(Type, Type, bool)>();
            controllersToAdd = new Queue<Type>();

            configProvider = new ConfigProvider(Path.Combine(hostDir, configPath));
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
            servicesToAdd.Enqueue((typeof(I), typeof(S), false));
        }

        public void AddController<T>() where T : Controller
        {
            controllersToAdd.Enqueue(typeof(T));
        }

        private bool TryInvokeObject(Type type, out object? obj)
        {
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

            View.AbsolutePath = AbsolutePath("/");
            Controller.AbsolutePath = AbsolutePath("/");

            while (controllersToAdd.Count > 0)
            {
                Type controllerType = controllersToAdd.Dequeue();
                if (TryInvokeObject(controllerType, out object? controllerObj))
                {
                    Controller controller = (Controller)controllerObj!;

                    controller._server = this;

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
            var defaultPage = GetConfig("DefaultPage") ?? "/Home/Index";
            string? url = context.Request.RawUrl;

            if (url == null || url == "/")
            {
                url = defaultPage;
            }

            Session session = sessionManager.GetSession(context.Request.RemoteEndPoint);

            if (session.IsExpired(5))
            {
                session.Authorized = false;
            }

            Stream bodyStream = context.Request.InputStream;
            Dictionary<string, string>? inputParams = null;

            if (bodyStream != null && !string.IsNullOrEmpty(context.Request.ContentType))
            {
                if (context.Request.ContentType == "application/x-www-form-urlencoded")
                {
                    StringBuilder body = new StringBuilder();

                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = bodyStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        body.Append(context.Request.ContentEncoding.GetString(buffer, 0, bytesRead));
                    }
                    inputParams = body.ToString().asQuery();
                }
            }      

            ResponseAction? response = await router.TryRoute(session, new HttpMethod(context.Request.HttpMethod), url, inputParams);

            session.UpdateLastConnection();

            if (response.Status)
            {
                if (response.Function != null && response.ReturnType != null)
                {
                    if (response.ReturnType == typeof(IActionResult))
                    {
                        switch (response.Function)
                        {
                            case View:
                                View view = (View)response.Function;
                                await SendStringAsync(view.Format(), view.MimeType);
                                break;
                            
                            case Redirect:
                                Redirect redirect = (Redirect)response.Function;
                                context.Response.Redirect(redirect.Url);
                                break;

                            case Error:
                                Error error = (Error)response.Function;
                                StatusCode = error.statusCode;
                                await SendFileAsync(error.Path);
                                break;
                        }
                    }
                    else
                    {
                        await SendStringAsync(response.Function.ToString()!, "text/html");
                    }
                }
                else
                {
                    Error error = new Error(HttpStatusCode.NotFound);

                    StatusCode = error.statusCode;
                    await SendFileAsync(error.Path);
                }
            }
            else
            {
                if (RootFiles.ContainsKey(Path.GetExtension(url)))
                {
                    await SendFileAsync(RootFiles[Path.GetExtension(url)] + url);
                }
                else
                {
                    Error error = new Error(HttpStatusCode.NotFound);

                    StatusCode = error.statusCode;
                    await SendFileAsync(error.Path);
                }
            }
        }
    }
}