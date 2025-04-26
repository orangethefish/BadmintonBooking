using Microsoft.EntityFrameworkCore;
using BadmintonBooking.API.Models;

namespace BadmintonBooking.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PricingConfiguration> PricingConfigurations { get; set; }
        public DbSet<BookingLock> BookingLocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Court>()
                .HasOne(c => c.Facility)
                .WithMany(f => f.Courts)
                .HasForeignKey(c => c.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Court)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PricingConfiguration>()
                .HasOne(p => p.Court)
                .WithMany(c => c.PricingConfigurations)
                .HasForeignKey(p => p.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingLock>()
                .HasOne(bl => bl.Court)
                .WithMany()
                .HasForeignKey(bl => bl.CourtId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 