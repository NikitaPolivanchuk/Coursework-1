namespace Webserver.Services
{
    public interface IConfigurationProvider
    {
        public string? GetSetting(string key);
    }
}
