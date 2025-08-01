using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using System;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notificationCollection;

        public NotificationService(IMongoDatabase database)
        {
            _notificationCollection = database.GetCollection<Notification>("notifications");
        }

        public async Task<bool> AddNotification(string userId, string message)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                Console.WriteLine($"[NotificationService] Invalid user ID: {userId}");
                return false;
            }

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow // Make sure your Notification model includes this field
            };

            try
            {
                await _notificationCollection.InsertOneAsync(notification);
                Console.WriteLine($"[NotificationService] Notification sent to user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Failed to send notification to user {userId}: {ex.Message}");
                return false;
            }
        }
    }
}
