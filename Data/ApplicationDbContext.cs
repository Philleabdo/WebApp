using grupp6WebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<User> User { get; set; }
}
