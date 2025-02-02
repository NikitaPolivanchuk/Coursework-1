using System.Reflection;

namespace Webserver.Routing;

internal record RouteMethod (Type CallerType, MethodInfo MethodInfo);
