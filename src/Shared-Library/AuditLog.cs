using System.Text.Json;

namespace Shared_Library
{
    public static class AuditLog
    {
        // HIPAA 要求：必须记录：谁、何时、做什么、访问什么、IP
        // HIPAA requires to record: who, when, what action, what resource, and IP address
        public static void Record(string userId, string role, string ipAddress,
            string resourceType, string resourceId, string action)
        {
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
}
