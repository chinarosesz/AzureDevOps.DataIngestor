using AzureDevOpsDataCollector.Core.Entities;
using EFCore.AutomaticMigrations;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssDbContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<VssRepositoryEntity> VssRepositoryEntities { get; set; }
        public DbSet<VssProjectEntity> VssProjectEntities { get; set; }

        public VssDbContext(string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AzureDevOpsCollector") : base()
        {
            this.connectionString = connectionString;
            MigrateDatabaseToLatestVersion.ExecuteAsync(this).Wait();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(this.connectionString);
            }
        }

        public async Task BulkInsertOrUpdateAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            Logger.WriteLine($"Insert or Update {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkInsertOrUpdateAsync(this, entities, bulkConfig);
        }
    }
}
