using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EntityFramework.BulkOperations.Tests
{
    [TestClass]
    public class BulkInsertTests
    {
        [TestMethod]
        public void Insert_50K_Records_With_Large_Data_In_Same_Table()
        {
            ILogger logger = this.RedirectLoggerToConsole();
            List<EmployeeWithDataEntity> entities = new List<EmployeeWithDataEntity>();

            StringBuilder largeText = new StringBuilder(25000);
            for (int i = 0; i < 25000; i++)
            {
                largeText.Append("a");
            };
            string largeTextString = largeText.ToString();

            for (int i = 0; i < 50000; i++)
            {
                EmployeeWithDataEntity employeeEntity = new EmployeeWithDataEntity
                {
                    EmailAddress = "aaaaaaa@hotmail.com",
                    HiredDate = DateTime.Now,
                    Address = "5555 1st NE, Redmond WA 98052",
                    First = "Bob",
                    Last = "Jones",
                    Gender = "Male",
                    EmployeeId = $"employee{i}",
                    Organization = "Technology",
                    Team = "VisionCore",
                    UniqueName = "bobjones",
                    Data = largeTextString,
                };
                entities.Add(employeeEntity);
            }

            logger.LogInformation("Ingesting data...");
            using EmployeeDbContext context = new EmployeeDbContext(null, logger);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            int insertedResult = context.BulkInsertOrUpdate(entities);
            transaction.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult} records");

            logger.LogInformation("Ingesting data...");
            using IDbContextTransaction transaction2 = context.Database.BeginTransaction();
            int insertedResult2 = context.BulkInsertOrUpdate(entities);
            transaction2.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult2} records");
        }

        [TestMethod]
        public void Insert_50K_Records_With_Large_Compressed_Data_In_Same_Table()
        {
            ILogger logger = this.RedirectLoggerToConsole();
            List<EmployeeWithCompressedDataEntity> entities = new List<EmployeeWithCompressedDataEntity>();

            StringBuilder largeText = new StringBuilder(25000);
            for (int i = 0; i < 25000; i++)
            {
                largeText.Append("a");
            };
            byte[] compressedData = this.Compress(largeText.ToString());

            for (int i = 0; i < 50000; i++)
            {
                EmployeeWithCompressedDataEntity employeeEntity = new EmployeeWithCompressedDataEntity
                {
                    EmailAddress = "aaaaaaa@hotmail.com",
                    HiredDate = DateTime.Now,
                    Address = "5555 1st NE, Redmond WA 98052",
                    First = "Bob",
                    Last = "Jones",
                    Gender = "Male",
                    EmployeeId = $"employee{i}",
                    Organization = "Technology",
                    Team = "VisionCore",
                    UniqueName = "bobjones",
                    Data = compressedData,
                };
                entities.Add(employeeEntity);
            }

            logger.LogInformation("Ingesting data...");
            using EmployeeDbContext context = new EmployeeDbContext(null, logger);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            int insertedResult = context.BulkInsertOrUpdate(entities);
            transaction.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult} records");

            logger.LogInformation("Ingesting data...");
            using IDbContextTransaction transaction2 = context.Database.BeginTransaction();
            int insertedResult2 = context.BulkInsertOrUpdate(entities);
            transaction2.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult2} records");
        }

        [TestMethod]
        public void Insert_50K_Records_With_Large_Text_In_Separate_Table()
        {
            ILogger logger = this.RedirectLoggerToConsole();

            List<EmployeeEntity> entities = new List<EmployeeEntity>();
            List<EmployeeDataEntity> employeeDataEntities = new List<EmployeeDataEntity>();

            // A large string of 25K characters
            StringBuilder largeText = new StringBuilder(25000);
            for (int i = 0; i < 25000; i++)
            {
                largeText.Append("a");
            };
            string largeTextString = largeText.ToString();

            for (int i = 0; i < 50000; i++)
            {
                string employeeId = $"employee{i}";

                EmployeeEntity employeeEntity = new EmployeeEntity
                {
                    EmailAddress = "aaaaaaa@hotmail.com",
                    HiredDate = DateTime.Now,
                    Address = "5555 1st NE, Redmond WA 98052",
                    First = "Bob",
                    Last = "Jones",
                    Gender = "Male",
                    EmployeeId = employeeId,
                    Organization = "Technology",
                    Team = "VisionCore",
                    UniqueName = "bobjones",
                };
                entities.Add(employeeEntity);

                EmployeeDataEntity employeeDataEntity = new EmployeeDataEntity
                {
                    EmployeeId = employeeId,
                    Data = largeTextString,
                };
                employeeDataEntities.Add(employeeDataEntity);
            }

            logger.LogInformation("Ingesting data...");
            using EmployeeDbContext context = new EmployeeDbContext(null, logger);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            int insertedResult = context.BulkInsertOrUpdate(entities);
            int dataInsertedResult = context.BulkInsertOrUpdate(employeeDataEntities);
            transaction.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult} employee records and {dataInsertedResult} employee data records");

            logger.LogInformation("Ingesting data...");
            using IDbContextTransaction transaction2 = context.Database.BeginTransaction();
            int insertedResult2 = context.BulkInsertOrUpdate(entities);
            int dataInsertedResult2 = context.BulkInsertOrUpdate(employeeDataEntities);
            transaction2.Commit();
            logger.LogInformation($"Succesfully inserted {insertedResult2} employee records and {dataInsertedResult2} employee data records");
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

        private byte[] Compress(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            using MemoryStream msi = new MemoryStream(bytes);
            using MemoryStream mso = new MemoryStream();
            using (GZipStream gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }
            return mso.ToArray();
        }
    }
}
