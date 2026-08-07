using System.Text.Json;

namespace Shared_Library
{
    /// <summary>
    /// [EN] HIPAA-compliant audit logging utility for recording Protected Health Information (PHI) access.
    /// Records who, when, what action, what resource, and IP address.
    /// [CN] 符合HIPAA标准的审计日志工具，用于记录受保护健康信息(PHI)的访问。
    /// 记录谁、何时、什么操作、什么资源以及IP地址。
    /// </summary>
    public static class AuditLog
    {
        /// <summary>
        /// [EN] Records a HIPAA-compliant audit log entry for PHI access.
        /// Parameters: userId (the user accessing data), role (user's role), ipAddress (source IP),
        /// resourceType (FHIR resource type), resourceId (resource ID), action (READ/WRITE/DELETE/UPDATE).
        /// [CN] 记录一条符合HIPAA标准的审计日志条目，用于PHI访问。
        /// 参数：userId(访问数据的用户)、role(用户角色)、ipAddress(来源IP)、
        /// resourceType(FHIR资源类型)、resourceId(资源ID)、action(操作类型)。
        /// </summary>
        /// <param name="userId">[EN] User identifier / [CN] 用户标识符</param>
        /// <param name="role">[EN] User role / [CN] 用户角色</param>
        /// <param name="ipAddress">[EN] Source IP address / [CN] 来源IP地址</param>
        /// <param name="resourceType">[EN] FHIR resource type / [CN] FHIR资源类型</param>
        /// <param name="resourceId">[EN] Resource identifier / [CN] 资源标识符</param>
        /// <param name="action">[EN] Action performed (READ/WRITE/DELETE) / [CN] 执行的操作类型</param>
        public static void Record(string userId, string role, string ipAddress,
            string resourceType, string resourceId, string action)
        {
            GuardAgainstNullOrEmpty(userId, nameof(userId));
            GuardAgainstNullOrEmpty(role, nameof(role));
            GuardAgainstNullOrEmpty(ipAddress, nameof(ipAddress));
            GuardAgainstNullOrEmpty(resourceType, nameof(resourceType));
            GuardAgainstNullOrEmpty(resourceId, nameof(resourceId));
            GuardAgainstNullOrEmpty(action, nameof(action));

            var record = new
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                UserId = userId,
                Role = role,
                IpAddress = ipAddress,
                Action = action,
                Resource = $"{resourceType}/{resourceId}",
                AuditMessage = "PHI access recorded for HIPAA compliance"
            };

            Console.WriteLine("\n========== HIPAA AUDIT LOG ==========");
            Console.WriteLine(JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("=====================================\n");
        }

        /// <summary>
        /// [EN] Validates that a string argument is neither null nor empty.
        /// Throws ArgumentNullException for null and ArgumentException for empty, matching BCL conventions.
        /// [CN] 验证字符串参数既不为null也不为空。
        /// null抛出ArgumentNullException，空字符串抛出ArgumentException，符合BCL惯例。
        /// </summary>
        private static void GuardAgainstNullOrEmpty(string? value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name, $"Parameter '{name}' must not be null.");
            if (value.Length == 0)
                throw new ArgumentException($"Parameter '{name}' must not be empty.", name);
        }
    }
}
