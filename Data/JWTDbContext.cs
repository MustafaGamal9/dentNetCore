using JwtApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtApp.Data
{
    public class JWTDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public JWTDbContext(DbContextOptions<JWTDbContext> options) : base(options) { }


        public DbSet<Appointment> Appointments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Remove unique constraint on UserName to allow duplicate usernames
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.NormalizedUserName)
                              .HasDatabaseName("UserNameIndex")
                              .HasFilter("[NormalizedUserName] IS NOT NULL")
                              .IsUnique(false); 
            });
        }
    }
}