using System.Windows;
using Microsoft.Win32;
using ChatAssistant.Core.Data;
using ChatAssistant.Core.Services;

namespace ChatAssistant.Desktop.Views;

public partial class ImportWindow : Window
{
    private readonly ChatDatabase _database;
    private readonly string _userName;

    public ImportWindow(ChatDatabase database, string userName)
    {
        InitializeComponent();
        _database = database;
        _userName = userName;
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "文本文件 (*.txt)|*.txt|CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
            Title = "选择聊天记录文件"
        };

        if (dialog.ShowDialog() == true)
        {
            FilePathBox.Text = dialog.FileName;
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FilePathBox.Text))
        {
            StatusText.Text = "请选择文件";
            return;
        }

        if (string.IsNullOrWhiteSpace(ContactNameBox.Text))
        {
            StatusText.Text = "请输入对方名字";
            return;
        }

        try
        {
            StatusText.Text = "正在导入，请稍候...";
            StatusText.Foreground = System.Windows.Media.Brushes.Blue;

            var importer = new ChatImporter(_database);
            var platform = ((System.Windows.Controls.ComboBoxItem)PlatformBox.SelectedItem).Content.ToString() ?? "微信";
            var filePath = FilePathBox.Text;
            var contactName = ContactNameBox.Text.Trim();

            int count;
            if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                count = await importer.ImportFromCsvAsync(filePath, _userName, contactName, platform);
            }
            else
            {
                count = await importer.ImportFromTextFileAsync(filePath, _userName, contactName, platform);
            }

            MessageBox.Show($"导入成功！共导入 {count} 条消息。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"导入失败: {ex.Message}";
            StatusText.Foreground = System.Windows.Media.Brushes.Red;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
