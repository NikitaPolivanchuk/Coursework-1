namespace Webserver.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute : Attribute
{
    public string Name { get; }

    public RouteAttribute(string name)
    {
        Name = name;
    }
}
