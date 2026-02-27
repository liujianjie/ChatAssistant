using ChatAssistant.Core.Models;
using ChatAssistant.Core.Data;
using System.Text.RegularExpressions;

namespace ChatAssistant.Core.Services;

/// <summary>
/// 聊天记录导入服务
/// </summary>
public class ChatImporter
{
    private readonly ChatDatabase _database;

    public ChatImporter(ChatDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// 从文本文件导入微信聊天记录
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="userName">用户自己的名字（用于识别哪些是自己发的）</param>
    /// <param name="contactName">对方的名字</param>
    /// <param name="platform">平台名称（如：微信、Soul等）</param>
    public async Task<int> ImportFromTextFileAsync(
        string filePath,
        string userName,
        string contactName,
        string platform = "微信")
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("找不到指定的文件", filePath);
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        var messages = ParseWeChatText(lines, userName, contactName, platform);

        // 创建会话
        var conversationId = Guid.NewGuid().ToString();
        var conversation = new Conversation
        {
            Id = conversationId,
            ContactName = contactName,
            Platform = platform,
            MessageCount = messages.Count,
            CreatedAt = DateTime.Now
        };

        if (messages.Count > 0)
        {
            var lastMsg = messages[^1];
            conversation.LastMessage = lastMsg.Content;
            conversation.LastMessageTime = lastMsg.Timestamp;
        }

        await _database.UpsertConversationAsync(conversation);

        // 插入消息
        foreach (var message in messages)
        {
            message.ConversationId = conversationId;
            await _database.InsertMessageAsync(message);
        }

        return messages.Count;
    }

    /// <summary>
    /// 解析微信导出的文本格式
    /// 格式示例：
    /// 2024-01-15 20:30:15 张三
    /// 你好啊
    /// 2024-01-15 20:31:20 我
    /// 嗨，最近怎么样
    /// </summary>
    private List<ChatMessage> ParseWeChatText(
        string[] lines,
        string userName,
        string contactName,
        string platform)
    {
        var messages = new List<ChatMessage>();
        var timePattern = @"^(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})\s+(.+)$";

        ChatMessage? currentMessage = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var match = Regex.Match(line, timePattern);
            if (match.Success)
            {
                // 保存上一条消息
                if (currentMessage != null && !string.IsNullOrWhiteSpace(currentMessage.Content))
                {
                    messages.Add(currentMessage);
                }

                // 开始新消息
                var timestamp = DateTime.Parse(match.Groups[1].Value);
                var sender = match.Groups[2].Value.Trim();

                currentMessage = new ChatMessage
                {
                    Timestamp = timestamp,
                    Sender = sender,
                    IsFromUser = sender == userName || sender == "我",
                    Platform = platform,
                    Content = ""
                };
            }
            else if (currentMessage != null)
            {
                // 消息内容（可能多行）
                if (!string.IsNullOrEmpty(currentMessage.Content))
                    currentMessage.Content += "\n";
                currentMessage.Content += line.Trim();
            }
        }

        // 添加最后一条消息
        if (currentMessage != null && !string.IsNullOrWhiteSpace(currentMessage.Content))
        {
            messages.Add(currentMessage);
        }

        return messages;
    }

    /// <summary>
    /// 从 CSV 文件导入
    /// CSV 格式：时间,发送者,内容
    /// </summary>
    public async Task<int> ImportFromCsvAsync(
        string filePath,
        string userName,
        string contactName,
        string platform = "微信")
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("找不到指定的文件", filePath);
        }

        var lines = await File.ReadAllLinesAsync(filePath);
        var messages = new List<ChatMessage>();

        // 跳过表头
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length < 3)
                continue;

            var message = new ChatMessage
            {
                Timestamp = DateTime.Parse(parts[0]),
                Sender = parts[1].Trim(),
                Content = parts[2].Trim(),
                Platform = platform
            };

            message.IsFromUser = message.Sender == userName || message.Sender == "我";
            messages.Add(message);
        }

        // 创建会话并保存
        var conversationId = Guid.NewGuid().ToString();
        var conversation = new Conversation
        {
            Id = conversationId,
            ContactName = contactName,
            Platform = platform,
            MessageCount = messages.Count,
            CreatedAt = DateTime.Now
        };

        if (messages.Count > 0)
        {
            var lastMsg = messages[^1];
            conversation.LastMessage = lastMsg.Content;
            conversation.LastMessageTime = lastMsg.Timestamp;
        }

        await _database.UpsertConversationAsync(conversation);

        foreach (var message in messages)
        {
            message.ConversationId = conversationId;
            await _database.InsertMessageAsync(message);
        }

        return messages.Count;
    }
}
