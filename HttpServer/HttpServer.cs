using Logging;
using System.Net;
using System.Text;

namespace HttpServer
{
    public abstract class HttpServer : IHttpServer
    {
        protected string hostUrl;

        protected string hostDir;

        private bool _running = false;

        protected HttpListener listener;
        protected HttpListenerContext context;

        protected ILogger logger;

        public HttpListenerRequest Request
        {
            get
            {
                return context.Request;
            }
        }
        public HttpStatusCode StatusCode
        {
            set
            {
                context.Response.StatusCode = (int)value;
            }
        }
        public long ContentLength
        {
            set 
            {
                context.Response.ContentLength64 = value;
            }

        }
        public string ContentType 
        {
            set
            {
                context.Response.ContentType = value;
            }
        }
        public Encoding Encoding 
        {
            set
            {
                context.Response.ContentEncoding = value;
            }
        }

        protected string AbsolutePath(string path)
        {
            path = path.StartsWith("/")
                ? $"{hostDir}{path}"
                : $"{hostDir}/{path}";
            
            return path.Replace('/', Path.DirectorySeparatorChar);
        }

        public HttpServer(string hostUrl, string hostDir, ILogger? logger = null)
        {
            this.hostUrl = string.IsNullOrEmpty(hostUrl)
                ? "http://localhost:80/"
                : hostUrl;

            this.hostDir = string.IsNullOrEmpty(hostDir)
                ? Directory.GetCurrentDirectory()
                : hostDir;

            this.logger = logger ?? Logger.ConsoleLogger;
        }

        public async Task SendFileAsync(string path, params string[] args)
        {
            path = AbsolutePath(path);

            try
            {
                if (args.Length > 0)
                {
                    await _SendFormattedFileAsync(path, args);
                }
                else
                {
                    await _SendFileAsync(path);
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                logger.Send(e);
            }
        }

        private async Task _SendFileAsync(string path)
        {
            await context.Response.OutputStream.FlushAsync();

            Stream input = new FileStream(path, FileMode.Open);

            ContentLength = input.Length;
            ContentType = Mime.GetType(path);
            Encoding = Encoding.UTF8;

            //max size of a TCP packet is 64K (65535 bytes)
            byte[] buffer = new byte[65535];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                await context.Response.OutputStream.WriteAsync(buffer, 0, bytesRead);
                await context.Response.OutputStream.FlushAsync();
            }

            input.Close();
        }

        private async Task _SendFormattedFileAsync(string path, params string[] args)
        {
            string content = File.ReadAllText(path);
            string contentType = Mime.GetType(path);

            await SendStringAsync(content, contentType, args);
        }

        public async Task SendStringAsync(string content, string contentType, params string[] args)
        {
            await context.Response.OutputStream.FlushAsync();

            try
            {
                content = string.Format(content, args);
            }
            catch (Exception e)
            {
                logger.Send(new Message("Failed to format")
                    .With(e));
            }

            ContentLength = content.Length;
            ContentType = contentType;
            Encoding = Encoding.UTF8;

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            context.Response.SendChunked = true;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await context.Response.OutputStream.FlushAsync();
        }

        public abstract Task ProcessRequest();

        public async void ProcessContextAsync(IAsyncResult result)
        {           
            if (_running) 
            {
                context = listener.EndGetContext(result);


                logger.Send(new Message("Request")
                    .With("Url", context.Request.Url)
                    .With("Method", context.Request.HttpMethod)
                    .With("Agent", context.Request.UserHostName));

                await ProcessRequest();

                logger.Send(new Message("Response")
                    .With("Status code", context.Response.StatusCode)
                    .With("Type", context.Response.ContentType)
                    .With("Length", context.Response.ContentLength64));
    
                context.Response.Close();

                listener.BeginGetContext(new AsyncCallback(ProcessContextAsync), listener);
            }
        }

        protected abstract void Start();

        public int Run()
        {
            if (_running) return 1;

            try
            {
                listener = new HttpListener();

                listener.Prefixes.Add(hostUrl);
                listener.Start();

                Start();
            }
            catch (Exception e)
            {
                logger.Send(new Message("Server initialization failed")
                    .With(e));
                listener.Close();
                return -1;
            }

            logger.Send($"Listening on {hostUrl}");
            _running = true;

            listener.BeginGetContext(new AsyncCallback(ProcessContextAsync), listener);

            while (_running)
            {
                string? command = Console.ReadLine();
                if (command == "shutdown")
                {
                    Shutdown();
                    break;
                }
                else
                {
                    logger.Send("Unknown command");
                }
            }

            listener.Close();

            return 0;
        }

        public void Shutdown()
        {
            _running = false;
        }
    }
}
