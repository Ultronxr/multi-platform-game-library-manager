using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameLibrary.Api.Services;

/// <summary>
/// UTC+8 时间格式化工具。
/// </summary>
public static class Utc8DateTimeFormatter
{
    /// <summary>
    /// 统一时间输出格式。
    /// </summary>
    public const string OutputFormat = "yyyy-MM-dd HH:mm:ss";

    private static readonly TimeZoneInfo Utc8TimeZone = ResolveUtc8TimeZone();

    /// <summary>
    /// 获取当前 UTC+8 时间（DateTimeKind.Unspecified）。
    /// </summary>
    public static DateTime NowUtc8() =>
        DateTime.SpecifyKind(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Utc8TimeZone),
            DateTimeKind.Unspecified);

    /// <summary>
    /// 将时间格式化为 UTC+8 字符串。
    /// </summary>
    /// <param name="value">原始时间。</param>
    /// <returns>UTC+8 格式化结果。</returns>
    public static string Format(DateTime value) =>
        ConvertToUtc8(value).ToString(OutputFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// 将时间字符串解析为 UTC+8 时间（DateTimeKind.Unspecified）。
    /// </summary>
    /// <param name="value">时间字符串。</param>
    /// <returns>UTC+8 时间。</returns>
    public static DateTime ParseToUtc8(string value)
    {
        if (DateTime.TryParseExact(
                value,
                OutputFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedLocal))
        {
            return DateTime.SpecifyKind(parsedLocal, DateTimeKind.Unspecified);
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return ConvertToUtc8(parsed);
        }

        throw new JsonException($"Invalid datetime value: {value}");
    }

    /// <summary>
    /// 将任意 DateTime 规范化为 UTC+8 时间（DateTimeKind.Unspecified）。
    /// </summary>
    /// <param name="value">原始时间。</param>
    /// <returns>UTC+8 时间。</returns>
    public static DateTime NormalizeToUtc8(DateTime value) => ConvertToUtc8(value);

    private static DateTime ConvertToUtc8(DateTime value)
    {
        var utc8Value = value.Kind switch
        {
            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(value, Utc8TimeZone),
            DateTimeKind.Local => TimeZoneInfo.ConvertTime(value, Utc8TimeZone),
            // 对于数据库 DATETIME 读出的 Unspecified，按“已是 UTC+8”处理，避免重复偏移。
            _ => value
        };

        return DateTime.SpecifyKind(utc8Value, DateTimeKind.Unspecified);
    }

    private static TimeZoneInfo ResolveUtc8TimeZone()
    {
        // Windows 与 Linux/macOS 的时区 ID 不同，这里做双端兼容。
        var candidates = new[] { "China Standard Time", "Asia/Shanghai" };
        foreach (var id in candidates)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
                // ignore and continue
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");
    }
}

/// <summary>
/// DateTime 序列化转换器（统一输出 UTC+8 字符串）。
/// </summary>
public sealed class Utc8DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString() ?? string.Empty;
            return Utc8DateTimeFormatter.ParseToUtc8(value);
        }

        throw new JsonException("DateTime value must be a string.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(Utc8DateTimeFormatter.Format(value));
}

/// <summary>
/// Nullable DateTime 序列化转换器（统一输出 UTC+8 字符串）。
/// </summary>
public sealed class NullableUtc8DateTimeJsonConverter : JsonConverter<DateTime?>
{
    /// <inheritdoc />
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Utc8DateTimeFormatter.ParseToUtc8(value);
        }

        throw new JsonException("DateTime value must be a string or null.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(Utc8DateTimeFormatter.Format(value.Value));
    }
}
