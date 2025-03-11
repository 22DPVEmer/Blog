using Blog.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Blog.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Users
            var hasher = new PasswordHasher<User>();
            
            // Regular Users
            var user1 = new User
            {
                Id = "1",
                UserName = "user1@example.com",
                NormalizedUserName = "USER1@EXAMPLE.COM",
                Email = "user1@example.com",
                NormalizedEmail = "USER1@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Doe",
                CanWriteArticles = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user1.PasswordHash = hasher.HashPassword(user1, "Test123!");

            var user2 = new User
            {
                Id = "2",
                UserName = "user2@example.com",
                NormalizedUserName = "USER2@EXAMPLE.COM",
                Email = "user2@example.com",
                NormalizedEmail = "USER2@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith",
                CanWriteArticles = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user2.PasswordHash = hasher.HashPassword(user2, "Test123!");

            var user3 = new User
            {
                Id = "3",
                UserName = "user3@example.com",
                NormalizedUserName = "USER3@EXAMPLE.COM",
                Email = "user3@example.com",
                NormalizedEmail = "USER3@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Mike",
                LastName = "Johnson",
                CanWriteArticles = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user3.PasswordHash = hasher.HashPassword(user3, "Test123!");

            // Admin Users
            var admin1 = new User
            {
                Id = "4",
                UserName = "admin1@example.com",
                NormalizedUserName = "ADMIN1@EXAMPLE.COM",
                Email = "admin1@example.com",
                NormalizedEmail = "ADMIN1@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "One",
                IsAdmin = true,
                CanWriteArticles = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            admin1.PasswordHash = hasher.HashPassword(admin1, "Test123!");

            var admin2 = new User
            {
                Id = "5",
                UserName = "admin2@example.com",
                NormalizedUserName = "ADMIN2@EXAMPLE.COM",
                Email = "admin2@example.com",
                NormalizedEmail = "ADMIN2@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Two",
                IsAdmin = true,
                CanWriteArticles = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            admin2.PasswordHash = hasher.HashPassword(admin2, "Test123!");

            builder.Entity<User>().HasData(user1, user2, user3, admin1, admin2);

            // Article configuration
            builder.Entity<Article>(entity =>
            {
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Articles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}