using grupp6WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Profile>().ToTable("Profile");
        modelBuilder.Entity<Project>().ToTable("Project");
        modelBuilder.Entity<Message>().ToTable("Message");

        // --- KONFIGURATION FÖR NYA FÄLT ---

        // Sätter IsActive till 1 (true) som standard i databasen
        modelBuilder.Entity<User>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Sätter ViewCount till 0 som standard i databasen
        modelBuilder.Entity<Profile>()
            .Property(p => p.ViewCount)
            .HasDefaultValue(0);

        // ----------------------------------

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Projects)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.ReceiverUser)
            .HasForeignKey(m => m.ReceiverUserId);
    }
}