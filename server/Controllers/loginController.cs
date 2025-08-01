using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;
using System;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Employer> _employers;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration, MongoDbContext dbContext)
        {
            _configuration = configuration;
            _users = dbContext.Users;
            _employers = dbContext.Employers;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine("Login attempt received");

                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    Console.WriteLine("Missing email or password");
                    return BadRequest(new { message = "Email and password are required" });
                }

                Console.WriteLine($"Looking up account for: {request.Email}");

                var userTask = _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
                var employerTask = _employers.Find(e => e.Email == request.Email).FirstOrDefaultAsync();

                await Task.WhenAll(userTask, employerTask);

                var user = userTask.Result;
                var employer = employerTask.Result;
                var account = (object)user ?? employer;

                if (account == null)
                {
                    Console.WriteLine($"No account found for: {request.Email}");
                    return NotFound(new { message = "Invalid email or password" });
                }

                string hashedPassword = user != null ? user.Password : employer.Password;
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, hashedPassword);

                if (!isPasswordValid)
                {
                    Console.WriteLine($"Invalid password attempt for: {request.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var role = user != null ? "user" : "employer";
                var id = user != null ? user.Id : employer.Id;
                var name = user != null ? user.Name : employer.Name;
                var email = user != null ? user.Email : employer.Email;

                var accessToken = GenerateJwtToken(id, email, role, _configuration["Jwt:Key"], 60); // 1 hour
                var refreshToken = GenerateJwtToken(id, email, role, _configuration["Jwt:RefreshKey"], 10080); // 7 days

                return Ok(new
                {
                    message = "Login successful",
                    token = accessToken,
                    refresh_token = refreshToken,
                    user = new
                    {
                        _id = id,
                        email = email,
                        role = role,
                        name = name
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login error: " + ex.Message);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public IActionResult RefreshAccessToken([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                Console.WriteLine("No refresh token provided");
                return Unauthorized(new { message = "Refresh token required" });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshKey"]);

                var principal = tokenHandler.ValidateToken(request.RefreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                var newAccessToken = GenerateJwtToken(id, email, role, _configuration["Jwt:Key"], 60);

                return Ok(new
                {
                    accessToken = newAccessToken,
                    message = "Access token refreshed"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid or expired refresh token: " + ex.Message);
                return StatusCode(403, new { message = "Invalid or expired refresh token" });
            }
        }

        private string GenerateJwtToken(string id, string email, string role, string secret, int expiresInMinutes)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }
}