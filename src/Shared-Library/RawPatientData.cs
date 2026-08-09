namespace Shared_Library;

/// <summary>
/// [EN] Source DTO for SMART-on-FHIR CSV data (module 05).
/// Simpler schema than LegacyPatientRecord — no explicit ID column.
/// [CN] SMART-on-FHIR CSV数据的源DTO（模块05）。比LegacyPatientRecord更简单的架构——无显式ID列。
/// </summary>
public sealed record RawPatientData
{
    /// <summary>[EN] Patient first name. / [CN] 患者名字</summary>
    public required string FirstName { get; init; }

    /// <summary>[EN] Patient last name. / [CN] 患者姓氏</summary>
    public required string LastName { get; init; }

    /// <summary>[EN] Administrative gender string. / [CN] 行政性别字符串</summary>
    public required string Gender { get; init; }

    /// <summary>[EN] Birth date in ISO 8601 format. / [CN] 出生日期（ISO 8601格式）</summary>
    public required string BirthDate { get; init; }
}
