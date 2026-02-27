namespace ChatAssistant.Core.Models;

/// <summary>
/// 应用配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Claude API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 用户名（用于识别聊天记录中的自己）
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID（用于保存风格分析）
    /// </summary>
    public string UserId { get; set; } = "default_user";

    /// <summary>
    /// 数据库路径
    /// </summary>
    public string DatabasePath { get; set; } = "chatassistant.db";

    /// <summary>
    /// 是否已完成初始设置
    /// </summary>
    public bool IsConfigured { get; set; }
}
