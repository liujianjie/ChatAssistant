namespace ChatAssistant.Core.Models;

/// <summary>
/// 聊天消息模型
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 会话ID（用于区分不同的聊天对象）
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// 发送者名称
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// 是否是用户自己发送的消息
    /// </summary>
    public bool IsFromUser { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息来源平台（微信、Soul、小红书等）
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
