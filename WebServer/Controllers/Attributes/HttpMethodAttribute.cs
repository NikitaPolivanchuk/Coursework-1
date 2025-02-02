namespace Webserver.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute : Attribute
{
    public HttpMethod Method { get; }

    public string? Template { get; }

    public HttpMethodAttribute(HttpMethod method)
    {
        Method = method;
    }

    public HttpMethodAttribute(HttpMethod method, string template)
    {
        Template = template;
        Method = method;
    }
}
