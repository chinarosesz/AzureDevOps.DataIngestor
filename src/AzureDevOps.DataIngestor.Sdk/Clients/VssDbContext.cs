using AzureDevOps.DataIngestor.Sdk.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace AzureDevOps.DataIngestor.Sdk.Clients
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
        public DbSet<VssBuildEntity> VssBuildEntities { get; set; }
        public DbSet<VssPullRequestWatermarkEntity> VssPullRequestWatermarkEntities { get; set; }
        public DbSet<VssBuildWatermarkEntity> VssBuildWatermarkEntities { get; set; }
        public DbSet<VssCommitEntity> VssCommitEntities { get; set; }
        public DbSet<VssCommitWatermarkEntity> VssCommitWatermarkEntities { get; set; }
        public VssDbContext() : base() { }
        
        public VssDbContext(ILogger logger, string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=VssAzureDevOps") : base()
        {
            this.logger = logger;
            this.connectionString = connectionString;
            this.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            try
            {
                this.Database.Migrate();
            }
            catch (ArgumentException)
            {
                this.logger.LogInformation("Failed to migrate database with current connection string");
                throw;
            }
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
                .HasIndex(p => p.Organization);

            modelBuilder.Entity<VssRepositoryEntity>()
                .HasIndex(p => p.Organization);

            modelBuilder.Entity<VssPullRequestEntity>()
                .HasKey(p => new { p.PullRequestId, p.RepositoryId });

            modelBuilder.Entity<VssCommitEntity>()
                .HasKey(p => new { p.CommitId, p.RepositoryId });

            modelBuilder.Entity<VssCommitWatermarkEntity>()
                .HasKey(p => new { p.RepositoryId });

            modelBuilder.Entity<VssBuildDefinitionEntity>()
                .HasKey(p => new { p.Id, p.ProjectId });

            modelBuilder.Entity<VssBuildDefinitionStepEntity>()
                .HasKey(p => new { p.ProjectId, p.BuildDefinitionId, p.StepNumber });

            modelBuilder.Entity<VssBuildEntity>()
                .HasKey(p => new { p.Id, p.ProjectId });

            modelBuilder.Entity<VssPullRequestWatermarkEntity>()
                .HasKey(p => new { p.PullRequestStatus, p.ProjectId });

            modelBuilder.Entity<VssBuildWatermarkEntity>()
                .HasKey(p => new { p.ProjectId });
        }
    }
}
