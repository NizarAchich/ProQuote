using Microsoft.EntityFrameworkCore;

namespace ProQuote.Database
{
    public class ProQuoteDbContext(DbContextOptions<ProQuoteDbContext> options) : DbContext(options)
    {

        // DbSet placeholders for future entities, for example:
        // public DbSet<AppUser> AppUsers { get; set; } = null!;
        // public DbSet<Organization> Organizations { get; set; } = null!;
    }

}
