using Microsoft.Data.Sqlite;
using ChatAssistant.Core.Models;
using System.Text.Json;

namespace ChatAssistant.Core.Data;

/// <summary>
/// SQLite 数据库访问类
/// </summary>
public class ChatDatabase : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public ChatDatabase(string databasePath = "chatassistant.db")
    {
        _connectionString = $"Data Source={databasePath}";
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection(_connectionString);
        await _connection.OpenAsync();

        // 创建消息表
        var createMessagesTable = @"
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ConversationId TEXT NOT NULL,
                Sender TEXT NOT NULL,
                IsFromUser INTEGER NOT NULL,
                Content TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                Platform TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            )";

        // 创建会话表
        var createConversationsTable = @"
            CREATE TABLE IF NOT EXISTS Conversations (
                Id TEXT PRIMARY KEY,
                ContactName TEXT NOT NULL,
                Platform TEXT NOT NULL,
                LastMessage TEXT NOT NULL,
                LastMessageTime TEXT NOT NULL,
                MessageCount INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL
            )";

        // 创建风格分析表
        var createStylesTable = @"
            CREATE TABLE IF NOT EXISTS ChatStyles (
                UserId TEXT PRIMARY KEY,
                Description TEXT NOT NULL,
                HumorTypes TEXT NOT NULL,
                CommonPhrases TEXT NOT NULL,
                TopicPreferences TEXT NOT NULL,
                EmotionalStyle TEXT NOT NULL,
                ResponseStyle TEXT NOT NULL,
                AnalyzedAt TEXT NOT NULL,
                MessageCount INTEGER NOT NULL
            )";

        await using var command = _connection.CreateCommand();
        command.CommandText = createMessagesTable;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createConversationsTable;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createStylesTable;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 插入聊天消息
    /// </summary>
    public async Task<int> InsertMessageAsync(ChatMessage message)
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = @"
            INSERT INTO Messages (ConversationId, Sender, IsFromUser, Content, Timestamp, Platform, CreatedAt)
            VALUES (@ConversationId, @Sender, @IsFromUser, @Content, @Timestamp, @Platform, @CreatedAt);
            SELECT last_insert_rowid();";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@ConversationId", message.ConversationId);
        command.Parameters.AddWithValue("@Sender", message.Sender);
        command.Parameters.AddWithValue("@IsFromUser", message.IsFromUser ? 1 : 0);
        command.Parameters.AddWithValue("@Content", message.Content);
        command.Parameters.AddWithValue("@Timestamp", message.Timestamp.ToString("O"));
        command.Parameters.AddWithValue("@Platform", message.Platform);
        command.Parameters.AddWithValue("@CreatedAt", message.CreatedAt.ToString("O"));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// 获取指定会话的所有消息
    /// </summary>
    public async Task<List<ChatMessage>> GetMessagesAsync(string conversationId, int limit = 100)
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = @"
            SELECT * FROM Messages
            WHERE ConversationId = @ConversationId
            ORDER BY Timestamp DESC
            LIMIT @Limit";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@ConversationId", conversationId);
        command.Parameters.AddWithValue("@Limit", limit);

        var messages = new List<ChatMessage>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            messages.Add(new ChatMessage
            {
                Id = reader.GetInt32(0),
                ConversationId = reader.GetString(1),
                Sender = reader.GetString(2),
                IsFromUser = reader.GetInt32(3) == 1,
                Content = reader.GetString(4),
                Timestamp = DateTime.Parse(reader.GetString(5)),
                Platform = reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7))
            });
        }

        messages.Reverse(); // 按时间正序返回
        return messages;
    }

    /// <summary>
    /// 保存聊天风格分析结果
    /// </summary>
    public async Task SaveChatStyleAsync(ChatStyle style)
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = @"
            INSERT OR REPLACE INTO ChatStyles
            (UserId, Description, HumorTypes, CommonPhrases, TopicPreferences, EmotionalStyle, ResponseStyle, AnalyzedAt, MessageCount)
            VALUES (@UserId, @Description, @HumorTypes, @CommonPhrases, @TopicPreferences, @EmotionalStyle, @ResponseStyle, @AnalyzedAt, @MessageCount)";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@UserId", style.UserId);
        command.Parameters.AddWithValue("@Description", style.Description);
        command.Parameters.AddWithValue("@HumorTypes", JsonSerializer.Serialize(style.HumorTypes));
        command.Parameters.AddWithValue("@CommonPhrases", JsonSerializer.Serialize(style.CommonPhrases));
        command.Parameters.AddWithValue("@TopicPreferences", JsonSerializer.Serialize(style.TopicPreferences));
        command.Parameters.AddWithValue("@EmotionalStyle", style.EmotionalStyle);
        command.Parameters.AddWithValue("@ResponseStyle", style.ResponseStyle);
        command.Parameters.AddWithValue("@AnalyzedAt", style.AnalyzedAt.ToString("O"));
        command.Parameters.AddWithValue("@MessageCount", style.MessageCount);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 获取聊天风格
    /// </summary>
    public async Task<ChatStyle?> GetChatStyleAsync(string userId)
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = "SELECT * FROM ChatStyles WHERE UserId = @UserId";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new ChatStyle
            {
                UserId = reader.GetString(0),
                Description = reader.GetString(1),
                HumorTypes = JsonSerializer.Deserialize<List<string>>(reader.GetString(2)) ?? new(),
                CommonPhrases = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new(),
                TopicPreferences = JsonSerializer.Deserialize<List<string>>(reader.GetString(4)) ?? new(),
                EmotionalStyle = reader.GetString(5),
                ResponseStyle = reader.GetString(6),
                AnalyzedAt = DateTime.Parse(reader.GetString(7)),
                MessageCount = reader.GetInt32(8)
            };
        }

        return null;
    }

    /// <summary>
    /// 获取所有会话
    /// </summary>
    public async Task<List<Conversation>> GetConversationsAsync()
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = "SELECT * FROM Conversations ORDER BY LastMessageTime DESC";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;

        var conversations = new List<Conversation>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            conversations.Add(new Conversation
            {
                Id = reader.GetString(0),
                ContactName = reader.GetString(1),
                Platform = reader.GetString(2),
                LastMessage = reader.GetString(3),
                LastMessageTime = DateTime.Parse(reader.GetString(4)),
                MessageCount = reader.GetInt32(5),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            });
        }

        return conversations;
    }

    /// <summary>
    /// 更新或插入会话信息
    /// </summary>
    public async Task UpsertConversationAsync(Conversation conversation)
    {
        if (_connection == null) throw new InvalidOperationException("数据库未初始化");

        var sql = @"
            INSERT OR REPLACE INTO Conversations
            (Id, ContactName, Platform, LastMessage, LastMessageTime, MessageCount, CreatedAt)
            VALUES (@Id, @ContactName, @Platform, @LastMessage, @LastMessageTime, @MessageCount, @CreatedAt)";

        await using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Id", conversation.Id);
        command.Parameters.AddWithValue("@ContactName", conversation.ContactName);
        command.Parameters.AddWithValue("@Platform", conversation.Platform);
        command.Parameters.AddWithValue("@LastMessage", conversation.LastMessage);
        command.Parameters.AddWithValue("@LastMessageTime", conversation.LastMessageTime.ToString("O"));
        command.Parameters.AddWithValue("@MessageCount", conversation.MessageCount);
        command.Parameters.AddWithValue("@CreatedAt", conversation.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
