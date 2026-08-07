using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _04_Data_Mapping_ETL;

namespace HealthData.Interop.Tests.ModelTests;

/// <summary>
/// [EN] Unit tests for LegacyPatientRecord data model.
/// [CN] LegacyPatientRecord 数据模型的单元测试。
/// Verifies record structural equality, property initialization, and copy-with semantics.
/// 验证记录的结构性相等、属性初始化和copy-with语义。
/// </summary>
[TestClass]
public sealed class LegacyPatientRecordTests
{
    /// <summary>
    /// [EN] Verify record can be constructed with all required properties.
    /// [CN] 验证记录可以使用所有必需属性构造。
    /// </summary>
    [TestMethod]
    public void Constructor_WithAllRequiredProperties_ShouldSucceed()
    {
        // Arrange & Act
        var record = new LegacyPatientRecord
        {
            Id = "P001",
            FirstName = "John",
            LastName = "Doe",
            Gender = "Male",
            BirthDate = "1990-01-15"
        };

        // Assert
        record.Id.Should().Be("P001");
        record.FirstName.Should().Be("John");
        record.LastName.Should().Be("Doe");
        record.Gender.Should().Be("Male");
        record.BirthDate.Should().Be("1990-01-15");
    }

    /// <summary>
    /// [EN] Verify optional Phone property defaults to null.
    /// [CN] 验证可选的Phone属性默认为null。
    /// </summary>
    [TestMethod]
    public void Constructor_WithoutPhone_ShouldDefaultToNull()
    {
        // Arrange & Act
        var record = new LegacyPatientRecord
        {
            Id = "P002",
            FirstName = "Jane",
            LastName = "Smith",
            Gender = "Female",
            BirthDate = "1985-06-20"
        };

        // Assert
        record.Phone.Should().BeNull("Phone is an optional property and should default to null");
    }

    /// <summary>
    /// [EN] Verify optional Phone property can be set explicitly.
    /// [CN] 验证可选的Phone属性可以显式设置。
    /// </summary>
    [TestMethod]
    public void Constructor_WithPhone_ShouldAcceptValue()
    {
        // Arrange & Act
        var record = new LegacyPatientRecord
        {
            Id = "P003",
            FirstName = "Bob",
            LastName = "Wilson",
            Gender = "Male",
            BirthDate = "1975-12-01",
            Phone = "+1-555-0123"
        };

        // Assert
        record.Phone.Should().Be("+1-555-0123");
    }

    /// <summary>
    /// [EN] Verify record structural equality (two instances with same values are equal).
    /// [CN] 验证记录的结构性相等（两个值相同的实例应视为相等）。
    /// </summary>
    [TestMethod]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var record1 = new LegacyPatientRecord
        {
            Id = "P004",
            FirstName = "Alice",
            LastName = "Brown",
            Gender = "Female",
            BirthDate = "2000-03-10",
            Phone = "+1-555-9999"
        };

        var record2 = new LegacyPatientRecord
        {
            Id = "P004",
            FirstName = "Alice",
            LastName = "Brown",
            Gender = "Female",
            BirthDate = "2000-03-10",
            Phone = "+1-555-9999"
        };

        // Assert
        record1.Should().Be(record2, "Records with identical values should be structurally equal");
    }

    /// <summary>
    /// [EN] Verify record structural inequality (different values produce unequal instances).
    /// [CN] 验证记录的结构性不等（不同值产生不等的实例）。
    /// </summary>
    [TestMethod]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var record1 = new LegacyPatientRecord
        {
            Id = "P005",
            FirstName = "Alice",
            LastName = "Brown",
            Gender = "Female",
            BirthDate = "2000-03-10"
        };

        var record2 = new LegacyPatientRecord
        {
            Id = "P006",
            FirstName = "Charlie",
            LastName = "Davis",
            Gender = "Male",
            BirthDate = "1980-07-25"
        };

        // Assert
        record1.Should().NotBe(record2, "Records with different values should not be equal");
    }

    /// <summary>
    /// [EN] Verify record copy-with syntax (immutability pattern).
    /// [CN] 验证记录的copy-with语法（不可变性模式）。
    /// </summary>
    [TestMethod]
    public void CopyWith_ModifySingleProperty_ShouldPreserveOthers()
    {
        // Arrange
        var original = new LegacyPatientRecord
        {
            Id = "P007",
            FirstName = "Eve",
            LastName = "Franklin",
            Gender = "Female",
            BirthDate = "1995-11-30"
        };

        // Act
        var updated = original with { Phone = "+44-20-7946-0000" };

        // Assert - Original unchanged
        original.Phone.Should().BeNull("Original record should remain unchanged");

        // Assert - Updated has new value
        updated.Phone.Should().Be("+44-20-7946-0000");
        updated.Id.Should().Be(original.Id);
        updated.FirstName.Should().Be(original.FirstName);
        updated.LastName.Should().Be(original.LastName);
    }

    /// <summary>
    /// [EN] Verify records with same data but different Phone produce inequality.
    /// [CN] 验证相同数据但Phone不同的记录应视为不相等。
    /// </summary>
    [TestMethod]
    public void Equality_DifferentPhone_ShouldNotBeEqual()
    {
        // Arrange
        var record1 = new LegacyPatientRecord
        {
            Id = "P008",
            FirstName = "Same",
            LastName = "Data",
            Gender = "Male",
            BirthDate = "2000-01-01",
            Phone = "+1-555-1111"
        };

        var record2 = new LegacyPatientRecord
        {
            Id = "P008",
            FirstName = "Same",
            LastName = "Data",
            Gender = "Male",
            BirthDate = "2000-01-01",
            Phone = "+1-555-2222"
        };

        // Assert
        record1.Should().NotBe(record2, "Records with different phone numbers should not be equal");
    }

    /// <summary>
    /// [EN] Verify GetHashCode consistency for structurally equal records.
    /// [CN] 验证结构性相等记录的GetHashCode一致性。
    /// </summary>
    [TestMethod]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        var record1 = new LegacyPatientRecord
        {
            Id = "P009",
            FirstName = "Hash",
            LastName = "Test",
            Gender = "Other",
            BirthDate = "1988-08-08"
        };

        var record2 = new LegacyPatientRecord
        {
            Id = "P009",
            FirstName = "Hash",
            LastName = "Test",
            Gender = "Other",
            BirthDate = "1988-08-08"
        };

        // Assert
        record1.GetHashCode().Should().Be(record2.GetHashCode(), "Equal records should have equal hash codes");
    }

    /// <summary>
    /// [EN] Verify ToString produces meaningful output.
    /// [CN] 验证ToString产生有意义的输出。
    /// </summary>
    [TestMethod]
    public void ToString_ShouldProduceMeaningfulOutput()
    {
        // Arrange
        var record = new LegacyPatientRecord
        {
            Id = "P010",
            FirstName = "ToString",
            LastName = "Test",
            Gender = "Male",
            BirthDate = "2000-01-01"
        };

        // Act
        string result = record.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty("ToString should not return null or empty");
        result.Should().Contain("LegacyPatientRecord", "Output should contain the type name");
    }

    /// <summary>
    /// [EN] Verify record supports various gender values.
    /// [CN] 验证记录支持各种性别值。
    /// </summary>
    [TestMethod, DataRow("Male")]
    [DataRow("Female")]
    [DataRow("Other")]
    [DataRow("Unknown")]
    [DataRow("Non-binary")]
    public void Gender_ShouldAcceptVariousValues(string genderValue)
    {
        // Arrange & Act
        var record = new LegacyPatientRecord
        {
            Id = "P011",
            FirstName = "Gender",
            LastName = "Test",
            Gender = genderValue,
            BirthDate = "2000-01-01"
        };

        // Assert
        record.Gender.Should().Be(genderValue);
    }

    /// <summary>
    /// [EN] Verify record supports various date formats for BirthDate.
    /// [CN] 验证记录支持多种日期格式用于BirthDate。
    /// </summary>
    [TestMethod, DataRow("2000-01-01")]
    [DataRow("1950-12-31")]
    [DataRow("2024-06-15")]
    public void BirthDate_ShouldAcceptVariousFormats(string dateValue)
    {
        // Arrange & Act
        var record = new LegacyPatientRecord
        {
            Id = "P012",
            FirstName = "Date",
            LastName = "Test",
            Gender = "Male",
            BirthDate = dateValue
        };

        // Assert
        record.BirthDate.Should().Be(dateValue);
    }
}
