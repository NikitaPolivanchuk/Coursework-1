namespace Webserver.Routing
{
    internal class RoutingResult
    {
        public bool Successful { get; }
        public object? Function { get; }
        public Type? ReturnType { get; }

        public RoutingResult(bool status, object? function, Type? returnType)
        {
            Successful = status;
            Function = function;
            ReturnType = returnType;
        }

        public static RoutingResult Empty() => new(false, null, null);
    }
}
