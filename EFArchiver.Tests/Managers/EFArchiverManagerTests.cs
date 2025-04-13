using EFArchiver;
using EFArchiver.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;

namespace EFArchiver.Tests.Managers
{
    public class EFArchiverManagerTests
    {
        public class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            [PartitionKey(ThresholdDays = 90)]
            public DateTime CreatedAt { get; set; }
        }
        // partitioned entity that inherits from Person
        // its decorated with [PartitionedEntity] to mark it as eligible for archiving.
        [PartitionedEntity]
        private class PartitionedPerson : Person { }

        /// <summary>
        /// test DbContext using SQLite in-memory
        /// </summary>
        private class TestDbContext : DbContext
        {
            public DbSet<Person> People => Set<Person>();

            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PartitionedPerson>().ToTable("People");
            }
        }

        /// <summary>
        /// helper to count the number of records in any table using raw SQL
        /// Used for the archive table which is not tracked with dbset
        /// </summary>
        private int GetTableCount(DbContext context, string tableName)
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// This test verifies that PartitionAllAsync correctly moves old records
        /// (based on the CreatedAt threshold) from the main table to the archive table
        /// </summary>
        [Fact]
        public async Task PartitionAllAsync_ShouldMoveOldRecordsToStorage()
        {

            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection) // PASSA l'oggetto connection aperto!
                .Options;

            // create tables and seed test data
            using (var context = new TestDbContext(options))
            {

                context.Database.EnsureCreated();

                // create the archive table "People_Storage" with the same schema
                string createArchiveTableSql = @"
                    CREATE TABLE IF NOT EXISTS [People_Storage] (
                        [Id] TEXT PRIMARY KEY,
                        [Name] TEXT NOT NULL,
                        [LastName] TEXT NOT NULL,
                        [CreatedAt] TEXT NOT NULL
                    );";

                context.Database.ExecuteSqlRaw(createArchiveTableSql);

                // this record should be archived
                context.Add(new PartitionedPerson
                {
                    Id = Guid.NewGuid(),
                    Name = "Mario",
                    LastName = "Rossi",
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                });
                // thus record should not be archived
                context.Add(new PartitionedPerson
                {
                    Id = Guid.NewGuid(),
                    Name = "Gianni",
                    LastName = "Bianchi",
                    CreatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // execute the archive logic
            using (var context = new TestDbContext(options))
            {
                var manager = new EFArchiverManager(context);
                await manager.PartitionAllAsync("Storage");
            }

            //ASSERT
            using (var context = new TestDbContext(options))
            {
                var remainingCount = context.People.Count();
                var archivedCount = GetTableCount(context, "People_Storage");

                Assert.Equal(1, remainingCount);
                Assert.Equal(1, archivedCount);
            }
        }
    }
}
