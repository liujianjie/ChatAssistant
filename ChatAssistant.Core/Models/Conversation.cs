namespace ChatAssistant.Core.Models;

/// <summary>
/// 会话信息
/// </summary>
public class Conversation
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 对方名称
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// 平台（微信、Soul、小红书等）
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 最后一条消息内容
    /// </summary>
    public string LastMessage { get; set; } = string.Empty;

    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime LastMessageTime { get; set; }

    /// <summary>
    /// 消息总数
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
