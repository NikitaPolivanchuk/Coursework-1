namespace Webserver.Utility
{
    public class Endpoint : Attribute
    {
        public HttpMethod Method { get; }
        public string Route { get; }

        public Endpoint(string method, string route)
        {
            Method = new HttpMethod(method);
            Route = route;
        }
    }
}
