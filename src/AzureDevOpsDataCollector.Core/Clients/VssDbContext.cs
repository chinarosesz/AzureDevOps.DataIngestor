using AzureDevOpsDataCollector.Core.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssDbContext : DbContext
    {
        private readonly string connectionString;
        private readonly ILogger logger;

        public DbSet<VssRepositoryEntity> VssRepositoryEntities { get; set; }
        public DbSet<VssProjectEntity> VssProjectEntities { get; set; }

        public VssDbContext(ILogger logger, string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AzureDevOpsCollector") : base()
        {
            this.connectionString = connectionString;
            this.logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(this.connectionString);
            }
            else
            {
                // MigrateDatabaseToLatestVersion.ExecuteAsync(this).Wait();
                this.Database.Migrate();
            }
        }

        public async Task BulkInsertOrUpdateAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"Insert or Update {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkInsertOrUpdateAsync(this, entities, bulkConfig);
        }
    }
}
