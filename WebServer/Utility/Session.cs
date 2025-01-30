namespace Webserver.Utility
{
    public class Session
    {
        public const int ExpirationHours = 1;

        public DateTime LastConnection {  get; private set; }
        public bool Authorized { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public Session()
        {
            Properties = new Dictionary<string, string>();
            UpdateLastConnection();
        }
        public void UpdateLastConnection()
        {
            LastConnection = DateTime.Now;
        }

        public bool IsExpired(int expirationInHours)
        {
            return (DateTime.Now - LastConnection).Hours > expirationInHours;
        }
    }
}
