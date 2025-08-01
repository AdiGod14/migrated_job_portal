using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MigratedJobPortalAPI.Models
{
    public class User
    {
        [BsonId] // Primary key
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement("email")]
        [BsonRequired]
        public string Email { get; set; }

        [BsonElement("password")]
        [BsonRequired]
        public string Password { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } = "user";

        [BsonElement("preferredDomain")]
        public string PreferredDomain { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("profilePicture")]
        public ProfilePicture? ProfilePicture { get; set; }

        [BsonElement("experience")]
        public int? Experience { get; set; }

        [BsonElement("resume")]
        public Resume? Resume { get; set; }
    }

    public class ProfilePicture
    {
        [BsonElement("data")]
        public string? Data { get; set; } // base64 string

        [BsonElement("contentType")]
        public string? ContentType { get; set; }
    }

    public class Resume
    {
        [BsonElement("data")]
        public string? Data { get; set; } // base64 PDF

        [BsonElement("contentType")]
        public string? ContentType { get; set; }
    }
}
