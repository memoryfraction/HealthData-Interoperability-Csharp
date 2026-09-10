using System.Text.Json;

namespace HealthDataInteropSharedLibrary.Shared;
    /// <summary>
    /// [EN] Audit logging utility for recording Protected Health Information (PHI) access events (HIPAA Security Rule-oriented, illustrative).
    /// Records who, when, what action, what resource, and IP address.
    /// [CN] 记录受保护健康信息(PHI)访问事件的审计日志工具（HIPAA安全规则取向，示例性质）。
    /// </summary>
    public static class AuditLog
    {
        /// <summary>
        /// [EN] Records an audit log entry for a PHI access event.
        /// Parameters: userId (the user accessing data), role (user's role), ipAddress (source IP),
        /// resourceType (FHIR resource type), resourceId (resource ID), action (READ/WRITE/DELETE/UPDATE).
        /// [CN] 记录一条PHI访问的审计日志条目。
        /// </summary>
        public static void Record(string userId, string role, string ipAddress,
            string resourceType, string resourceId, string action)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));
            Guard.NotNullOrEmpty(role, nameof(role));
            Guard.NotNullOrEmpty(ipAddress, nameof(ipAddress));
            Guard.NotNullOrEmpty(resourceType, nameof(resourceType));
            Guard.NotNullOrEmpty(resourceId, nameof(resourceId));
            Guard.NotNullOrEmpty(action, nameof(action));

            var record = new
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                UserId = userId,
                Role = role,
                IpAddress = ipAddress,
                Action = action,
                Resource = $"{resourceType}/{resourceId}",
                AuditMessage = "PHI access recorded (HIPAA Security Rule-oriented audit)"
            };

            SafeConsole.WriteLine("\n========== HIPAA AUDIT LOG ==========");
            SafeConsole.WriteLine(JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
            SafeConsole.WriteLine("=====================================\n");
        }
    }

