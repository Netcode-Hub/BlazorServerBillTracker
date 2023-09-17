using BlazorServerBillTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerBillTracker.Data.DatabaseConfiguration
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Bill> Bills { get; set; } = default!;
        public DbSet<BillPeriod> BillPeriods { get; set; } = default!;
        public DbSet<BillHistory> BillsHistory { get; set; } = default!;
        public DbSet<DueBill> DueBills { get; set; } = default!;
    }
}
