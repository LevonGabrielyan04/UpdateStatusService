using Microsoft.EntityFrameworkCore;
using UpdateStatusService.Models;

namespace UpdateStatusService
{
    public class AppDbContext:DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=UpdateStatusDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
