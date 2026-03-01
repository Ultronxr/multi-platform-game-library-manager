using System.Text;

namespace GameLibrary.Api.Services;

/// <summary>
/// 游戏标题归一化工具。
/// </summary>
public static class TitleNormalizer
{
    /// <summary>
    /// 将标题转换为仅包含小写字母与数字的标准形式，用于重复检测。
    /// </summary>
    /// <param name="title">原始游戏标题。</param>
    /// <returns>归一化后的标题。</returns>
    public static string Normalize(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(title.Length);
        foreach (var c in title.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
