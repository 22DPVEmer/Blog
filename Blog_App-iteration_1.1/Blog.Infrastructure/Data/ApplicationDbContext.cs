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
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<PermissionRequest> PermissionRequests { get; set; }

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

            // Regular Users without permissions
            var regularUser1 = new User
            {
                Id = "6",
                UserName = "regular1@example.com",
                NormalizedUserName = "REGULAR1@EXAMPLE.COM",
                Email = "regular1@example.com",
                NormalizedEmail = "REGULAR1@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "One",
                CanWriteArticles = false,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            regularUser1.PasswordHash = hasher.HashPassword(regularUser1, "Test123!");

            var regularUser2 = new User
            {
                Id = "7",
                UserName = "regular2@example.com",
                NormalizedUserName = "REGULAR2@EXAMPLE.COM",
                Email = "regular2@example.com",
                NormalizedEmail = "REGULAR2@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "Two",
                CanWriteArticles = false,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            regularUser2.PasswordHash = hasher.HashPassword(regularUser2, "Test123!");

            var regularUser3 = new User
            {
                Id = "8",
                UserName = "regular3@example.com",
                NormalizedUserName = "REGULAR3@EXAMPLE.COM",
                Email = "regular3@example.com",
                NormalizedEmail = "REGULAR3@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "Three",
                CanWriteArticles = false,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            regularUser3.PasswordHash = hasher.HashPassword(regularUser3, "Test123!");

            var regularUser4 = new User
            {
                Id = "9",
                UserName = "regular4@example.com",
                NormalizedUserName = "REGULAR4@EXAMPLE.COM",
                Email = "regular4@example.com",
                NormalizedEmail = "REGULAR4@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "Four",
                CanWriteArticles = false,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            regularUser4.PasswordHash = hasher.HashPassword(regularUser4, "Test123!");

            var regularUser5 = new User
            {
                Id = "10",
                UserName = "regular5@example.com",
                NormalizedUserName = "REGULAR5@EXAMPLE.COM",
                Email = "regular5@example.com",
                NormalizedEmail = "REGULAR5@EXAMPLE.COM",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "Five",
                CanWriteArticles = false,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            regularUser5.PasswordHash = hasher.HashPassword(regularUser5, "Test123!");

            builder.Entity<User>().HasData(user1, user2, user3, admin1, admin2, 
                regularUser1, regularUser2, regularUser3, regularUser4, regularUser5);

            // Seed Articles
            var articles = new[]
            {
                new Article
                {
                    Id = 1,
                    Title = "Getting Started with ASP.NET Core",
                    Intro = "An introduction to building web applications with ASP.NET Core",
                    Content = "This is a basic introduction to ASP.NET Core...",
                    UserId = user1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    PublishedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Article
                {
                    Id = 2,
                    Title = "Understanding Entity Framework Core",
                    Intro = "Learn the basics of Entity Framework Core and data access",
                    Content = "Entity Framework Core is an object-relational mapper...",
                    UserId = user2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    PublishedAt = DateTime.UtcNow.AddDays(-9)
                },
                new Article
                {
                    Id = 3,
                    Title = "Identity Management in ASP.NET Core",
                    Intro = "Implementing authentication and authorization",
                    Content = "Security is a crucial aspect of web applications...",
                    UserId = user3.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    PublishedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Article
                {
                    Id = 4,
                    Title = "Best Practices for Web API Design",
                    Intro = "Learn how to design clean and efficient Web APIs",
                    Content = "When designing Web APIs, there are several principles...",
                    UserId = admin1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    PublishedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Article
                {
                    Id = 5,
                    Title = "Dependency Injection in .NET",
                    Intro = "Understanding dependency injection and its implementation",
                    Content = "Dependency injection is a software design pattern...",
                    UserId = admin2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    PublishedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Article
                {
                    Id = 6,
                    Title = "Working with Azure Cloud Services",
                    Intro = "Introduction to Microsoft Azure cloud platform",
                    Content = "Cloud computing has revolutionized modern applications...",
                    UserId = user1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    PublishedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Article
                {
                    Id = 7,
                    Title = "Microservices Architecture",
                    Intro = "Building scalable applications with microservices",
                    Content = "Microservices architecture is an approach to building applications...",
                    UserId = user2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    PublishedAt = DateTime.UtcNow.AddDays(-4)
                },
                new Article
                {
                    Id = 8,
                    Title = "Unit Testing Best Practices",
                    Intro = "Writing effective unit tests for your applications",
                    Content = "Unit testing is a fundamental practice in software development...",
                    UserId = user3.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    PublishedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Article
                {
                    Id = 9,
                    Title = "DevOps Fundamentals",
                    Intro = "Understanding DevOps practices and principles",
                    Content = "DevOps is a set of practices that combines software development...",
                    UserId = admin1.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    PublishedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Article
                {
                    Id = 10,
                    Title = "Securing Web Applications",
                    Intro = "Essential security practices for web applications",
                    Content = "Security should be a top priority in web development...",
                    UserId = admin2.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    PublishedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            builder.Entity<Article>().HasData(articles);

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

            // Configure PermissionRequest relationships
            builder.Entity<PermissionRequest>()
                .HasOne(pr => pr.User)
                .WithMany()
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PermissionRequest>()
                .HasOne(pr => pr.ProcessedByUser)
                .WithMany()
                .HasForeignKey(pr => pr.ProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}