namespace Webserver.Utility
{
    internal class ResponseAction
    {
        public bool Status { get; }
        public object? Function { get; }
        public Type? ReturnType { get; }

        public ResponseAction(bool status, object? function, Type? returnType)
        {
            Status = status;
            Function = function;
            ReturnType = returnType;
        }
    }
}
