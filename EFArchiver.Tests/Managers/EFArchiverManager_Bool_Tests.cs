using EFArchiver;
using EFArchiver.Attributes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;

namespace EFArchiver.Tests.Managers
{
    public class EFArchiverManager_Bool_Tests
    {
        [PartitionedEntity]
        private class Message
        {
            public Guid Id { get; set; }
            public string Content { get; set; }

            [PartitionKey(EqualTo = true)]
            public bool IsArchived { get; set; }
        }

        private class MessageDbContext : DbContext
        {
            public DbSet<Message> Messages => Set<Message>();

            public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Message>().ToTable("Messages");
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
        public async Task PartitionAllAsync_ShouldArchiveMessagesWithIsArchivedTrue()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<MessageDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var context = new MessageDbContext(options))
            {
                context.Database.EnsureCreated();

                // Create archive table manually
                string createArchiveTableSql = @"
                    CREATE TABLE IF NOT EXISTS [Messages_Storage] (
                        [Id] TEXT PRIMARY KEY,
                        [Content] TEXT NOT NULL,
                        [IsArchived] INTEGER NOT NULL
                    );";
                context.Database.ExecuteSqlRaw(createArchiveTableSql);

                // One archived (true), one active (false)
                context.Add(new Message
                {
                    Id = Guid.NewGuid(),
                    Content = "Archived message",
                    IsArchived = true
                });
                context.Add(new Message
                {
                    Id = Guid.NewGuid(),
                    Content = "Active message",
                    IsArchived = false
                });
                context.SaveChanges();
            }

            using (var context = new MessageDbContext(options))
            {
                var manager = new EFArchiverManager(context);
                await manager.PartitionAllAsync("Storage");
            }

            using (var context = new MessageDbContext(options))
            {
                int remaining = context.Messages.Count();
                int archived = GetTableCount(context, "Messages_Storage");

                Assert.Equal(1, remaining);
                Assert.Equal(1, archived);
            }
        }
    }
}
