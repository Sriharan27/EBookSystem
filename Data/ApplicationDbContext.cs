using EBookSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EBookSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Books> Books { get; set; }
        public DbSet<Genres> Genres { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderLineItems> OrderLineItems { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
