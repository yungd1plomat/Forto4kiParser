using Forto4kiParser.Models;
using Microsoft.EntityFrameworkCore;

namespace Forto4kiParser.Data
{
    public class AppDb : DbContext
    {
        public DbSet<Filter> Filters { get; set; }

        public AppDb(DbContextOptions<AppDb> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
