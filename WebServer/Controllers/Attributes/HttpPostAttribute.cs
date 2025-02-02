namespace Webserver.Controllers.Attributes;

public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute() : base(HttpMethod.Post) { }

    public HttpPostAttribute(string template): base(HttpMethod.Post, template) { }
}
