using System.Net;

namespace Webserver.Services
{
    internal class HttpContextAccessor : IHttpContextAccessor
    {
        public HttpListenerContext Context { get; init; }

        public HttpContextAccessor(HttpListenerContext context)
        {
            Context = context;
        }
    }
}
