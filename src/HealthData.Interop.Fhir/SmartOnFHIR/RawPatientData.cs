namespace HealthDataInteropSharedLibrary.SmartOnFHIR
{
    /// <summary>
    /// [EN] Data model for raw patient data mapped from CSV files.
    /// Used in the SMART on FHIR ETL module to represent source data before FHIR transformation.
    /// [CN] 从CSV文件映射的原始患者数据的数据模型。
    /// 在SMART on FHIR ETL模块中用于表示FHIR转换之前的源数据。
    /// </summary>
    public class RawPatientData
    {
        /// <summary>
        /// [EN] Patient's first name / given name. / [CN] 患者的名字。
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// [EN] Patient's last name / family name. / [CN] 患者的姓氏。
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// [EN] Administrative gender (e.g., Male, Female). / [CN] 行政性别（如：男、女）。
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// [EN] Birth date in ISO 8601 format. / [CN] 出生日期，ISO 8601格式。
        /// </summary>
        public string BirthDate { get; set; }
    }
}

