using Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Business
{
    public class AppDBContext : DbContext
    {

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Models.User> User { get; set; }
        public DbSet<Models.Books> Book { get; set; }
        public DbSet<Models.BorrowRecords> BorrowRecord { get; set; }
        public DbSet<Models.Sessions> Session { get; set; }
        public DbSet<Models.Category> Category { get; set; }
        public DbSet<Models.NotificationTemplate> NotificationTemplate { get; set; }
        public DbSet<Models.NotificationLog> NotificationLog { get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        var builder = new ConfigurationBuilder()
        //            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),"../FUView"))
        //            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //        IConfigurationRoot configuration = builder.Build();
        //        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Book configuration
            modelBuilder.Entity<Books>()
                .HasIndex(b => b.ISBN)
                .IsUnique();
            
            modelBuilder.Entity<Books>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category configuration
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // NotificationTemplate configuration
            modelBuilder.Entity<NotificationTemplate>()
                .HasIndex(nt => new { nt.Name, nt.Type })
                .IsUnique();

            // NotificationLog configuration
            modelBuilder.Entity<NotificationLog>()
                .HasOne(nl => nl.User)
                .WithMany()
                .HasForeignKey(nl => nl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationLog>()
                .HasOne(nl => nl.NotificationTemplate)
                .WithMany()
                .HasForeignKey(nl => nl.NotificationTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationLog>()
                .HasOne(nl => nl.BorrowRecord)
                .WithMany()
                .HasForeignKey(nl => nl.BorrowRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationLog>()
                .HasIndex(nl => new { nl.UserId, nl.Type, nl.CreatedAt });
        }
    }
}
