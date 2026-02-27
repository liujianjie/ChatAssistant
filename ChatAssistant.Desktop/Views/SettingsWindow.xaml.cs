using System.Windows;
using ChatAssistant.Desktop.ViewModels;

namespace ChatAssistant.Desktop.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;

        Loaded += SettingsWindow_Loaded;
    }

    private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 加载已保存的 API Key
        ApiKeyBox.Password = _viewModel.ApiKey;
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.ApiKey = ApiKeyBox.Password;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var success = await _viewModel.Save();
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
