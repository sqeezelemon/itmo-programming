using Microsoft.EntityFrameworkCore;
using Reports.Models;

namespace Reports.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<EmployeeAction> Actions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ReportStat> ReportStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Supervisor)
            .WithMany();
        modelBuilder.Entity<Employee>().HasKey(e => e.Login);

        modelBuilder.Entity<Message>().HasKey(m => m.Id);
        modelBuilder.Entity<EmployeeAction>().HasKey(a => a.Id);

        modelBuilder.Entity<Session>().HasKey(s => s.Token);
        modelBuilder.Entity<Account>().HasKey(a => a.Id);

        modelBuilder.Entity<Report>().HasKey(r => r.Id);
        modelBuilder.Entity<ReportStat>().HasKey(r => r.Id);

        base.OnModelCreating(modelBuilder);
    }
}