using System.Text.Json;

namespace Shared_Library.Shared;
    /// <summary>
    /// [EN] HIPAA-compliant audit logging utility for recording Protected Health Information (PHI) access.
    /// Records who, when, what action, what resource, and IP address.
    /// [CN] 符合HIPAA标准的审计日志工具，用于记录受保护健康信息(PHI)的访问。
    /// </summary>
    public static class AuditLog
    {
        /// <summary>
        /// [EN] Records a HIPAA-compliant audit log entry for PHI access.
        /// Parameters: userId (the user accessing data), role (user's role), ipAddress (source IP),
        /// resourceType (FHIR resource type), resourceId (resource ID), action (READ/WRITE/DELETE/UPDATE).
        /// [CN] 记录一条符合HIPAA标准的审计日志条目，用于PHI访问。
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
                AuditMessage = "PHI access recorded for HIPAA compliance"
            };

            Console.WriteLine("\n========== HIPAA AUDIT LOG ==========");
            Console.WriteLine(JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("=====================================\n");
        }
    }
