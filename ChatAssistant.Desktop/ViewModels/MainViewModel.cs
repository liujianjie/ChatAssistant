using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChatAssistant.Core.Models;
using ChatAssistant.Core.Services;
using ChatAssistant.Core.Data;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatAssistant.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private ChatDatabase? _database;
    private AIClient? _aiClient;
    private StyleAnalyzer? _styleAnalyzer;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "欢迎使用 ChatAssistant";

    [ObservableProperty]
    private bool _isConfigured;

    [ObservableProperty]
    private bool _hasAnalyzedStyle;

    [ObservableProperty]
    private ObservableCollection<Conversation> _conversations = new();

    public MainViewModel()
    {
        _configService = new ConfigService();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var config = await _configService.LoadConfigAsync();
            IsConfigured = config.IsConfigured;
            UserName = config.UserName;

            if (IsConfigured)
            {
                await InitializeServicesAsync(config);
                await LoadConversationsAsync();
                await CheckStyleAnalysisAsync(config.UserId);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"初始化失败: {ex.Message}";
        }
    }

    private async Task InitializeServicesAsync(AppConfig config)
    {
        _database = new ChatDatabase(config.DatabasePath);
        await _database.InitializeAsync();

        _aiClient = new AIClient(config.ApiKey);
        _styleAnalyzer = new StyleAnalyzer(_aiClient, _database);
    }

    private async Task LoadConversationsAsync()
    {
        if (_database == null) return;

        var conversations = await _database.GetConversationsAsync();
        Conversations.Clear();
        foreach (var conv in conversations)
        {
            Conversations.Add(conv);
        }

        StatusMessage = $"已加载 {conversations.Count} 个会话";
    }

    private async Task CheckStyleAnalysisAsync(string userId)
    {
        if (_database == null) return;

        var style = await _database.GetChatStyleAsync(userId);
        HasAnalyzedStyle = style != null;

        if (HasAnalyzedStyle)
        {
            StatusMessage = $"欢迎回来，{UserName}！已加载你的聊天风格。";
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settingsWindow = new Views.SettingsWindow();
        if (settingsWindow.ShowDialog() == true)
        {
            _ = InitializeAsync();
        }
    }

    [RelayCommand]
    private void OpenImport()
    {
        if (!IsConfigured)
        {
            MessageBox.Show("请先完成设置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var importWindow = new Views.ImportWindow(_database!, UserName);
        if (importWindow.ShowDialog() == true)
        {
            _ = LoadConversationsAsync();
        }
    }

    [RelayCommand]
    private async Task AnalyzeStyle()
    {
        if (!IsConfigured || _styleAnalyzer == null)
        {
            MessageBox.Show("请先完成设置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Conversations.Count == 0)
        {
            MessageBox.Show("请先导入聊天记录", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusMessage = "正在分析你的聊天风格，请稍候...";
            var config = await _configService.LoadConfigAsync();
            var style = await _styleAnalyzer.AnalyzeUserStyleAsync(config.UserId);

            HasAnalyzedStyle = true;
            StatusMessage = "风格分析完成！";

            MessageBox.Show(
                $"分析完成！\n\n{style.Description}\n\n" +
                $"幽默类型: {string.Join(", ", style.HumorTypes)}\n" +
                $"情感风格: {style.EmotionalStyle}",
                "风格分析结果",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"分析失败: {ex.Message}";
            MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenChatAssistant()
    {
        if (!IsConfigured)
        {
            MessageBox.Show("请先完成设置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!HasAnalyzedStyle)
        {
            MessageBox.Show("请先分析聊天风格", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var config = _configService.GetConfig();
        if (config == null) return;

        var chatWindow = new Views.ChatAssistantWindow(_aiClient!, _database!, config.UserId);
        chatWindow.Show();
    }
}
