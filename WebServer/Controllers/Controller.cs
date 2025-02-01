using System.Net;
using Webserver.Controllers.Content;

namespace Webserver.Controllers
{
    public abstract class Controller
    {
        public static string AbsolutePath { get; internal set; }

        protected ViewResult View(string title, string path, params object[] args)
        {
            return new ViewResult(title, path, args);
        }
        protected ViewResult View(string title, string path, string[] args)
        {
            ViewResult view = new ViewResult(title, path);
            view.Args = args;
            return view;
        }
        protected RedirectResult Redirect(string url)
        {
            return new RedirectResult(url);
        }
        protected ErrorResult Error(HttpStatusCode code)
        {
            return new ErrorResult(code);
        }
    }
}