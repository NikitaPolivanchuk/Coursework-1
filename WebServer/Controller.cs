using System.Net;
using Webserver.Content;

namespace Webserver
{
    public abstract class Controller
    {
        internal WebServer _server { get; set; }

        protected HttpListenerRequest Request
        {
            get
            {
                return _server.Request;
            }
        }

        public static string AbsolutePath { get; internal set; }

        protected string? GetConfig(string key)
        {
            return _server.GetConfig(key);
        }

        protected View View(string title, string path, params object[] args)
        {
            return new View(title, path, args);
        }
        protected View View(string title, string path, string[] args)
        {
            View view = new View(title, path);
            view.Args = args;
            return view;
        }
        protected Redirect Redirect(string url)
        {
            return new Redirect(url);
        }
        protected Error Error(HttpStatusCode code)
        {
            return new Error(code);
        }
    }
}