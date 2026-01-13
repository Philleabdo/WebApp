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

        // Standardvärden
        modelBuilder.Entity<User>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<Profile>()
            .Property(p => p.ViewCount)
            .HasDefaultValue(0);

        // 1. Relation: User <-> Profile (One-to-One)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 2. Relation: Ägarskap (Vem skapade projektet?)
        // Detta fält används för att styra vem som får RADERA
        modelBuilder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Hindra radering av user om projekt finns

        // 3. Relation: VISNING (Many-to-Many) - VIKTIGAST!
        // Detta gör att många kan visa samma projekt på sitt CV
        modelBuilder.Entity<User>()
            .HasMany(u => u.Projects)
            .WithMany(p => p.UsersWhoDisplay)
            .UsingEntity(j => j.ToTable("UserProjectDisplay")); // Skapar kopplingstabellen

        // 4. Relation: Meddelanden
        modelBuilder.Entity<User>()
            .HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.ReceiverUser)
            .HasForeignKey(m => m.ReceiverUserId);
    }
}