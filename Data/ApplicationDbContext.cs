using Microsoft.EntityFrameworkCore;
using System;

namespace AQUA.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        protected ApplicationDbContext()
        {
        }
        public DbSet<UserBase> Users { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<LevelChange> LevelChanges { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserBase>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<Student>("Student")
                .HasValue<Coach>("Coach")
                .HasValue<ServerAdmin>("ServerAdmin")
                .HasValue<Manager>("Manager");

            modelBuilder.Entity<UserBase>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<ServerAdmin>()
                .Property(sa => sa.AccessLevel)
                .HasColumnName("AccessLevel")
                .IsRequired();

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Schedule)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Coach)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice relationships
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany() // Or add ICollection<Invoice> to UserBase if needed
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // LevelChange relationships
            modelBuilder.Entity<LevelChange>()
                .HasOne(lc => lc.Student)
                .WithMany()
                .HasForeignKey(lc => lc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LevelChange>()
                .HasOne(lc => lc.Coach)
                .WithMany()
                .HasForeignKey(lc => lc.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            SeedManager(modelBuilder);
        }

        private static readonly DateTime SeedDate = new DateTime(2025, 12, 27, 14, 0, 0);

        private void SeedManager(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manager>().HasData(
                new Manager
                {
                    UserId = "M0001",
                    FullName = "System Manager",
                    Email = "manager@aqua.system",
                    Phone = "0000000000",
                    Password = "MANAGERPASS123",
                    Role = "Manager",
                    AccessLevel = 999,
                    IsActive = true,
                    RegistrationDate = SeedDate
                }
            );
        }
    }
}