using EFArchiver.WebTestApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EFArchiver.WebTestApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> People => Set<Person>();
        public DbSet<Profile> Profiles => Set<Profile>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>().ToTable("People");

            modelBuilder.Entity<Profile>()
                .ToTable("Profiles");

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Profile)
                .WithOne(p => p.Person)
                .HasForeignKey<Profile>(pr => pr.PersonId);
                
        }
    }
}
