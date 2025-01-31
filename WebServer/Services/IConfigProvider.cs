namespace Webserver.Services
{
    public interface IConfigProvider
    {
        public string? GetSetting(string key);
    }
}
