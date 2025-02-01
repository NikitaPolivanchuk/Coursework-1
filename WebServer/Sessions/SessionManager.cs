using System.Net;

namespace Webserver.Sessions
{
    internal class SessionManager
    {
        private Dictionary<IPAddress, Session> sessionMap;

        public SessionManager()
        {
            sessionMap = new Dictionary<IPAddress, Session>();
        }

        public Session GetSession(IPEndPoint endPoint)
        {
            if (!sessionMap.ContainsKey(endPoint.Address))
            {
                sessionMap.Add(endPoint.Address, new Session());
            }
            return sessionMap[endPoint.Address];
        }
    }
}
