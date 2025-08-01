using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace MigratedJobPortalAPI.Models
{
    public class Application
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonRequired]
        public string UserId { get; set; }

        [BsonElement("jobId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonRequired]
        public string JobId { get; set; }

        [BsonElement("employer")]
        [BsonRequired]
        public string Employer { get; set; }

        [BsonElement("employerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonRequired]
        public string EmployerId { get; set; }

        [BsonElement("status")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        [BsonElement("appliedAt")]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ApplicationStatus
    {
        Applied,
        [EnumMember(Value = "In Progress")]
        InProgress,
        Accepted,
        Rejected
    }
}
