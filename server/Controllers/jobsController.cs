using MigratedJobPortalAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Utils;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<JobsController> _logger;
    private readonly JobUtils _jobUtils;


    public JobsController(MongoDbContext context, ILogger<JobsController> logger, JobUtils jobUtils)
    {
        _context = context;
        _logger = logger;
        _jobUtils = jobUtils;
    }

    [HttpPost]
    public async Task<IActionResult> AddJob([FromBody] Job job)
    {
        _logger.LogInformation("[AddJob] Incoming job data: {@Job}", job);

        if (job.Description == null || string.IsNullOrEmpty(job.Description.Overview)
            || job.Description.Responsibilities == null
            || job.Description.RequiredSkills == null)
        {
            return BadRequest(new { message = "Missing required fields in job description" });
        }

        job.Status ??= "open";
        await _context.Jobs.InsertOneAsync(job);
        _logger.LogInformation("[AddJob] Job created: {JobId}", job.Id);
        return StatusCode(201, new { message = "Job added successfully" });
    }

    [HttpGet("employer/{employerId}")]
    public async Task<IActionResult> GetJobsForEmployer(string employerId, int page = 1)
    {
        const int limit = 12;
        var filter = Builders<Job>.Filter.Eq(j => j.EmployerId, employerId);
        var total = await _context.Jobs.CountDocumentsAsync(filter);

        var jobs = await _context.Jobs
            .Find(filter)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        if (!jobs.Any())
            return NotFound(new { message = "No jobs found for this employer" });

        return Ok(new
        {
            currentPage = page,
            totalPages = (int)Math.Ceiling((double)total / limit),
            totalJobs = total,
            jobs
        });
    }

    [HttpPut("{jobId}")]

    public async Task<IActionResult> UpdateJob(string jobId, [FromBody] Job update)
    {
        _logger.LogInformation("[UpdateJob] jobId: {JobId}", jobId);

        var updateDef = Builders<Job>.Update
            .Set(j => j.Title, update.Title)
            .Set(j => j.Description, update.Description)
            .Set(j => j.Company, update.Company)
            .Set(j => j.Location, update.Location)
            .Set(j => j.Salary, update.Salary)
            .Set(j => j.Type, update.Type)
            .Set(j => j.Vacancies, update.Vacancies)
            .Set(j => j.Experience, update.Experience)
            .Set(j => j.Domain, update.Domain)
            .Set(j => j.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Jobs.UpdateOneAsync(j => j.Id == jobId, updateDef);

        if (result.MatchedCount == 0)
            return NotFound(new { message = "Job not found" });

        return Ok(new { message = "Job updated successfully" });
    }


    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(string jobId)
    {
        if (!ObjectId.TryParse(jobId, out _))
            return BadRequest(new { message = "Invalid job ID format" });

        var appDeleteResult = await _context.Applications.DeleteManyAsync(a => a.JobId == jobId);
        var jobDeleteResult = await _context.Jobs.DeleteOneAsync(j => j.Id == jobId);

        if (jobDeleteResult.DeletedCount == 0)
            return NotFound(new { message = "Job not found" });

        return Ok(new
        {
            message = "Job and related applications deleted successfully",
            deletedApplications = appDeleteResult.DeletedCount
        });
    }

    [HttpGet("summary/{employerId}")]
    public async Task<IActionResult> GetJobsSummaryForEmployer(string employerId, int page = 1)
    {
        const int limit = 9;
        var filter = Builders<Job>.Filter.Eq(j => j.EmployerId, employerId);

        var summaries = await _context.Jobs.Find(filter)
            .Project(j => new { j.Id, j.Title, j.Vacancies, j.ApplicantCount })
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        var total = await _context.Jobs.CountDocumentsAsync(filter);

        return Ok(new
        {
            currentPage = page,
            totalPages = (int)Math.Ceiling((double)total / limit),
            totalJobs = total,
            jobs = summaries
        });
    }

    [HttpGet("domain")]
    public async Task<IActionResult> GetJobsByDomain(string domain, string userId)
    {
        if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "Domain and user ID are required." });

        var appliedJobIds = await _context.Applications.Find(a => a.UserId == userId)
            .Project(a => a.JobId)
            .ToListAsync();

        var jobs = await _context.Jobs.Find(j =>
                j.Domain == domain && j.Status == "open" && !appliedJobIds.Contains(j.Id))
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJobById(string jobId)
    {
        var job = await _context.Jobs.Find(j => j.Id == jobId).FirstOrDefaultAsync();
        if (job == null)
            return NotFound(new { message = "Job not found" });

        var applications = await _context.Applications
            .Find(a => a.JobId == jobId)
            .SortByDescending(a => a.AppliedAt)
            .ToListAsync();

        var enrichedApplicants = new List<object>();
        foreach (var app in applications)
        {
            var user = await _context.Users
                .Find(u => u.Id == app.UserId)
                .FirstOrDefaultAsync();

            enrichedApplicants.Add(new
            {
                _id = app.Id,
                jobId = app.JobId,
                status = app.Status,
                appliedAt = app.AppliedAt,
                userId = user // now Angular gets full user info
            });
        }

        return Ok(new { job, applicants = enrichedApplicants });
    }


    [HttpGet("searchJobs")]
    public async Task<IActionResult> SearchJobsForUsers(
        string? domain,
        string? preferredDomain,
        string? experience,
        string? expectedSalary,
        string? type,
        string? search,
        string? userId,
        int page = 1)
    {
        const int limit = 12;
        var filter = Builders<Job>.Filter.Eq(j => j.Status, "open");
        var filters = new List<FilterDefinition<Job>> { filter };

        if (string.IsNullOrEmpty(search))
        {
            if (!string.IsNullOrEmpty(domain))
                filters.Add(Builders<Job>.Filter.Eq(j => j.Domain, domain));
            else if (!string.IsNullOrEmpty(preferredDomain))
                filters.Add(Builders<Job>.Filter.Eq(j => j.Domain, preferredDomain));
        }

        if (!string.IsNullOrEmpty(type))
            filters.Add(Builders<Job>.Filter.Eq(j => j.Type, type));

        if (!string.IsNullOrEmpty(experience))
            filters.Add(Builders<Job>.Filter.Lte(j => j.Experience, int.Parse(experience)));

        if (!string.IsNullOrEmpty(expectedSalary))
            filters.Add(Builders<Job>.Filter.Gte(j => j.Salary, int.Parse(expectedSalary)));

        if (!string.IsNullOrEmpty(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            filters.Add(Builders<Job>.Filter.Or(
                Builders<Job>.Filter.Regex(j => j.Title, regex),
                Builders<Job>.Filter.Regex(j => j.Company, regex),
                Builders<Job>.Filter.Regex(j => j.Location, regex),
                Builders<Job>.Filter.Regex("description.overview", regex)
            ));
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var appliedJobs = await _context.Applications.Find(a => a.UserId == userId)
                .Project(a => a.JobId)
                .ToListAsync();
            filters.Add(Builders<Job>.Filter.Nin(j => j.Id, appliedJobs));
        }

        var finalFilter = Builders<Job>.Filter.And(filters);

        var jobs = await _context.Jobs.Find(finalFilter)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        var total = await _context.Jobs.CountDocumentsAsync(finalFilter);

        return Ok(new
        {
            jobs,
            pagination = new
            {
                total,
                page,
                totalPages = (int)Math.Ceiling((double)total / limit)
            }
        });
    }

}
