using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MigratedJobPortalAPI.Models;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using MigratedJobPortalAPI;
using MongoDB.Driver;
using MigratedJobPortalAPI.Services;
using MigratedJobPortalAPI.Utils;


var builder = WebApplication.CreateBuilder(args);

// Register MongoDB conventions
var conventionPack = new ConventionPack
{
    new IgnoreExtraElementsConvention(true)
};
ConventionRegistry.Register("IgnoreExtraElements", conventionPack, _ => true);

// Register enum serializer to store enums as strings in MongoDB
BsonSerializer.RegisterSerializer(
    typeof(ApplicationStatus),
    new ApplicationStatusStringSerializer()
);

// Read JWT settings from appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtRefreshKey = builder.Configuration["Jwt:RefreshKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<JobUtils>();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false, // Optional: set to true if validating audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
// Add MongoDB services
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDB:ConnectionString"];
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"];
    return client.GetDatabase(databaseName);
});

builder.Services.AddSingleton<NotificationService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var client = new MongoClient(builder.Configuration["MongoDB:ConnectionString"]);
    var database = client.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
    return new NotificationService(database);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // Optional, if you're using cookies or auth headers
        );
});


var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthentication(); // <-- Add this to enable JWT auth middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
