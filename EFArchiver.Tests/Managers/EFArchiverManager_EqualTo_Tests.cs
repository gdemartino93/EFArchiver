using EFArchiver;
using EFArchiver.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;

namespace EFArchiver.Tests.Managers
{
    public class EFArchiverManager_EqualTo_Tests
    {
        [PartitionedEntity]
        private class Invoice
        {
            public Guid Id { get; set; }
            public string Description { get; set; }

            // Archive if Status == 5
            [PartitionKey(EqualTo = 5)]
            public int Status { get; set; }
        }

        private class InvoiceDbContext : DbContext
        {
            public DbSet<Invoice> Invoices => Set<Invoice>();

            public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Invoice>().ToTable("Invoices");
            }
        }

        private int GetTableCount(DbContext context, string tableName)
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        [Fact]
        public async Task PartitionAllAsync_ShouldArchiveInvoicesWithStatus5()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<InvoiceDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new InvoiceDbContext(options))
            {
                context.Database.EnsureCreated();

                // Create storage table manually
                string createArchiveTableSql = @"
                    CREATE TABLE IF NOT EXISTS [Invoices_Storage] (
                        [Id] TEXT PRIMARY KEY,
                        [Description] TEXT NOT NULL,
                        [Status] INTEGER NOT NULL
                    );";
                context.Database.ExecuteSqlRaw(createArchiveTableSql);

                // Insert one archivable and one non-archivable invoice
                context.Add(new Invoice
                {
                    Id = Guid.NewGuid(),
                    Description = "Archived Invoice",
                    Status = 5
                });
                context.Add(new Invoice
                {
                    Id = Guid.NewGuid(),
                    Description = "Active Invoice",
                    Status = 1
                });
                context.SaveChanges();
            }

            using (var context = new InvoiceDbContext(options))
            {
                var manager = new EFArchiverManager(context);
                await manager.PartitionAllAsync("Storage");
            }

            using (var context = new InvoiceDbContext(options))
            {
                int remaining = context.Invoices.Count();
                int archived = GetTableCount(context, "Invoices_Storage");

                Assert.Equal(1, remaining);
                Assert.Equal(1, archived);
            }
        }
    }
}
