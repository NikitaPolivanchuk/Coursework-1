namespace Webserver.Routing
{
    internal class ResponseAction
    {
        public bool Successful { get; }
        public object? Function { get; }
        public Type? ReturnType { get; }

        public ResponseAction(bool status, object? function, Type? returnType)
        {
            Successful = status;
            Function = function;
            ReturnType = returnType;
        }
    }
}
