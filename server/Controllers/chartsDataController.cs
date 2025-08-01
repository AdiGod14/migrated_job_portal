using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMongoCollection<Application> _applicationsCollection;
        private readonly IMongoCollection<Job> _jobsCollection;

        public AnalyticsController(IMongoDatabase database)
        {
            _applicationsCollection = database.GetCollection<Application>("applications");
            _jobsCollection = database.GetCollection<Job>("jobs");
        }

        // 1. EMPLOYER - Status-wise application count
        [HttpGet("employer/status/{userId}")]
        public async Task<IActionResult> GetApplicationsDataForEmployerBasedOnStatus(string userId)
        {
            if (!ObjectId.TryParse(userId, out ObjectId objectId))
                return BadRequest("Invalid Employer ID format.");

            var results = await _applicationsCollection.Aggregate()
                .Match(app => app.EmployerId == objectId.ToString())
                .Group(app => app.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var allStatuses = new[] { "Applied", "In Progress", "Accepted", "Rejected" };
            var statusCounts = allStatuses.ToDictionary(status => status, _ => 0);

            foreach (var result in results)
            {
                if (statusCounts.ContainsKey(result.Status.ToString()))
                    statusCounts[result.Status.ToString()] = result.Count;
            }

            return Ok(statusCounts);
        }

        // 2. USER - Domain-wise application count
        [HttpGet("user/domain/{userId}")]
        public async Task<IActionResult> GetApplicationsByDomain(string userId)
        {
            if (!ObjectId.TryParse(userId, out ObjectId objectId))
                return BadRequest("Invalid User ID format.");

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("userId", objectId)),
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "jobs" },
            { "localField", "jobId" },
            { "foreignField", "_id" },
            { "as", "jobDetails" }
        }),
        new BsonDocument("$unwind", "$jobDetails"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$jobDetails.domain" },
            { "count", new BsonDocument("$sum", 1) }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "domain", "$_id" },
            { "count", 1 }
        })
    };

            var results = await _applicationsCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            // Convert BsonDocument to regular objects
            var domainCounts = results.Select(doc => new
            {
                domain = doc["domain"].AsString,
                count = doc["count"].AsInt32
            }).ToList();

            return Ok(domainCounts);
        }

        // 3. USER - Status-wise application count
        [HttpGet("user/status/{userId}")]
        public async Task<IActionResult> GetApplicationsDataForUserBasedOnStatus(string userId)
        {
            if (!ObjectId.TryParse(userId, out ObjectId objectId))
                return BadRequest("Invalid User ID format.");

            var results = await _applicationsCollection.Aggregate()
                .Match(app => app.UserId == objectId.ToString())
                .Group(app => app.Status, g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var allStatuses = new[] { "Applied", "In Progress", "Accepted", "Rejected" };
            var statusCounts = allStatuses.ToDictionary(status => status, _ => 0);

            foreach (var result in results)
            {
                if (statusCounts.ContainsKey(result.Status.ToString()))
                    statusCounts[result.Status.ToString()] = result.Count;
            }

            return Ok(statusCounts);
        }
    }
}
