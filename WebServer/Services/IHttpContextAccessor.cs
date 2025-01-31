using System.Net;

namespace Webserver.Services
{
    public interface IHttpContextAccessor
    {
        HttpListenerContext Context { get; init; }
    }
}
