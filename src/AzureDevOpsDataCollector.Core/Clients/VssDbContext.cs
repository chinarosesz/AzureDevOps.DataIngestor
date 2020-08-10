using AzureDevOpsDataCollector.Core.Entities;
using EFCore.BulkExtensions;
using EntityFrameworkCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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
        public DbSet<VssPullRequestEntity> VssPullRequestEntities { get; set; }
        public DbSet<VssBuildDefinitionEntity> VssBuildDefinitionEntities { get; set; }
        public DbSet<VssBuildDefinitionStepEntity> VssBuildDefinitionStepEntities { get; set; }
        public DbSet<VssPullRequestWatermarkEntity> VssPullRequestWatermarkEntities { get; set; }

        public VssDbContext() : base() 
        { 
        }

        public VssDbContext(string connectionString, ILogger logger) : base()
        {
            this.logger = logger;
            this.connectionString = connectionString;

            this.logger.LogInformation($"Migrate database {this.Database.GetDbConnection().Database} from server {this.Database.GetDbConnection().DataSource}");
            this.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
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

            modelBuilder.Entity<VssPullRequestEntity>()
                .HasKey(p => new { p.PullRequestId, p.RepositoryId });

            modelBuilder.Entity<VssBuildDefinitionEntity>()
                .HasKey(p => new { p.Id, p.ProjectId });

            modelBuilder.Entity<VssBuildDefinitionStepEntity>()
                .HasKey(p => new { p.ProjectId, p.BuildDefinitionId, p.StepNumber });

            modelBuilder.Entity<VssPullRequestWatermarkEntity>()
                .HasKey(p => new { p.PullRequestStatus, p.ProjectId });
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

        public void BulkInsert<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"BulkInsert {entities.Count} {typeof(T).Name} entities");
            BulkExtensions.BulkInsert(this, entities);
        }

        public void BulkDelete<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            this.logger.LogInformation($"BulkDelete {entities.Count} {typeof(T).Name} entities");
            BulkExtensions.BulkDelete(this, entities);
        }
    }
}
