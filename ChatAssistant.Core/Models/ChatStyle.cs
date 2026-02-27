namespace ChatAssistant.Core.Models;

/// <summary>
/// 聊天风格分析结果
/// </summary>
public class ChatStyle
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 风格描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 幽默类型（如：自嘲型、冷幽默、梗文化等）
    /// </summary>
    public List<string> HumorTypes { get; set; } = new();

    /// <summary>
    /// 常用词汇和短语
    /// </summary>
    public List<string> CommonPhrases { get; set; } = new();

    /// <summary>
    /// 话题偏好（如：科技、电影、游戏等）
    /// </summary>
    public List<string> TopicPreferences { get; set; } = new();

    /// <summary>
    /// 情感表达方式（如：委婉、直接、温暖等）
    /// </summary>
    public string EmotionalStyle { get; set; } = string.Empty;

    /// <summary>
    /// 回复速度倾向（如：即时回复、深思熟虑等）
    /// </summary>
    public string ResponseStyle { get; set; } = string.Empty;

    /// <summary>
    /// 分析时间
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 分析依据的消息数量
    /// </summary>
    public int MessageCount { get; set; }
}
