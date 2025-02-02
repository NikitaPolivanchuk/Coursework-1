namespace Webserver.Routing;

public class RouteSegment
{
    public string Name { get; }

    public Type ParameterType { get; }

    public object? ParameterValue { get; }

    public bool IsParameter => ParameterType != typeof(string);

    public RouteSegment(string name)
    {
        Name = name;
        ParameterType = typeof(string);
        ParameterValue = null;
    }

    public RouteSegment(string name, Type parameterType)
    {
        Name = name;
        ParameterType = parameterType;
        ParameterValue = null;
    }

    public RouteSegment(string name, string value)
    {
        Name = name;
        ParameterType = typeof(string);
        ParameterValue = value;
    }
}
