using System.Text.Json;

namespace MovieRatingApp.Models
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid EntityId { get; private set; }
        public string EntityName { get; private set; }
        public string EntityData { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public AuditActionType AuditAction { get; private set; }

        public AuditLog(AuditActionType auditAction,IHasId hasId)
        {
            var type = hasId.GetType();
            Id = Guid.NewGuid();
            EntityId = hasId.Id;
            EntityName = type.Name;
            CreatedDate = DateTime.UtcNow;
            AuditAction = auditAction;
            EntityData = JsonSerializer.Serialize(
                    hasId,
                    type,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
        }
        public AuditLog() { }
    }
    public enum AuditActionType
    {
        Updated,
        Deleted
    }
}
