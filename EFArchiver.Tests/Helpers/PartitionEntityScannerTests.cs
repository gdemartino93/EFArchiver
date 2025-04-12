using EFArchiver.Attributes;
using EFArchiver.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EFArchiver.Tests.Helpers
{
    public class PartitionEntityScannerTests
    {
        private class MyDbContext : DbContext
        {
            public DbSet<Person> People => Set<Person>();
            public DbSet<Animal> Animals => Set<Animal>();
            public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>().ToTable("People");
                modelBuilder.Entity<Animal>().ToTable("Animals");
            }

        }

        [PartitionedEntity]
        private class Person
        {
            public Guid Id { get; set; }

            [PartitionKey(ThresholdDays = 30)]
            public DateTime CreatedAt { get; set; }
        }

        private class Animal
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
        }

        [Fact]
        public void GetPartitionedEntities_ShouldReturnOnly_Persons()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase("PartitionedDb").Options;

            using var context = new MyDbContext(options);

            //Act
            var partitionedEntities = PartitionEntityScanner.GetPartitionedEntities(context);

            //Assert
            var types = partitionedEntities.Select(e => e.ClrType).ToList();
            Assert.Single(types);
            Assert.Contains(typeof(Person), types);
            Assert.DoesNotContain(typeof(Animal), types);
        }

    }
}
