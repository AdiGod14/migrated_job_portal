using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MigratedJobPortalAPI.Models;
using MigratedJobPortalAPI.Utils;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MigratedJobPortalAPI.Services;


namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMongoCollection<Application> _applicationCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Job> _jobCollection;
        private readonly NotificationService _notificationService;


        public ApplicationsController(MongoDbContext context, NotificationService notificationService)
        {
            _applicationCollection = context.Applications;
            _userCollection = context.Users;
            _jobCollection = context.Jobs;
            _notificationService = notificationService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyForJob([FromBody] ApplyRequest request)
        {
            Console.WriteLine($"[applyForJob] Request received: {request.UserId}, {request.JobId}");

            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.JobId))
                return BadRequest(new { message = "User ID and Job ID are required." });

            if (!ObjectId.TryParse(request.UserId, out _) || !ObjectId.TryParse(request.JobId, out _))
                return BadRequest(new { message = "Invalid User ID or Job ID format." });

            try
            {
                var user = await _userCollection.Find(u => u.Id == request.UserId).FirstOrDefaultAsync();
                var job = await _jobCollection.Find(j => j.Id == request.JobId).FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "User not found." });

                if (job == null)
                    return NotFound(new { message = "Job not found." });

                var application = new Application
                {
                    UserId = request.UserId,
                    JobId = request.JobId,
                    Employer = job.EmployerName,
                    EmployerId = job.EmployerId,
                };

                try
                {
                    await _applicationCollection.InsertOneAsync(application);
                }
                catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(new { message = "You have already applied for this job." });
                }

                await JobUtils.ChangeApplicantCount(_jobCollection, job.Id); // Increment count

                return Created("", new { message = "Application submitted successfully.", application });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[applyForJob] Error: {ex}");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAppliedJobs(string userId, int page = 1, int limit = 5)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest(new { message = "Invalid User ID format." });

            try
            {
                var totalApplications = await _applicationCollection.CountDocumentsAsync(a => a.UserId == userId);

                var applications = await _applicationCollection.Find(a => a.UserId == userId)
                    .Skip((page - 1) * limit)
                    .Limit(limit)
                    .ToListAsync();

                var appliedJobs = new List<object>();

                foreach (var app in applications)
                {
                    var job = await _jobCollection.Find(j => j.Id == app.JobId).FirstOrDefaultAsync();
                    if (job != null)
                    {
                        appliedJobs.Add(new
                        {
                            applicationId = app.Id,
                            job.Id,
                            job.Title,
                            job.ApplicantCount,
                            job.Salary,
                            job.Company,
                            job.Vacancies,
                            job.CreatedAt,
                            job.Type,
                            job.Description,
                            job.Location,
                            job.EmployerName,
                            job.Experience,
                            app.Status

                        });
                    }
                }

                return Ok(new
                {
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalApplications / limit),
                    totalApplications,
                    jobs = appliedJobs
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[getUserAppliedJobs] Error: {ex}");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPatch("{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(string applicationId, [FromBody] UpdateStatusRequest req)
        {
            var validStatuses = new[] { "InProgress", "Accepted", "Rejected" };

            if (!ObjectId.TryParse(applicationId, out _))
                return BadRequest(new { message = "Invalid application ID format." });

            if (!Enum.TryParse<ApplicationStatus>(req.Status, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            try
            {
                var application = await _applicationCollection.Find(a => a.Id == applicationId).FirstOrDefaultAsync();
                if (application == null)
                    return NotFound(new { message = "Application not found." });

                if (application.Status.ToString() == req.Status)
                    return Ok(new { message = "Status is already up to date.", application });

                var job = await _jobCollection.Find(j => j.Id == application.JobId).FirstOrDefaultAsync();
                if (job == null)
                    return NotFound(new { message = "Associated job not found." });

                if (application.Status != ApplicationStatus.Accepted && newStatus == ApplicationStatus.Accepted)
                {
                    if (job.Vacancies <= 0)
                        return BadRequest(new { message = "No vacancies available for this job." });

                    await JobUtils.ChangeVacancyCount(_jobCollection, job.Id, "dec");
                }
                else if (application.Status == ApplicationStatus.Accepted && newStatus != ApplicationStatus.Accepted)
                {
                    await JobUtils.ChangeVacancyCount(_jobCollection, job.Id, "inc");
                }

                application.Status = newStatus;
                await _applicationCollection.ReplaceOneAsync(a => a.Id == applicationId, application);

                await _notificationService.AddNotification(application.UserId, $"Your application for the job \"{job.Title}\" has been updated to '{req.Status}'.");

                return Ok(new { message = "Application status updated and user notified.", application });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[updateApplicationStatus] Error: {ex}");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpDelete("revoke/{applicationId}")]
        public async Task<IActionResult> RevokeApplication(string applicationId)
        {
            if (!ObjectId.TryParse(applicationId, out _))
                return BadRequest(new { message = "Invalid Application ID format." });

            try
            {
                var application = await _applicationCollection.Find(a => a.Id == applicationId).FirstOrDefaultAsync();
                if (application == null)
                    return NotFound(new { message = "Application not found" });

                await JobUtils.ChangeApplicantCount(_jobCollection, application.JobId, "dec");

                if (application.Status == ApplicationStatus.Accepted)
                    await JobUtils.ChangeVacancyCount(_jobCollection, application.JobId, "inc");

                await _applicationCollection.DeleteOneAsync(a => a.Id == applicationId);

                return Ok(new { message = "Application revoked successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[revokeApplication] Error: {ex}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    // DTOs
    public class ApplyRequest
    {
        public string UserId { get; set; }
        public string JobId { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}
