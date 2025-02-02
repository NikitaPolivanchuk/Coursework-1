using System.Reflection;
using Webserver.Sessions;

namespace Webserver.Routing;

internal class Router
{
    private readonly RouteNode root = new("/");
    private readonly Dictionary<Type, object> callers = [];

    public void AddCaller(Type callerType, object caller) => callers[callerType] = caller;

    public void AddRoute(
        string httpMethod,
        string route,
        Type caller,
        MethodInfo methodInfo)
    {
        var current = root;
        var routeSegments = RouteParser.Parse(httpMethod, route);

        foreach (var segment in routeSegments)
        {
            if (segment.IsParameter)
            {
                if (!current.TryGetArgSubNode(segment.ParameterType, out var next))
                {
                    next = new RouteNode(segment.Name);
                    current.AddArgSubNode(segment.ParameterType, next);
                }
                current = next!;
            }
            else
            {
                if (!current.TryGetSubNode(segment.Name, out var next))
                {
                    next = new RouteNode(segment.Name);
                    current.AddSubNode(segment.Name, next);
                }
                current = next!;
            }
        }

        current.Method = new RouteMethod(caller, methodInfo);
    }

    public async Task<RoutingResult> TryRoute(
        Session session,
        string httpMethod,
        string path,
        Dictionary<string, string> inputParams)
    {
        var current = root;
        var routeSegments = RouteParser.Parse(httpMethod, path);
        var routeArgs = new Dictionary<string, object>();

        foreach (var segment in routeSegments)
        {
            if (current.TryGetSubNode(segment.Name, out current!))
            {
                continue;
            }
            if (segment.IsParameter && current.TryGetArgSubNode(segment.ParameterType, out current!))
            {
                routeArgs[current.Name] = segment.ParameterValue!;
                continue;
            }
            return RoutingResult.Empty();
        }

        return await InvokeMethod(session, current, routeArgs, inputParams);
    }

    private async Task<RoutingResult> InvokeMethod(
        Session session,
        RouteNode current,
        Dictionary<string, object> routeArgs,
        Dictionary<string, string> inputParams)
    {
        if (current.Method == null || !callers.TryGetValue(current.Method.CallerType, out var caller))
        {
            return RoutingResult.Empty();
        }

        var parameters = PrepareMethodParameters(session, current.Method.MethodInfo, routeArgs, inputParams);

        var returnValue = current.Method.MethodInfo.Invoke(caller, parameters?.ToArray());
        
        return await HandleMethodResult(returnValue, current.Method.MethodInfo.ReturnType);
    }

    private List<object>? PrepareMethodParameters(
        Session session,
        MethodInfo method,
        Dictionary<string, object> routeArgs,
        Dictionary<string, string> inputParams)
    {
        var parameters = new List<object>();

        foreach (var param in method.GetParameters())
        {
            if (param.ParameterType == typeof(Session))
            {
                parameters.Add(session);
                continue;
            }
            if (param.ParameterType == typeof(Dictionary<string, string>))
            {
                parameters.Add(inputParams);
                break;
            }
            if (routeArgs.TryGetValue(param.Name!, out object? routeValue))
            {
                parameters.Add(routeValue);
                continue;
            }
            if (inputParams.TryGetValue(param.Name!, out string? value))
            {
                parameters.Add(param.ParameterType == typeof(string[]) ? value.Split('&') : value);
                continue;
            }
            return null;
        }
        return parameters;
    }

    private async Task<RoutingResult> HandleMethodResult(object? returnValue, Type returnType)
    {
        if (returnType == typeof(Task))
        {
            return RoutingResult.Empty();
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var task = (Task)returnValue!;
            await task.ConfigureAwait(false);
            returnValue = returnType.GetProperty(nameof(Task<object>.Result))!.GetValue(task);
        }

        return new RoutingResult(true, returnValue, returnType);
    }
}
