using DotNetKafkaRabbitMQExample.Infrastructure.Persistence;
namespace DotNetKafkaRabbitMQExample.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotNetKafkaRabbitMQExample.Domain.Entities;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public new DbSet<User> Users { get; set; } = null!;
    public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;


}



