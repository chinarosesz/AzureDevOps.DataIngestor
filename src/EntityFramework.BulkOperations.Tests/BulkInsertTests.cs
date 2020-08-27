using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkOperations.Tests
{
    [TestClass]
    public class BulkInsertTests
    {
        [TestMethod]
        public void Insert100KRecords()
        {
            ILogger logger = this.RedirectLoggerToConsole();
            List<EmployeeEntity> entities = new List<EmployeeEntity>();

            for (int i = 0; i < 100000; i++)
            {
                EmployeeEntity employeeEntity = new EmployeeEntity
                {
                    EmailAddress = "aaaaaaa@hotmail.com",
                    HiredDate = DateTime.Now,
                    Address = "5555 1st NE, Redmond WA 98052",
                    First = "Bob",
                    Last = "Jones",
                    Gender = "Male",
                    EmployeeId = Guid.NewGuid(),
                    Organization = "Technology",
                    Team = "VisionCore",
                    UniqueName = "bobjones",
                    Data = "a",
                };
                entities.Add(employeeEntity);
            }
            using EmployeeDbContext context = new EmployeeDbContext(null, logger);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            int insertedResult = context.BulkInsert(entities);
            transaction.Commit();
            logger.LogInformation($"Succesfully deleted 0 and inserted {insertedResult} records");
        }

        private ILogger RedirectLoggerToConsole()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole((ConsoleLoggerOptions options) =>
                {
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss: ";
                    options.Format = ConsoleLoggerFormat.Systemd;
                });
            });

            ILogger logger = loggerFactory.CreateLogger(string.Empty);

            return logger;
        }
    }
}
