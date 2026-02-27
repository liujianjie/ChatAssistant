using ChatAssistant.Core.Models;
using ChatAssistant.Core.Data;
using System.Text.Json;

namespace ChatAssistant.Core.Services;

/// <summary>
/// 回复生成服务
/// </summary>
public class ReplyGenerator
{
    private readonly AIClient _aiClient;
    private readonly ChatDatabase _database;

    public ReplyGenerator(AIClient aiClient, ChatDatabase database)
    {
        _aiClient = aiClient;
        _database = database;
    }

    /// <summary>
    /// 为指定消息生成回复建议
    /// </summary>
    public async Task<List<SuggestionReply>> GenerateSuggestionsAsync(
        string userId,
        string conversationId,
        string lastMessage)
    {
        // 获取用户风格
        var style = await _database.GetChatStyleAsync(userId);
        if (style == null)
        {
            throw new InvalidOperationException("未找到用户聊天风格，请先进行风格分析");
        }

        // 获取对话历史
        var conversationHistory = await _database.GetMessagesAsync(conversationId, 20);
        var historyTexts = conversationHistory
            .Select(m => $"{(m.IsFromUser ? "我" : "对方")}: {m.Content}")
            .ToList();

        // 将风格转为 JSON
        var styleJson = JsonSerializer.Serialize(new
        {
            style.Description,
            style.HumorTypes,
            style.CommonPhrases,
            style.TopicPreferences,
            style.EmotionalStyle,
            style.ResponseStyle
        }, new JsonSerializerOptions { WriteIndented = true });

        // 调用 AI 生成建议
        var suggestionsJson = await _aiClient.GenerateReplySuggestionsAsync(
            styleJson,
            historyTexts,
            lastMessage);

        // 解析结果
        List<SuggestionReply> suggestions;
        try
        {
            suggestions = JsonSerializer.Deserialize<List<SuggestionReply>>(suggestionsJson) ?? new();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"解析回复建议失败: {ex.Message}\n原始响应: {suggestionsJson}");
        }

        return suggestions;
    }

    /// <summary>
    /// 为指定消息生成回复建议（简化版，直接传入历史消息）
    /// </summary>
    public async Task<List<SuggestionReply>> GenerateSuggestionsSimpleAsync(
        string userId,
        List<string> conversationHistory,
        string lastMessage)
    {
        // 获取用户风格
        var style = await _database.GetChatStyleAsync(userId);
        if (style == null)
        {
            throw new InvalidOperationException("未找到用户聊天风格，请先进行风格分析");
        }

        // 将风格转为 JSON
        var styleJson = JsonSerializer.Serialize(new
        {
            style.Description,
            style.HumorTypes,
            style.CommonPhrases,
            style.TopicPreferences,
            style.EmotionalStyle,
            style.ResponseStyle
        }, new JsonSerializerOptions { WriteIndented = true });

        // 调用 AI 生成建议
        var suggestionsJson = await _aiClient.GenerateReplySuggestionsAsync(
            styleJson,
            conversationHistory,
            lastMessage);

        // 解析结果
        List<SuggestionReply> suggestions;
        try
        {
            suggestions = JsonSerializer.Deserialize<List<SuggestionReply>>(suggestionsJson) ?? new();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"解析回复建议失败: {ex.Message}\n原始响应: {suggestionsJson}");
        }

        return suggestions;
    }
}
