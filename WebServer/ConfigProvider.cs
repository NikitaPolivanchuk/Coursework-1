using System.Text.Json;

namespace Webserver
{
    internal class ConfigProvider
    {
        private readonly string _filePath;
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public ConfigProvider(string filePath)
        {
            _filePath = filePath;
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Configuration file not found at {_filePath}");
            }

            string json = File.ReadAllText(_filePath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        public string? GetSetting(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : null;
        }
    }
}
