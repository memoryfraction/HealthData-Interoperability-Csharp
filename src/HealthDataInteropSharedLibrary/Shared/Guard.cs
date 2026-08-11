namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Parameter validation helpers (CodeStandard.md: all public methods start with guard clauses).
/// [CN] 参数验证辅助方法（CodeStandard.md要求：所有public方法以保护子句开始）。
/// </summary>
public static class Guard
{
    /// <summary>
    /// [EN] Throw ArgumentNullException if value is null.
    /// [CN] 如果值为null则抛出ArgumentNullException。
    /// </summary>
    public static void NotNull(object? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
    }

    /// <summary>
    /// [EN] Throw for null or empty strings. Null yields ArgumentNullException, empty yields ArgumentException.
    /// [CN] 对null或空字符串抛出异常。null抛出ArgumentNullException，空字符串抛出ArgumentException。
    /// </summary>
    public static void NotNullOrEmpty(string? value, string name)
    {
        if (value is null)
            throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
        if (value.Length == 0)
            throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
    }
}

