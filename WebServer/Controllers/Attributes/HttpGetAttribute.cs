namespace Webserver.Controllers.Attributes;

public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute() : base(HttpMethod.Get) { }

    public HttpGetAttribute(string template) : base(HttpMethod.Get, template) { }
}
