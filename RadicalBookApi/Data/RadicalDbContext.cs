using Microsoft.EntityFrameworkCore;
using RadicalBookApi.Models;
using System.Collections.Generic;

namespace RadicalBookApi.Data
{
    public class RadicalDbContext : DbContext
    {
        public RadicalDbContext(DbContextOptions<RadicalDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
    }
}
