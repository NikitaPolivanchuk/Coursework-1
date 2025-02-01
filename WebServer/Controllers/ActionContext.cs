using System.Net;

namespace Webserver.Controllers
{
    public class ActionContext
    {
        public required HttpListenerRequest Request { get; set; }
        public required HttpListenerResponse Response { get; set; }
        public required string AbsolutePath { get; set; }
    }
}
