#region Using derectives

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Configuration;
using SysTest.Win.Database.Entity;

#endregion

namespace SysTest.Win.Database
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
                : base(options) {
            Database.EnsureCreated(); }

        public ApplicationContext() { 
            Database.EnsureCreated();}

        public DbSet<Port> Ports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //var connectionStringBuilder = new SqliteConnectionStringBuilder {DataSource = "Test.db"};
            //var connectionString = connectionStringBuilder.ToString();
            //var connection = new SqliteConnection(connectionString);

            options.UseSqlite("Data Source=.\\Test.db");
        }
    }
}