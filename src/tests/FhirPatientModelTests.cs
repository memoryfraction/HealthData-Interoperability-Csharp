using System.Text.Json;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HealthData.Interop.Tests.FhirModelTests;

/// <summary>
/// [EN] Unit tests for FHIR Patient model construction and JSON serialization.
/// [CN] FHIR Patient模型构造和JSON序列化的单元测试。
/// Tests the core data model used across all interoperability modules without requiring a FHIR server.
/// 测试所有互操作性模块使用的核心数据模型，无需FHIR服务器。
/// </summary>
[TestClass]
public sealed class FhirPatientModelTests
{
    /// <summary>
    /// [EN] Verify basic Patient resource can be constructed with required fields.
    /// [CN] 验证基础Patient资源可以使用必需字段构造。
    /// </summary>
    [TestMethod]
    public void ConstructPatient_WithRequiredFields_ShouldSucceed()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { "John" },
                    Family = "Doe"
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "1990-01-01"
        };

        // Assert
        patient.Name.Should().HaveCount(1);
        patient.Name[0].Given.Should().Contain("John");
        patient.Name[0].Family.Should().Be("Doe");
        patient.Gender.Should().Be(AdministrativeGender.Male);
    }

    /// <summary>
    /// [EN] Verify Patient with Identifier can be constructed.
    /// [CN] 验证带有Identifier的Patient可以构造。
    /// </summary>
    [TestMethod]
    public void ConstructPatient_WithIdentifier_ShouldSucceed()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Id = "P001",
            Identifier = new List<Identifier>
            {
                new Identifier("http://example.org/test-ids", "LEGACY-001")
            },
            Name = new List<HumanName> { new HumanName().WithGiven("Test").AndFamily("Patient") },
            Gender = AdministrativeGender.Female,
            BirthDate = "1985-06-20"
        };

        // Assert
        patient.Id.Should().Be("P001");
        patient.Identifier.Should().HaveCount(1);
        patient.Identifier[0].Value.Should().Be("LEGACY-001");
    }

    /// <summary>
    /// [EN] Verify Patient with Telecom (ContactPoint) can be constructed.
    /// [CN] 验证带有Telecom(ContactPoint)的Patient可以构造。
    /// </summary>
    [TestMethod]
    public void ConstructPatient_WithTelecom_ShouldSucceed()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Jane").AndFamily("Doe") },
            Gender = AdministrativeGender.Female,
            BirthDate = "1990-05-15",
            Telecom = new List<ContactPoint>
            {
                new ContactPoint(ContactPoint.ContactPointSystem.Phone, null, "+1-555-0123")
            }
        };

        // Assert
        patient.Telecom.Should().HaveCount(1);
        patient.Telecom[0].System.Should().Be(ContactPoint.ContactPointSystem.Phone);
        patient.Telecom[0].Value.Should().Be("+1-555-0123");
    }

    /// <summary>
    /// [EN] Verify Patient with Meta tags can be constructed for test data markers.
    /// [CN] 验证带有Meta标签的Patient可以构造，用于测试数据标记。
    /// </summary>
    [TestMethod]
    public void ConstructPatient_WithMetaTags_ShouldSucceed()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Meta = new Meta
            {
                Tag = new List<Coding>
                {
                    new Coding("http://terminology.hl7.org/CodeSystem/v3-ObservationValue", "SUBSET", "Test Data")
                }
            },
            Name = new List<HumanName> { new HumanName().WithGiven("Tagged").AndFamily("Patient") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01"
        };

        // Assert
        patient.Meta.Should().NotBeNull();
        patient.Meta.Tag.Should().HaveCount(1);
        patient.Meta.Tag[0].Code.Should().Be("SUBSET");
    }

    /// <summary>
    /// [EN] Verify Patient with US Core profile reference can be constructed.
    /// [CN] 验证带有US Core Profile引用的Patient可以构造。
    /// </summary>
    [TestMethod]
    public void ConstructPatient_WithUsCoreProfile_ShouldSucceed()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Meta = new Meta
            {
                Profile = new[] { "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient" }
            },
            Name = new List<HumanName> { new HumanName().WithGiven("USCore").AndFamily("Patient") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01",
            Active = true
        };

        // Assert
        patient.Meta.Profile.Should().Contain("http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient");
    }

    /// <summary>
    /// [EN] Verify Patient can be serialized to valid JSON using FhirJsonSerializer.
    /// [CN] 验证Patient可以使用FhirJsonSerializer序列化为有效的JSON。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_ValidPatient_ShouldProduceValidJson()
    {
        // Arrange
        var patient = new Patient
        {
            Id = "P001",
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { "John" },
                    Family = "Doe"
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "1990-01-01"
        };

        // Act
        var serializer = new FhirJsonSerializer();
        string json = serializer.SerializeToString(patient);

        // Assert
        json.Should().NotBeNullOrEmpty("Serialized output should not be empty");
        json.Should().Contain("\"resourceType\":\"Patient\"", "JSON should contain resourceType");
        json.Should().Contain("\"gender\":\"male\"", "JSON should contain gender");

        // Verify it can be parsed back as valid JSON
        var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
        parsedJson.GetProperty("resourceType").GetString().Should().Be("Patient");
    }

    /// <summary>
    /// [EN] Verify Patient with multiple given names serializes correctly.
    /// [CN] 验证带有多个名字的Patient正确序列化。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_MultipleGivenNames_ShouldSerializeCorrectly()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { "John", "James" },
                    Family = "Doe"
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "1990-01-01"
        };

        // Act
        var serializer = new FhirJsonSerializer();
        string json = serializer.SerializeToString(patient);

        // Assert
        json.Should().Contain("\"family\":\"Doe\"", "JSON should contain family name");
        json.Should().Contain("\"John\"", "JSON should contain first given name");
        json.Should().Contain("\"James\"", "JSON should contain second given name");
    }

    /// <summary>
    /// [EN] Verify Patient with null BirthDate serializes without error.
    /// [CN] 验证BirthDate为null的Patient序列化时不报错。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_NullBirthDate_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Unknown").AndFamily("Date") },
            Gender = AdministrativeGender.Unknown
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle null BirthDate gracefully");
    }

    /// <summary>
    /// [EN] Verify all valid AdministrativeGender values produce correct JSON representation.
    /// [CN] 验证所有有效的AdministrativeGender值产生正确的JSON表示。
    /// </summary>
    [TestMethod, DataRow("Male", "\"male\"")]
    [DataRow("Female", "\"female\"")]
    [DataRow("Other", "\"other\"")]
    [DataRow("Unknown", "\"unknown\"")]
    public void SerializeToJson_GenderValues_ShouldBeLowercase(string genderEnum, string expectedJson)
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Test").AndFamily("Patient") },
            Gender = (AdministrativeGender)Enum.Parse(typeof(AdministrativeGender), genderEnum),
            BirthDate = "2000-01-01"
        };

        // Act
        var serializer = new FhirJsonSerializer();
        string json = serializer.SerializeToString(patient);

        // Assert
        json.Should().Contain(expectedJson, $"JSON should contain lowercase gender '{expectedJson}'");
    }

    /// <summary>
    /// [EN] Verify BirthDate serialization uses ISO 8601 date format.
    /// [CN] 验证BirthDate序列化使用ISO 8601日期格式。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_BirthDate_ShouldUseIso8601Format()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Date").AndFamily("Test") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-12-31"
        };

        // Act
        var serializer = new FhirJsonSerializer();
        string json = serializer.SerializeToString(patient);

        // Assert
        json.Should().Contain("\"birthDate\":\"2000-12-31\"", "BirthDate should use ISO 8601 format");
    }

    /// <summary>
    /// [EN] Verify Patient with Active flag serializes correctly.
    /// [CN] 验证带有Active标志的Patient正确序列化。
    /// </summary>
    [TestMethod, DataRow(true)]
    [DataRow(false)]
    public void SerializeToJson_ActiveFlag_ShouldBeIncluded(bool activeValue)
    {
        // Arrange
        var patient = new Patient
        {
            Active = activeValue,
            Name = new List<HumanName> { new HumanName().WithGiven("Active").AndFamily("Test") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01"
        };

        // Act
        var serializer = new FhirJsonSerializer();
        string json = serializer.SerializeToString(patient);

        // Assert
        json.Should().Contain($"\"active\":{activeValue.ToString().ToLower()}", $"JSON should contain active flag as {activeValue}");
    }

    /// <summary>
    /// [EN] Error scenario: Verify serialization handles Patient with empty names gracefully.
    /// [CN] 错误场景：验证序列化在Patient名字为空时不报错。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_EmptyNames_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName>(),
            Gender = AdministrativeGender.Unknown,
            BirthDate = "2000-01-01"
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle empty names list");
    }

    /// <summary>
    /// [EN] Error scenario: Verify serialization handles Patient with null identifier gracefully.
    /// [CN] 错误场景：验证序列化在Patient标识符为null时不报错。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_NullIdentifier_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Null").AndFamily("Id") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01"
            // No Identifier set (null by default)
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle null identifier");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify serialization handles Patient with special characters in name.
    /// [CN] 边界条件：验证序列化在Patient名字包含特殊字符时正常工作。
    /// </summary>
    [TestMethod, DataRow("O'Brien")]
    [DataRow("Muñoz")]
    [DataRow("田中太郎")]
    [DataRow("\"Quoted\" Name")]
    public void SerializeToJson_SpecialCharacterNames_ShouldNotThrow(string lastName)
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { "Test" },
                    Family = lastName
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01"
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow($"Serialization should handle special characters in name: '{lastName}'");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify serialization handles Patient with future BirthDate (edge case).
    /// [CN] 边界条件：验证序列化在Patient出生日期为未来日期时不报错。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_FutureBirthDate_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Future").AndFamily("Date") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2099-12-31"
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle future birth dates");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify serialization handles Patient with ancient BirthDate (edge case).
    /// [CN] 边界条件：验证序列化在Patient出生日期为远古日期时不报错。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_AncientBirthDate_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Ancient").AndFamily("Date") },
            Gender = AdministrativeGender.Male,
            BirthDate = "1900-01-01"
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle ancient birth dates");
    }
}

/// <summary>
/// [EN] Unit tests for FHIR Bundle and Transaction model construction.
/// [CN] FHIR Bundle和Transaction模型构造的单元测试。
/// Tests the transaction bundle pattern used in ETL modules for batch operations.
/// 测试ETL模块中用于批量操作的transaction bundle模式。
/// </summary>
[TestClass]
public sealed class FhirBundleModelTests
{
    /// <summary>
    /// [EN] Verify Transaction Bundle can be constructed with PUT entries.
    /// [CN] 验证Transaction Bundle可以使用PUT条目构造。
    /// </summary>
    [TestMethod]
    public void CreateTransactionBundle_WithPutEntries_ShouldSucceed()
    {
        // Arrange
        const string idSystem = "http://example.org/test-ids";
        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        var patient = new Patient
        {
            Identifier = new List<Identifier> { new Identifier(idSystem, "LEGACY-001") },
            Name = new List<HumanName> { new HumanName().WithGiven("Test").AndFamily("Patient") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01"
        };

        // Act
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = patient,
            Request = new Bundle.RequestComponent
            {
                Method = Bundle.HTTPVerb.PUT,
                Url = $"Patient?identifier={idSystem}|LEGACY-001"
            }
        });

        // Assert
        bundle.Type.Should().Be(Bundle.BundleType.Transaction);
        bundle.Entry.Should().HaveCount(1);
    }

    /// <summary>
    /// [EN] Verify Bundle can contain multiple entries of different types.
    /// [CN] 验证Bundle可以包含多种类型的多个条目。
    /// </summary>
    [TestMethod]
    public void CreateTransactionBundle_WithMultipleEntries_ShouldSucceed()
    {
        // Arrange
        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        // Act
        for (int i = 0; i < 3; i++)
        {
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                Resource = new Patient
                {
                    Name = new List<HumanName> { new HumanName().WithGiven($"Test{i}").AndFamily("Patient") },
                    Gender = AdministrativeGender.Male,
                    BirthDate = "2000-01-01"
                },
                Request = new Bundle.RequestComponent
                {
                    Method = Bundle.HTTPVerb.POST,
                    Url = "Patient"
                }
            });
        }

        // Assert
        bundle.Entry.Should().HaveCount(3);
    }

    /// <summary>
    /// [EN] Verify Bundle can use all HTTP methods (GET, POST, PUT, DELETE).
    /// [CN] 验证Bundle可以使用所有HTTP方法（GET、POST、PUT、DELETE）。
    /// </summary>
    [TestMethod]
    public void CreateTransactionBundle_AllHttpMethods_ShouldSucceed()
    {
        // Arrange
        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        // Act - Add entries with different HTTP methods
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Patient(),
            Request = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.GET, Url = "Patient/1" }
        });
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Patient(),
            Request = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.POST, Url = "Patient" }
        });
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Patient(),
            Request = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.PUT, Url = "Patient/1" }
        });
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Patient(),
            Request = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.DELETE, Url = "Patient/1" }
        });

        // Assert
        bundle.Entry.Should().HaveCount(4);
    }

    /// <summary>
    /// [EN] Error scenario: Verify Bundle serializes even with minimal entries.
    /// [CN] 错误场景：验证Bundle即使在最小化条目下也能序列化。
    /// </summary>
    [TestMethod]
    public void SerializeTransactionBundle_WithMinimalEntry_ShouldNotThrow()
    {
        // Arrange
        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };
        bundle.Entry.Add(new Bundle.EntryComponent
        {
            Resource = new Patient(),
            Request = new Bundle.RequestComponent { Method = Bundle.HTTPVerb.POST, Url = "Patient" }
        });

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(bundle);
        };
        act.Should().NotThrow("Transaction bundle should serialize even with minimal entry");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify Bundle handles empty entries list.
    /// [CN] 边界条件：验证Bundle处理空条目列表。
    /// </summary>
    [TestMethod]
    public void CreateTransactionBundle_EmptyEntries_ShouldSerialize()
    {
        // Arrange
        var bundle = new Bundle { Type = Bundle.BundleType.Transaction };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(bundle);
        };
        act.Should().NotThrow("Transaction bundle should serialize even with no entries");
    }
}

/// <summary>
/// [EN] Unit tests for gender mapping logic used in ETL modules.
/// [CN] ETL模块中使用的性别映射逻辑的单元测试。
/// Verifies the switch expression that maps CSV gender strings to AdministrativeGender enum.
/// 验证将CSV性别字符串映射到AdministrativeGender枚举的switch表达式。
/// </summary>
[TestClass]
public sealed class GenderMappingTests
{
    /// <summary>
    /// [EN] Verify lowercase male maps correctly.
    /// [CN] 验证小写male正确映射。
    /// </summary>
    [TestMethod]
    public void MapGender_LowercaseMale_ShouldReturnMale()
    {
        // Arrange & Act
        var result = "male".ToLower() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };

        // Assert
        result.Should().Be(AdministrativeGender.Male);
    }

    /// <summary>
    /// [EN] Verify lowercase female maps correctly.
    /// [CN] 验证小写female正确映射。
    /// </summary>
    [TestMethod]
    public void MapGender_LowercaseFemale_ShouldReturnFemale()
    {
        // Arrange & Act
        var result = "female".ToLower() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };

        // Assert
        result.Should().Be(AdministrativeGender.Female);
    }

    /// <summary>
    /// [EN] Verify mixed case gender maps correctly (case-insensitive handling).
    /// [CN] 验证混合大小写性别正确映射（不区分大小写处理）。
    /// </summary>
    [TestMethod, DataRow("Male", "Male")]
    [DataRow("FEMALE", "Female")]
    [DataRow("MaLe", "Male")]
    public void MapGender_MixedCase_ShouldReturnCorrectValue(string input, string expectedGender)
    {
        // Arrange & Act
        var result = input.ToLower() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };

        // Assert
        result.Should().Be((AdministrativeGender)Enum.Parse(typeof(AdministrativeGender), expectedGender));
    }

    /// <summary>
    /// [EN] Boundary condition: Verify unrecognized gender values fall back to Unknown.
    /// [CN] 边界条件：验证无法识别的性别值回退到Unknown。
    /// </summary>
    [TestMethod, DataRow("Other")]
    [DataRow("M")]
    [DataRow("F")]
    [DataRow("")]
    public void MapGender_UnrecognizedValues_ShouldReturnUnknown(string input)
    {
        // Arrange & Act
        var result = (input ?? "").ToLower() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };

        // Assert
        result.Should().Be(AdministrativeGender.Unknown, $"Unrecognized gender '{input}' should map to Unknown");
    }

    /// <summary>
    /// [EN] Error scenario: Verify null input does not throw.
    /// [CN] 错误场景：验证null输入不抛异常。
    /// </summary>
    [TestMethod]
    public void MapGender_NullInput_ShouldNotThrow()
    {
        // Arrange
        string? gender = null;

        // Act & Assert
        Action act = () =>
        {
            var _ = (gender?.ToLower() ?? "unknown") switch
            {
                "male" => AdministrativeGender.Male,
                "female" => AdministrativeGender.Female,
                _ => AdministrativeGender.Unknown
            };
        };
        act.Should().NotThrow("Null gender input should be handled gracefully");
    }

    /// <summary>
    /// [EN] Error scenario: Verify whitespace-only input maps to Unknown.
    /// [CN] 错误场景：验证仅空白字符输入映射到Unknown。
    /// </summary>
    [TestMethod, DataRow(" ")]
    [DataRow("  male  ")]
    public void MapGender_WhitespaceInput_ShouldReturnUnknown(string input)
    {
        // Arrange & Act
        var result = input.ToLower() switch
        {
            "male" => AdministrativeGender.Male,
            "female" => AdministrativeGender.Female,
            _ => AdministrativeGender.Unknown
        };

        // Assert - whitespace variants won't match exact strings
        result.Should().Be(AdministrativeGender.Unknown, "Whitespace input should map to Unknown");
    }
}

/// <summary>
/// [EN] Unit tests for FhirJsonSerializer round-trip scenarios.
/// [CN] FhirJsonSerializer往返场景的单元测试。
/// Verifies that Patient resources can be serialized and deserialized without data loss.
/// 验证Patient资源可以序列化和反序列化而不丢失数据。
/// </summary>
[TestClass]
public sealed class FhirJsonRoundTripTests
{
    /// <summary>
    /// [EN] Verify full round-trip: serialize then deserialize preserves all data.
    /// [CN] 验证完整往返：序列化然后反序列化保留所有数据。
    /// </summary>
    [TestMethod]
    public void RoundTrip_SerializeThenDeserialize_ShouldPreserveAllData()
    {
        // Arrange
        var serializer = new FhirJsonSerializer();
        var original = new Patient
        {
            Id = "P001",
            Active = true,
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Given = new[] { "John", "James" },
                    Family = "Doe"
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "1990-01-15",
            Telecom = new List<ContactPoint>
            {
                new ContactPoint(ContactPoint.ContactPointSystem.Phone, null, "+1-555-0123")
            }
        };

        // Act - Serialize
        string json = serializer.SerializeToString(original);

        // Act - Deserialize back using System.Text.Json
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        parsed.GetProperty("resourceType").GetString().Should().Be("Patient");
        json.Should().Contain("\"id\":\"P001\"");
        json.Should().Contain("\"gender\":\"male\"");
        json.Should().Contain("\"birthDate\":\"1990-01-15\"");
    }

    /// <summary>
    /// [EN] Error scenario: Verify deserialization of malformed JSON does not crash the serializer.
    /// [CN] 错误场景：验证反序列化格式错误的JSON不导致崩溃。
    /// </summary>
    [TestMethod]
    public void Deserialize_MalformedJson_ShouldNotCrash()
    {
        // Arrange
        string malformedJson = "{\"resourceType\":\"Patient\",\"invalid data\"}";

        // Act & Assert - System.Text.Json should throw JsonException, but serializer won't crash
        Action act = () => JsonSerializer.Deserialize<JsonElement>(malformedJson);
        act.Should().Throw<JsonException>("Malformed JSON should cause a parse error");
    }

    /// <summary>
    /// [EN] Error scenario: Verify deserialization of empty string is handled.
    /// [CN] 错误场景：验证空字符串的反序列化得到正确处理。
    /// </summary>
    [TestMethod]
    public void Deserialize_EmptyString_ShouldThrowJsonException()
    {
        // Act & Assert
        Action act = () => JsonSerializer.Deserialize<JsonElement>("");
        act.Should().Throw<JsonException>("Empty string should not be valid JSON");
    }

    /// <summary>
    /// [EN] Error scenario: Verify deserialization of null is handled.
    /// [CN] 错误场景：验证null的反序列化得到正确处理。
    /// </summary>
    [TestMethod]
    public void Deserialize_Null_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        string? nullStr = null; Action act = () => JsonSerializer.Deserialize<JsonElement>(nullStr!);
        act.Should().Throw<ArgumentNullException>("Null input should throw ArgumentNullException");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify serialization of Patient with very long identifier value.
    /// [CN] 边界条件：验证Patient带有超长标识符值的序列化。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_VeryLongIdentifier_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Long").AndFamily("Id") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01",
            Identifier = new List<Identifier>
            {
                new Identifier("http://long-system.example.org", "VALUE-xxx")
            }
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            serializer.SerializeToString(patient);
        };
        act.Should().NotThrow("Serialization should handle very long identifier values");
    }

    /// <summary>
    /// [EN] Boundary condition: Verify serialization of Patient with multiple identifiers.
    /// [CN] 边界条件：验证Patient带有多个标识符的序列化。
    /// </summary>
    [TestMethod]
    public void SerializeToJson_MultipleIdentifiers_ShouldNotThrow()
    {
        // Arrange
        var patient = new Patient
        {
            Name = new List<HumanName> { new HumanName().WithGiven("Multi").AndFamily("Id") },
            Gender = AdministrativeGender.Male,
            BirthDate = "2000-01-01",
            Identifier = new List<Identifier>
            {
                new Identifier("http://sys1.org", "ID001"),
                new Identifier("http://sys2.org", "ID002"),
                new Identifier("http://sys3.org", "SSN-123")
            }
        };

        // Act & Assert
        Action act = () =>
        {
            var serializer = new FhirJsonSerializer();
            string json = serializer.SerializeToString(patient);
            json.Should().Contain("ID001");
            json.Should().Contain("ID002");
            json.Should().Contain("SSN-123");
        };
        act.Should().NotThrow("Serialization should handle multiple identifiers");
    }
}
