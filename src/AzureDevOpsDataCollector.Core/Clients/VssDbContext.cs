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

        public VssDbContext() : base() 
        { 
        }

        public VssDbContext(ILogger logger, string connectionString) : base()
        {
            this.logger = logger;
            this.connectionString = connectionString;

            this.logger.LogInformation("Migrating database");
            this.Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && this.connectionString == null)
            {
                optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AzureDevOps");
            }
            else if (!optionsBuilder.IsConfigured && this.connectionString != null)
            {
                optionsBuilder.UseSqlServer(this.connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VssProjectEntity>()
                .HasIndex(p => p.Organization).IsClustered(false);

            modelBuilder.Entity<VssRepositoryEntity>()
                .HasIndex(p => p.Organization).IsClustered(false);
        }

        public async Task BulkInsertOrUpdateAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"InsertOrUpdating {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkInsertOrUpdateAsync(this, entities, bulkConfig);
        }

        public async Task BulkInsertOrUpdateOrDeleteAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"BulkInsertOrUpdateOrDelete {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkInsertOrUpdateOrDeleteAsync(this, entities, bulkConfig);
        }

        public async Task BulkInsertAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"BulkInsert {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkInsertAsync(this, entities, bulkConfig);
        }

        public async Task BulkDeleteAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"BulkDelete {entities.Count} {typeof(T).Name} entities");
            await DbContextBulkExtensions.BulkDeleteAsync(this, entities, bulkConfig);
        }
    }
}
