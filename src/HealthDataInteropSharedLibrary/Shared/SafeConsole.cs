using System.Text.RegularExpressions;

namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Console wrapper that automatically masks PHI data before output.
/// Use this INSTEAD of Console.WriteLine to enforce HIPAA compliance during demonstrations/testing.
/// 
/// [CN] 自动在输出前脱敏PHI数据的控制台包装器。使用这个代替Console.WriteLine以在执行演示/测试时执行HIPAA合规性。
/// </summary>
public static class SafeConsole
{
    /// <summary>
    /// [EN] Write a line of text with all PHI patterns automatically masked.
    /// 
    /// [CN] 用所有自动脱敏的PHI模式写入一行文本。
    /// </summary>
    public static void WriteLine(string? message) =>
        System.Console.WriteLine(PhiMasker.Mask(message));

    /// <summary>
    /// [EN] Write an empty line.
    /// </summary>
    public static void WriteLine() =>
        System.Console.WriteLine();

    public static void Write(string? message) =>
        System.Console.Write(PhiMasker.Mask(message));
}
