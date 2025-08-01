using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMongoCollection<Notification> _notificationCollection;

        public NotificationsController(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]); // Replace with your DB name
            _notificationCollection = database.GetCollection<Notification>("notifications");
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddNotification(string userId, [FromBody] string message)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("Invalid user ID.");

            var notification = new Notification
            {
                UserId = userId,
                Message = message   
            };

            try
            {
                await _notificationCollection.InsertOneAsync(notification);
                Console.WriteLine($"[notifyUser] Notification sent to user {userId}");
                return Ok(new { message = "Notification added successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[notifyUser] Failed to send notification to user {userId}: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId, [FromQuery] int page = 1)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("Invalid user ID.");

            const int limit = 5;
            var skip = (page - 1) * limit;

            try
            {
                Console.WriteLine($"Fetching notifications for userId: {userId}, page: {page}");

                var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
                var total = await _notificationCollection.CountDocumentsAsync(filter);

                var notifications = await _notificationCollection.Find(filter)
                    .SortByDescending(n => n.CreatedAt)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync();

                return Ok(new
                {
                    notifications,
                    pagination = new
                    {
                        total,
                        page,
                        totalPages = (int)Math.Ceiling((double)total / limit)
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching notifications: " + ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearNotifications(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("Invalid user ID.");

            try
            {
                Console.WriteLine($"Clearing notifications for userId: {userId}");

                var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
                await _notificationCollection.DeleteManyAsync(filter);

                return Ok(new { message = "Notifications cleared successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error clearing notifications: " + ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
