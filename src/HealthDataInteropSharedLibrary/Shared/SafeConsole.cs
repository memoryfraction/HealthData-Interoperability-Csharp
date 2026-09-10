using System.Text.RegularExpressions;

namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Console wrapper that automatically masks PHI data before output.
/// Use this INSTEAD of Console.WriteLine to keep PHI masked in console output during demonstrations/testing (HIPAA Security Rule-oriented practice).
/// 
/// [CN] 自动在输出前脱敏PHI数据的控制台包装器。使用这个代替Console.WriteLine，在执行演示/测试时保持控制台输出脱敏（HIPAA安全规则取向的实践）。
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
