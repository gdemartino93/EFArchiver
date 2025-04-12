using EFArchiver;
using EFArchiver.TestApp.Data;
using EFArchiver.TestApp.Models;
using Microsoft.EntityFrameworkCore;

var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EFArchiverTestDb;Trusted_Connection=True;";
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new AppDbContext(options);

context.Database.ExecuteSqlRaw("""
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                   WHERE TABLE_NAME = 'People_Storage' AND TABLE_SCHEMA = 'dbo')
    BEGIN
        CREATE TABLE [People_Storage] (
            [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            [Name] NVARCHAR(MAX) NOT NULL,
            [LastName] NVARCHAR(MAX) NOT NULL,
            [Age] INT NOT NULL,
            [CreatedAt] DATETIME2 NOT NULL
        );
    END
""");

var archiver = new EntityArchiver<Person>(context);
await archiver.ArchiveAsync(p => p.CreatedAt < new DateTime(2025, 1, 1), "Storage");
