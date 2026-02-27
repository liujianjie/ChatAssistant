using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ChatAssistant.Core.Services;
using ChatAssistant.Core.Data;
using ChatAssistant.Core.Models;

namespace ChatAssistant.Desktop.Views;

public partial class ChatAssistantWindow : Window
{
    private readonly AIClient _aiClient;
    private readonly ChatDatabase _database;
    private readonly string _userId;
    private readonly ReplyGenerator _replyGenerator;

    public ChatAssistantWindow(AIClient aiClient, ChatDatabase database, string userId)
    {
        InitializeComponent();
        _aiClient = aiClient;
        _database = database;
        _userId = userId;
        _replyGenerator = new ReplyGenerator(aiClient, database);
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var lastMessage = MessageBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(lastMessage))
        {
            System.Windows.MessageBox.Show("请输入对方的消息", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusText.Text = "正在生成回复建议，请稍候...";
            StatusText.Foreground = Brushes.Blue;

            // 解析对话历史
            var history = new List<string>();
            if (!string.IsNullOrWhiteSpace(HistoryBox.Text))
            {
                var lines = HistoryBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                history.AddRange(lines.Select(l => l.Trim()));
            }

            // 生成建议
            var suggestions = await _replyGenerator.GenerateSuggestionsSimpleAsync(_userId, history, lastMessage);

            // 显示建议
            DisplaySuggestions(suggestions);

            StatusText.Text = $"已生成 {suggestions.Count} 条回复建议";
            StatusText.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"生成失败: {ex.Message}";
            StatusText.Foreground = Brushes.Red;
            System.Windows.MessageBox.Show($"生成失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DisplaySuggestions(List<SuggestionReply> suggestions)
    {
        SuggestionsPanel.Children.Clear();

        if (suggestions.Count == 0)
        {
            var emptyText = new TextBlock
            {
                Text = "未能生成回复建议，请稍后重试",
                FontSize = 13,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            SuggestionsPanel.Children.Add(emptyText);
            return;
        }

        for (int i = 0; i < suggestions.Count; i++)
        {
            var suggestion = suggestions[i];

            var card = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15),
                Background = Brushes.White
            };

            var stackPanel = new StackPanel();

            // 标题行
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var numberText = new TextBlock
            {
                Text = $"#{i + 1}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var typeText = new TextBlock
            {
                Text = suggestion.Type,
                FontSize = 12,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var confidenceText = new TextBlock
            {
                Text = $"置信度: {suggestion.Confidence:P0}",
                FontSize = 11,
                Foreground = Brushes.Gray
            };

            headerPanel.Children.Add(numberText);
            headerPanel.Children.Add(typeText);
            headerPanel.Children.Add(confidenceText);

            // 回复内容
            var contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var contentText = new TextBlock
            {
                Text = suggestion.Content,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };

            contentBorder.Child = contentText;

            // 推荐理由
            var reasonText = new TextBlock
            {
                Text = $"💡 {suggestion.Reason}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // 复制按钮
            var copyButton = new Button
            {
                Content = "📋 复制",
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var content = suggestion.Content;
            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(content);
                StatusText.Text = "已复制到剪贴板";
                StatusText.Foreground = Brushes.Green;
            };

            stackPanel.Children.Add(headerPanel);
            stackPanel.Children.Add(contentBorder);
            stackPanel.Children.Add(reasonText);
            stackPanel.Children.Add(copyButton);

            card.Child = stackPanel;
            SuggestionsPanel.Children.Add(card);
        }
    }
}
