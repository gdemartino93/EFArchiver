using EFArchiver.TestApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver.TestApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> People => Set<Person>();
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>().ToTable("People");
        }
    }
}
