namespace HealthDataInteropSharedLibrary.Shared;

/// <summary>
/// [EN] Source DTO matching the legacy CSV structure (module 04).
/// Represents raw patient data before FHIR transformation.
/// [CN] 匹配遗留CSV结构的源DTO（模块04）。表示FHIR转换前的原始患者数据。
/// </summary>
public sealed record LegacyPatientRecord
{
    /// <summary>[EN] Unique identifier from the source system. / [CN] 源系统唯一标识符</summary>
    public required string Id { get; init; }

    /// <summary>[EN] Patient first name. / [CN] 患者名字</summary>
    public required string FirstName { get; init; }

    /// <summary>[EN] Patient last name. / [CN] 患者姓氏</summary>
    public required string LastName { get; init; }

    /// <summary>[EN] Administrative gender string. / [CN] 行政性别字符串</summary>
    public required string Gender { get; init; }

    /// <summary>[EN] Birth date in yyyy-MM-dd format. / [CN] 出生日期（yyyy-MM-dd格式）</summary>
    public required string BirthDate { get; init; }

    /// <summary>[EN] Optional phone number. / [CN] 可选电话号码</summary>
    public string? Phone { get; init; }
}

