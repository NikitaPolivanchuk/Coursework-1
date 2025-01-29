namespace Logging
{
    public interface ILogger
    {
        public void Send(string message);
        public void Send(Message message);
        public void Send(Exception exception);

    }
}
