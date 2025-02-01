namespace Webserver.Controllers
{
    public class EndpointAttribute : Attribute
    {
        public HttpMethod Method { get; }
        public string Route { get; }

        public EndpointAttribute(string method, string route)
        {
            Method = new HttpMethod(method);
            Route = route;
        }
    }
}
