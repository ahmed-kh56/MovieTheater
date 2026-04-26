using MediatR;
using System.Text.Json;

namespace MovieRatingApp.Models.Common
{
    public class EventsOutbox :IHasId
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; }
        public bool IsHandled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? HandledAt { get; set; } = null;
        public List<string> ErrorMessage { get; set; } = new List<string>();
        public int RetryCount { get; set; } = 0;
        public string Notification { get; set; }
        public EventsOutbox(INotification notification)
        {
            Id = Guid.NewGuid();
            var type = notification.GetType();
            EventType = type.FullName;
            CreatedAt = DateTime.UtcNow;
            IsHandled = false;
            Notification = JsonSerializer.Serialize(notification, type);
        }
        public void MarkAsHandled()
        {
            IsHandled = true;
            HandledAt = DateTime.UtcNow;
        }
        public void MarkAsFailed(string message)
        {
            ErrorMessage.Add(message);
            RetryCount++;
        }
        public EventsOutbox() { }
    }
}
