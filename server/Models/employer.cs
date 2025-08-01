using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MigratedJobPortalAPI.Models
{
    public class Employer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement("email")]
        [BsonRequired]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } = "employer";

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("experience")]
        public int? Experience { get; set; }

        [BsonElement("designation")]
        public string? Designation { get; set; }

        [BsonElement("domain")]
        public string? Domain { get; set; }

        [BsonElement("profilePicture")]
        public EmployerProfilePicture? ProfilePicture { get; set; }
    }

    public class EmployerProfilePicture
    {
        [BsonElement("data")]
        public string? Data { get; set; } // base64 string

        [BsonElement("contentType")]
        public string? ContentType { get; set; }
    }
}
