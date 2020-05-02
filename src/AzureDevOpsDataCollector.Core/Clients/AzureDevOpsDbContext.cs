using AzureDevOpsDataCollector.Core.Entities;
using EFCore.AutomaticMigrations;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureDevOpsDbContext : DbContext
    {
        public DbSet<RepositoryEntity> AzureDevOpsRepositoryEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=AzureDevOpsCollector");
            }
        }

        public async Task BulkInsertOrUpdateOrDeleteAsync<T>(IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            Logger.WriteLine($"Insert/Update/Delete {entities.Count} entities...");
            await DbContextBulkExtensions.BulkInsertOrUpdateOrDeleteAsync(this, entities, bulkConfig);
            Logger.WriteLine($"Insert/Update/Delete {entities.Count} successfully");
        }
    }
}
