using System.Text.Json;

namespace Webserver.Services
{
    internal class ConfigProvider : IConfigProvider
    {
        public static string? FilePath { get; set; }
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public ConfigProvider()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException($"Configuration file not found at {FilePath}");
            }

            string json = File.ReadAllText(FilePath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        public string? GetSetting(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : null;
        }
    }
}
