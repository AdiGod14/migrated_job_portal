using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace MigratedJobPortalAPI.Models
{
    public class Job
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string?   Id { get; set; } // MongoDB _id

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("employerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string EmployerId { get; set; }

        [BsonElement("employerName")]
        public string EmployerName { get; set; }

        [BsonElement("domain")]
        public string Domain { get; set; }

        [BsonElement("description")]
        public JobDescription Description { get; set; } = new();

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("salary")]
        public double Salary { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("applicantCount")]
        public int ApplicantCount { get; set; } = 0;

        [BsonElement("experience")]
        public int Experience { get; set; }

        [BsonElement("vacancies")]
        public int Vacancies { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "open";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class JobDescription
    {
        [BsonElement("overview")]
        public string Overview { get; set; } = string.Empty;

        [BsonElement("responsibilities")]
        public List<string> Responsibilities { get; set; } = new();

        [BsonElement("requiredSkills")]
        public List<string> RequiredSkills { get; set; } = new();

        [BsonElement("preferredSkills")]
        public List<string> PreferredSkills { get; set; } = new();

        [BsonElement("whatWeOffer")]
        public List<string> WhatWeOffer { get; set; } = new();
    }

}
