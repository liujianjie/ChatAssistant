using ChatAssistant.Core.Models;
using System.Text.Json;

namespace ChatAssistant.Core.Services;

/// <summary>
/// 配置服务
/// </summary>
public class ConfigService
{
    private const string ConfigFileName = "config.json";
    private AppConfig? _config;

    /// <summary>
    /// 加载配置
    /// </summary>
    public async Task<AppConfig> LoadConfigAsync()
    {
        if (_config != null)
            return _config;

        if (File.Exists(ConfigFileName))
        {
            try
            {
                var json = await File.ReadAllTextAsync(ConfigFileName);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                _config = new AppConfig();
            }
        }
        else
        {
            _config = new AppConfig();
        }

        return _config;
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public async Task SaveConfigAsync(AppConfig config)
    {
        _config = config;
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ConfigFileName, json);
    }

    /// <summary>
    /// 获取当前配置（同步）
    /// </summary>
    public AppConfig? GetConfig()
    {
        return _config;
    }
}
