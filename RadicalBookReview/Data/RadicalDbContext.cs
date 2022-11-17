using Microsoft.EntityFrameworkCore;
using RadicalBookReview.Models;

namespace RadicalBookReview.Data
{
    public class RadicalDbContext:DbContext
    {
        public RadicalDbContext(DbContextOptions<RadicalDbContext> options):base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
    }
}
