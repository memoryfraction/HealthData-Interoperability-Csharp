namespace _04_Data_Mapping_ETL;

/// <summary>
/// Represents the raw data model matching the legacy CSV structure.
/// 匹配旧系统 CSV 结构的原始数据模型。
/// </summary>
public record LegacyPatientRecord
{
	/// <summary>
	/// Unique identifier from the source system.
	/// 来自源系统的唯一标识符。
	/// </summary>
	public required string Id { get; init; }

	/// <summary>
	/// Patient's first name / given name.
	/// 患者的名字。
	/// </summary>
	public required string FirstName { get; init; }

	/// <summary>
	/// Patient's last name / family name.
	/// 患者的姓氏。
	/// </summary>
	public required string LastName { get; init; }

	/// <summary>
	/// Administrative gender (e.g., Male, Female, Other).
	/// 行政性别（如：男、女、其他）。
	/// </summary>
	public required string Gender { get; init; }

	/// <summary>
	/// Birth date in "yyyy-MM-dd" format.
	/// 出生日期，格式为 "yyyy-MM-dd"。
	/// </summary>
	public required string BirthDate { get; init; }

	/// <summary>
	/// Optional contact phone number.
	/// 可选的联系电话。
	/// </summary>
	public string? Phone { get; init; }
}