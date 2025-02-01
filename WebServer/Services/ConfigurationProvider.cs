using System.Text.Json;

namespace Webserver.Services;

public class ConfigurationProvider : IConfigurationProvider
{
    private readonly Dictionary<string, string> settings = [];

    public ConfigurationProvider(string? filePath)
    {
        var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
        var fullPath = Path.Combine(projectDirectory, filePath ?? "");

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Configuration file not found at {fullPath}");
        }

        string json = File.ReadAllText(fullPath);
        settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    public string? GetSetting(string key)
    {
        return settings.TryGetValue(key, out var value) ? value : null;
    }
}
