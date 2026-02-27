using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace ChatAssistant.Core.Services;

/// <summary>
/// Claude API 客户端封装
/// </summary>
public class AIClient
{
    private readonly AnthropicClient _client;
    private const string DefaultModel = "claude-sonnet-4-5-20250929";

    public AIClient(string apiKey)
    {
        _client = new AnthropicClient(apiKey);
    }

    /// <summary>
    /// 发送消息并获取回复
    /// </summary>
    public async Task<string> SendMessageAsync(string systemPrompt, string userMessage, int maxTokens = 2000)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, userMessage)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = maxTokens,
            Model = DefaultModel,
            System = new List<SystemMessage> { new SystemMessage(systemPrompt) },
            Stream = false,
            Temperature = 0.7m
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);

        return response.Message.ToString();
    }

    /// <summary>
    /// 分析聊天风格
    /// </summary>
    public async Task<string> AnalyzeChatStyleAsync(List<string> userMessages)
    {
        var systemPrompt = @"你是一个专业的聊天风格分析师。
请分析用户的聊天风格，包括：
1. 幽默类型（如：自嘲、冷幽默、梗文化、调侃等）
2. 常用词汇和短语
3. 话题偏好
4. 情感表达方式（委婉、直接、温暖等）
5. 回复风格（简短、详细、emoji使用等）

以 JSON 格式返回分析结果，格式如下：
{
  ""description"": ""整体风格描述"",
  ""humorTypes"": [""幽默类型1"", ""幽默类型2""],
  ""commonPhrases"": [""常用短语1"", ""常用短语2""],
  ""topicPreferences"": [""话题1"", ""话题2""],
  ""emotionalStyle"": ""情感风格描述"",
  ""responseStyle"": ""回复风格描述""
}";

        var userMessage = $"请分析以下聊天记录的风格：\n\n{string.Join("\n", userMessages.Take(50))}";

        return await SendMessageAsync(systemPrompt, userMessage, 3000);
    }

    /// <summary>
    /// 生成回复建议
    /// </summary>
    public async Task<string> GenerateReplySuggestionsAsync(
        string chatStyleJson,
        List<string> conversationHistory,
        string lastMessage)
    {
        var systemPrompt = $@"你是一个聊天助手。根据用户的聊天风格，为对方的消息生成 3-5 条回复建议。

用户的聊天风格：
{chatStyleJson}

要求：
1. 回复必须符合用户的个性和风格
2. 提供不同类型的回复选项（幽默、关心、深入话题、调侃等）
3. 每条回复都要自然、真实、有个性
4. 考虑对话上下文

以 JSON 数组格式返回，格式如下：
[
  {{
    ""content"": ""回复内容"",
    ""type"": ""回复类型（如：幽默、关心、调侃等）"",
    ""reason"": ""推荐理由"",
    ""confidence"": 0.85
  }}
]";

        var contextMessages = conversationHistory.TakeLast(10).ToList();
        var userMessage = $@"对话历史：
{string.Join("\n", contextMessages)}

对方最新消息：{lastMessage}

请生成 3-5 条回复建议。";

        return await SendMessageAsync(systemPrompt, userMessage, 2000);
    }
}
