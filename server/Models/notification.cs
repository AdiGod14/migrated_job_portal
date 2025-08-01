using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MigratedJobPortalAPI.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        [BsonRequired]
        public string UserId { get; set; }

        [BsonElement("message")]
        [BsonRequired]
        public string Message { get; set; }

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
