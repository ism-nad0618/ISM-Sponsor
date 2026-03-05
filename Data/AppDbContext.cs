using ISMSponsor.Models;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<SponsorContact> SponsorContacts { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<LogCoverage> LogCoverages { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SchoolYear>().HasKey(s => s.SchoolYearId);
            builder.Entity<Sponsor>().HasKey(s => s.SponsorId);
            builder.Entity<SponsorContact>()
                .HasOne(c => c.Sponsor)
                .WithMany(s => s.Contacts)
                .HasForeignKey(c => c.SponsorId);
            builder.Entity<Student>()
                .HasKey(s => new { s.SchoolYearId, s.StudentId });
            builder.Entity<Student>()
                .HasOne(s => s.SchoolYear)
                .WithMany(y => y.Students)
                .HasForeignKey(s => s.SchoolYearId);
            builder.Entity<Student>()
                .HasOne(s => s.Sponsor)
                .WithMany(sp => sp.Students)
                .HasForeignKey(s => s.SponsorId);

            builder.Entity<LogCoverage>()
                .HasKey(l => new { l.SchoolYearId, l.StudentId });
            builder.Entity<LogCoverage>()
                .HasOne(l => l.Student)
                .WithOne()
                .HasForeignKey<LogCoverage>(l => new { l.SchoolYearId, l.StudentId })
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.Sponsor)
                .WithMany()
                .HasForeignKey(l => l.SponsorId)
                .OnDelete(DeleteBehavior.NoAction);
            // TODO: enforce SponsorId matches Student.SponsorId via a trigger or manual check in service

            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.Sponsor)
                .WithMany(s => s.ChangeRequests)
                .HasForeignKey(cr => cr.SponsorId);
            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.RequestedByUser)
                .WithMany()
                .HasForeignKey(cr => cr.RequestedByUserId).
                OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.ResolvedByUser)
                .WithMany()
                .HasForeignKey(cr => cr.ResolvedByUserId).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>().HasKey(i => i.ItemId);

            builder.Entity<ActivityLog>().HasKey(a => a.ActivityLogId);

            builder.Entity<UserPreference>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserPreference>(up => up.UserId);
        }
    }
}