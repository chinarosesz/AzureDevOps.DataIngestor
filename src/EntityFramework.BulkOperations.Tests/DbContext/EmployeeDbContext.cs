using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace EntityFramework.BulkOperations.Tests
{
    public class EmployeeDbContext : DbContext
    {
        private readonly string connectionString;
        private readonly ILogger logger;

        public DbSet<EmployeeEntity> EmployeeEntities { get; set; }
        public DbSet<EmployeeDataEntity> EmployeeDataEntities { get; set; }
        public DbSet<EmployeeWithDataEntity> EmployeeWithDataEntities { get; set; }
        public DbSet<EmployeeWithCompressedDataEntity> EmployeeWithCompressedDataEntities { get; set; }

        public EmployeeDbContext() : base() 
        { 
        }

        public EmployeeDbContext(string connectionString, ILogger logger) : base()
        {
            this.logger = logger;
            this.connectionString = connectionString;

            this.logger.LogInformation($"Create a new database context using database {this.Database.GetDbConnection().Database} from server {this.Database.GetDbConnection().DataSource}");
            this.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
            this.Database.EnsureDeleted();
            this.Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && this.connectionString == null)
            {
                optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EmployeeDatabase");
            }
            else if (!optionsBuilder.IsConfigured && this.connectionString != null)
            {
                optionsBuilder.UseSqlServer(this.connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
