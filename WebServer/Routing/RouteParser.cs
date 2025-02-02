namespace Webserver.Routing;

public static class RouteParser
{
    public static IList<RouteSegment> Parse(string httpMethod, string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var route = $"{httpMethod}/{path}";
        var segments = new List<RouteSegment>();

        foreach (var element in route.Split('/'))
        {
            if (string.IsNullOrEmpty(element)) continue;

            if (element.StartsWith('{') && element.EndsWith('}'))
            {
                var param = ParseRouteParameter(element);
                segments.Add(param);
            }
            else
            {
                segments.Add(new RouteSegment(element));
            }
        }
        return segments;
    }

    public static RouteSegment ParseRouteParameter(string routeParam)
    {
        var param = routeParam.Substring(1, routeParam.Length - 2);
        int typeIndex = param.IndexOf(':');

        string name = param;
        string typeStr = "string";

        if (typeIndex != -1)
        {
            name = param.Substring(0, typeIndex);
            typeStr = param.Substring(typeIndex + 1);
        }

        Type type = ArgType.GetArgType(typeStr).Type;
        return new RouteSegment(name, type);
    }
}
