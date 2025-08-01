using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using MigratedJobPortalAPI.Models;

namespace MigratedJobPortalAPI.Utils
{
    public class JobUtils
    {
        public static async Task ChangeApplicantCount(IMongoCollection<Job> jobCollection, string jobId, string action = "inc")
        {
            try
            {
                int incrementValue = action == "dec" ? -1 : 1;

                var update = Builders<Job>.Update.Inc(j => j.ApplicantCount, incrementValue);
                var result = await jobCollection.UpdateOneAsync(
                    j => j.Id == jobId,
                    update
                );

                if (result.ModifiedCount == 0)
                {
                    Console.WriteLine($"[ChangeApplicantCount] No job found with ID: {jobId}");
                }
                else
                {
                    Console.WriteLine($"[ChangeApplicantCount] Successfully {(action == "dec" ? "decremented" : "incremented")} applicantCount for job {jobId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChangeApplicantCount] Error {(action == "dec" ? "decrementing" : "incrementing")} applicantCount for job {jobId}: {ex}");
            }
        }

        public static async Task ChangeVacancyCount(IMongoCollection<Job> jobCollection, string jobId, string action = "inc")
        {
            try
            {
                int incrementValue = action == "dec" ? -1 : 1;

                var update = Builders<Job>.Update.Inc(j => j.Vacancies, incrementValue);
                var result = await jobCollection.UpdateOneAsync(
                    j => j.Id == jobId,
                    update
                );

                if (result.ModifiedCount == 0)
                {
                    Console.WriteLine($"[ChangeVacancyCount] No job found with ID: {jobId}");
                }
                else
                {
                    Console.WriteLine($"[ChangeVacancyCount] Successfully {(action == "dec" ? "decremented" : "incremented")} vacancies for job {jobId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChangeVacancyCount] Error {(action == "dec" ? "decrementing" : "incrementing")} vacancies for job {jobId}: {ex}");
            }
        }
    }
}
