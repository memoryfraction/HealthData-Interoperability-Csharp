using Shared_Library.Shared;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared_Library;

namespace HealthData.Interop.Tests.GenderNormalizerTests;

/// <summary>
/// [EN] GenderNormalizer normalization and display string tests.
/// Covers: normal gender values, case insensitivity, single-letter variants,
/// unrecognized/null/whitespace inputs, and reverse mapping.
/// [CN] GenderNormalizer 标准化和显示字符串测试。
/// 覆盖：正常性别值、大小写不敏感、单字母变体、无法识别/null/空白输入和反向映射。
/// </summary>
[TestClass]
public sealed class GenderNormalizerTests
{
    // ===== Normal Scenarios / 正常场景 =====

    /// <summary>
    /// Normalize: exact lowercase matches (male, female).
    /// </summary>
    [TestMethod]
    public void Normalize_LowercaseExact_ReturnsCorrectGender()
    {
        GenderNormalizer.Normalize("male").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("female").Should().Be(AdministrativeGender.Female);
    }

    /// <summary>
    /// Normalize: case-insensitive matching (MALE, Male, mAlE).
    /// </summary>
    [TestMethod]
    public void Normalize_CaseInsensitive_MatchesCorrectly()
    {
        GenderNormalizer.Normalize("MALE").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("MaLe").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("FEMALE").Should().Be(AdministrativeGender.Female);
        GenderNormalizer.Normalize("FeMalE").Should().Be(AdministrativeGender.Female);
    }

    /// <summary>
    /// Normalize: single-letter variants (M, F) and word variants (man, woman).
    /// </summary>
    [TestMethod]
    public void Normalize_SingleLetterAndWordVariants_MapsCorrectly()
    {
        GenderNormalizer.Normalize("M").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("m").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("man").Should().Be(AdministrativeGender.Male);
        GenderNormalizer.Normalize("F").Should().Be(AdministrativeGender.Female);
        GenderNormalizer.Normalize("f").Should().Be(AdministrativeGender.Female);
        GenderNormalizer.Normalize("woman").Should().Be(AdministrativeGender.Female);
    }

    // ===== Expected Exception Scenarios / 期待异常场景 =====

    /// <summary>
    /// Normalize: null input returns Unknown (no exception — graceful handling).
    /// </summary>
    [TestMethod]
    public void Normalize_NullInput_ReturnsUnknown()
    {
        GenderNormalizer.Normalize(null!).Should().Be(AdministrativeGender.Unknown);
    }

    /// <summary>
    /// Normalize: empty/whitespace input returns Unknown (no exception).
    /// </summary>
    [TestMethod]
    public void Normalize_EmptyOrWhitespace_ReturnsUnknown()
    {
        GenderNormalizer.Normalize("").Should().Be(AdministrativeGender.Unknown);
        GenderNormalizer.Normalize("   ").Should().Be(AdministrativeGender.Unknown);
        GenderNormalizer.Normalize("\t").Should().Be(AdministrativeGender.Unknown);
    }

    // ===== Error Scenarios / 错误场景 =====

    /// <summary>
    /// Normalize: unrecognized values (including partial matches like "mal" or "fem") return Unknown.
    /// </summary>
    [TestMethod]
    public void Normalize_UnrecognizedValues_ReturnsUnknown()
    {
        var unrecognized = new[] { "mal", "fem", "other", "non-binary" };
        foreach (var input in unrecognized)
            GenderNormalizer.Normalize(input).Should().Be(AdministrativeGender.Unknown, $"'{input}' should map to Unknown");
    }

    // ===== Display String Tests / 显示字符串测试 =====

    /// <summary>
    /// ToDisplayString: maps enum back to human-readable string.
    /// </summary>
    [TestMethod]
    public void ToDisplayString_AllEnums_ReturnCorrectString()
    {
        GenderNormalizer.ToDisplayString(AdministrativeGender.Male).Should().Be("Male");
        GenderNormalizer.ToDisplayString(AdministrativeGender.Female).Should().Be("Female");
        GenderNormalizer.ToDisplayString(AdministrativeGender.Other).Should().Be("Other");
        GenderNormalizer.ToDisplayString(AdministrativeGender.Unknown).Should().Be("Unknown");
    }

    /// <summary>
    /// ToDisplayString: default enum value (0 = Male) maps to "Male".
    /// </summary>
    [TestMethod]
    public void ToDisplayString_DefaultEnumValue_ReturnsMale()
    {
        var defaultValue = default(AdministrativeGender); // 0 = Male
        GenderNormalizer.ToDisplayString(defaultValue).Should().Be("Male");
    }
}
