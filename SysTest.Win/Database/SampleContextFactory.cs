using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows.Markup;

namespace SysTest.Win.Database
{
    public class SampleContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            var connectionString ="Data Source=.\\Test.db";

            optionsBuilder.UseSqlite(connectionString,
                                        opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}
