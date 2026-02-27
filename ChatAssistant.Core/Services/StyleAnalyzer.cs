using ChatAssistant.Core.Models;
using ChatAssistant.Core.Data;
using System.Text.Json;

namespace ChatAssistant.Core.Services;

/// <summary>
/// 聊天风格分析服务
/// </summary>
public class StyleAnalyzer
{
    private readonly AIClient _aiClient;
    private readonly ChatDatabase _database;

    public StyleAnalyzer(AIClient aiClient, ChatDatabase database)
    {
        _aiClient = aiClient;
        _database = database;
    }

    /// <summary>
    /// 分析用户的聊天风格
    /// </summary>
    public async Task<ChatStyle> AnalyzeUserStyleAsync(string userId)
    {
        // 从数据库获取用户发送的所有消息
        var allMessages = new List<ChatMessage>();
        var conversations = await _database.GetConversationsAsync();

        foreach (var conv in conversations)
        {
            var messages = await _database.GetMessagesAsync(conv.Id);
            allMessages.AddRange(messages.Where(m => m.IsFromUser));
        }

        if (allMessages.Count == 0)
        {
            throw new InvalidOperationException("没有找到用户的聊天记录，请先导入聊天记录");
        }

        // 提取用户消息内容
        var userMessages = allMessages.Select(m => m.Content).ToList();

        // 调用 AI 分析风格
        var analysisJson = await _aiClient.AnalyzeChatStyleAsync(userMessages);

        // 解析 JSON 结果
        ChatStyle style;
        try
        {
            var jsonDoc = JsonDocument.Parse(analysisJson);
            var root = jsonDoc.RootElement;

            style = new ChatStyle
            {
                UserId = userId,
                Description = root.GetProperty("description").GetString() ?? "",
                HumorTypes = root.GetProperty("humorTypes").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                CommonPhrases = root.GetProperty("commonPhrases").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                TopicPreferences = root.GetProperty("topicPreferences").EnumerateArray()
                    .Select(e => e.GetString() ?? "").ToList(),
                EmotionalStyle = root.GetProperty("emotionalStyle").GetString() ?? "",
                ResponseStyle = root.GetProperty("responseStyle").GetString() ?? "",
                MessageCount = allMessages.Count,
                AnalyzedAt = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"解析风格分析结果失败: {ex.Message}\n原始响应: {analysisJson}");
        }

        // 保存到数据库
        await _database.SaveChatStyleAsync(style);

        return style;
    }

    /// <summary>
    /// 获取已保存的聊天风格
    /// </summary>
    public async Task<ChatStyle?> GetChatStyleAsync(string userId)
    {
        return await _database.GetChatStyleAsync(userId);
    }
}
