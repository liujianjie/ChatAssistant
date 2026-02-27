using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatAssistant.Core.Models;
using ChatAssistant.Core.Services;

namespace ChatAssistant.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        _configService = new ConfigService();
        _ = LoadConfigAsync();
    }

    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();
        ApiKey = config.ApiKey;
        UserName = config.UserName;
    }

    [RelayCommand]
    public async Task<bool> Save()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            StatusMessage = "请输入 API Key";
            return false;
        }

        if (string.IsNullOrWhiteSpace(UserName))
        {
            StatusMessage = "请输入你的名字";
            return false;
        }

        try
        {
            var config = new AppConfig
            {
                ApiKey = ApiKey.Trim(),
                UserName = UserName.Trim(),
                UserId = "default_user",
                DatabasePath = "chatassistant.db",
                IsConfigured = true
            };

            await _configService.SaveConfigAsync(config);
            StatusMessage = "保存成功";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
            return false;
        }
    }
}
