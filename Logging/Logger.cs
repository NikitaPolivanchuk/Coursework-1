
namespace Logging
{
    public class Logger : ILogger
    {
        protected string terminator;

        protected Action<string>? outputAction;

        public Logger(Action<string> outputAction, string terminator)
        {
            this.terminator = terminator;
            this.outputAction = outputAction;
        }

        private static Logger? _consoleLogger;
        public static Logger ConsoleLogger
        {
            get
            {
                if ( _consoleLogger == null )
                {
                    _consoleLogger = new Logger(Console.Write, Environment.NewLine);
                }
                return _consoleLogger;
            }
        }

        public void Send(string message)
        {
            outputAction?.Invoke(message + terminator);
        }

        public void Send(Message message)
        {
            Send(message.ToString());
        }

        public void Send(Exception exception)
        {
            Send(exception.Message);
        }
    }
}
