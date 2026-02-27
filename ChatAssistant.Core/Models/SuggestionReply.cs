namespace ChatAssistant.Core.Models;

/// <summary>
/// AI 推荐的回复
/// </summary>
public class SuggestionReply
{
    /// <summary>
    /// 回复内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 回复类型（如：幽默、关心、调侃、深入话题等）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 推荐理由
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 置信度（0-1）
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// 生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
