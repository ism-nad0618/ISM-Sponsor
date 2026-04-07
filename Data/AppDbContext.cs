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
        public DbSet<SponsorAddress> SponsorAddresses { get; set; }
        public DbSet<SponsorContact> SponsorContacts { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<LogCoverage> LogCoverages { get; set; }
        public DbSet<LoGCoverageRule> LoGCoverageRules { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<CoverageEvaluationAudit> CoverageEvaluationAudits { get; set; }
        public DbSet<SponsorChangeRequest> SponsorChangeRequests { get; set; }
        
        // Step 6: Duplicate detection, merge, and sync tracking
        public DbSet<SponsorDuplicateCandidate> SponsorDuplicateCandidates { get; set; }
        public DbSet<MergeOperation> MergeOperations { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }
        
        // Step 8: User feedback for continuous improvement
        public DbSet<UserFeedback> UserFeedback { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SchoolYear>().HasKey(s => s.SchoolYearId);
            
            builder.Entity<Sponsor>().HasKey(s => s.SponsorId);
            builder.Entity<Sponsor>()
                .HasIndex(s => s.SponsorName);
            builder.Entity<Sponsor>()
                .HasIndex(s => s.Tin);

            builder.Entity<SponsorAddress>()
                .HasKey(sa => sa.SponsorAddressId);
            builder.Entity<SponsorAddress>()
                .HasOne(sa => sa.Sponsor)
                .WithMany(s => s.Addresses)
                .HasForeignKey(sa => sa.SponsorId);

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
                .HasKey(l => l.LogId);
            builder.Entity<LogCoverage>()
                .HasIndex(l => new { l.SchoolYearId, l.StudentId })
                .IsUnique();
            builder.Entity<LogCoverage>()
                .HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => new { l.SchoolYearId, l.StudentId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.Sponsor)
                .WithMany(s => s.LettersOfGuarantee)
                .HasForeignKey(l => l.SponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.ActivatedByUser)
                .WithMany()
                .HasForeignKey(l => l.ActivatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.DeactivatedByUser)
                .WithMany()
                .HasForeignKey(l => l.DeactivatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.CreatedByUser)
                .WithMany()
                .HasForeignKey(l => l.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.Entity<LogCoverage>()
                .HasOne(l => l.ModifiedByUser)
                .WithMany()
                .HasForeignKey(l => l.ModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.Sponsor)
                .WithMany(s => s.ChangeRequests)
                .HasForeignKey(cr => cr.SponsorId);
            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.RequestedByUser)
                .WithMany()
                .HasForeignKey(cr => cr.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ChangeRequest>()
                .HasOne(cr => cr.ResolvedByUser)
                .WithMany()
                .HasForeignKey(cr => cr.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>().HasKey(i => i.ItemId);
            builder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemCategory>().HasKey(ic => ic.CategoryId);

            builder.Entity<LoGCoverageRule>()
                .HasKey(r => r.RuleId);
            builder.Entity<LoGCoverageRule>()
                .HasOne(r => r.LetterOfGuarantee)
                .WithMany(l => l.CoverageRules)
                .HasForeignKey(r => r.LogId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<LoGCoverageRule>()
                .HasOne(r => r.Item)
                .WithMany()
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LoGCoverageRule>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LoGCoverageRule>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.Entity<LoGCoverageRule>()
                .HasOne(r => r.ModifiedByUser)
                .WithMany()
                .HasForeignKey(r => r.ModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            builder.Entity<LoGCoverageRule>()
                .Property(r => r.CoveragePercentage)
                .HasPrecision(18, 2);
            builder.Entity<LoGCoverageRule>()
                .Property(r => r.CoverageFixedAmount)
                .HasPrecision(18, 2);
            builder.Entity<LoGCoverageRule>()
                .Property(r => r.CapAmount)
                .HasPrecision(18, 2);

            builder.Entity<ActivityLog>().HasKey(a => a.ActivityLogId);

            builder.Entity<UserPreference>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserPreference>(up => up.UserId);

            builder.Entity<CoverageEvaluationAudit>()
                .HasKey(a => a.AuditId);
            builder.Entity<CoverageEvaluationAudit>()
                .HasOne(a => a.EvaluatedByUser)
                .WithMany()
                .HasForeignKey(a => a.EvaluatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CoverageEvaluationAudit>()
                .HasOne(a => a.LetterOfGuarantee)
                .WithMany()
                .HasForeignKey(a => a.LogId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CoverageEvaluationAudit>()
                .HasOne(a => a.MatchedRule)
                .WithMany()
                .HasForeignKey(a => a.MatchedRuleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<CoverageEvaluationAudit>()
                .Property(a => a.RequestedAmount)
                .HasPrecision(18, 2);
            builder.Entity<CoverageEvaluationAudit>()
                .Property(a => a.SponsorAmount)
                .HasPrecision(18, 2);
            builder.Entity<CoverageEvaluationAudit>()
                .Property(a => a.ParentAmount)
                .HasPrecision(18, 2);

            builder.Entity<SponsorChangeRequest>()
                .HasKey(r => r.RequestId);
            builder.Entity<SponsorChangeRequest>()
                .HasOne(r => r.Sponsor)
                .WithMany()
                .HasForeignKey(r => r.SponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorChangeRequest>()
                .HasOne(r => r.SubmittedByUser)
                .WithMany()
                .HasForeignKey(r => r.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorChangeRequest>()
                .HasOne(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorChangeRequest>()
                .HasOne(r => r.AppliedByUser)
                .WithMany()
                .HasForeignKey(r => r.AppliedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorChangeRequest>()
                .HasIndex(r => new { r.SponsorId, r.Status });
            builder.Entity<SponsorChangeRequest>()
                .HasIndex(r => r.SubmittedOn);

            // Step 6: Duplicate detection and merge configuration
            builder.Entity<SponsorDuplicateCandidate>()
                .HasKey(c => c.CandidateId);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasOne(c => c.PrimarySponsor)
                .WithMany()
                .HasForeignKey(c => c.PrimarySponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasOne(c => c.DuplicateSponsor)
                .WithMany()
                .HasForeignKey(c => c.DuplicateSponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasOne(c => c.ReviewedByUser)
                .WithMany()
                .HasForeignKey(c => c.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasOne(c => c.MergeOperation)
                .WithMany(m => m.RelatedCandidates)
                .HasForeignKey(c => c.MergeOperationId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<SponsorDuplicateCandidate>()
                .Property(c => c.MatchScore)
                .HasPrecision(5, 2); // 0.00 to 100.00
            builder.Entity<SponsorDuplicateCandidate>()
                .HasIndex(c => c.Status);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasIndex(c => c.MatchScore);
            builder.Entity<SponsorDuplicateCandidate>()
                .HasIndex(c => new { c.PrimarySponsorId, c.DuplicateSponsorId });

            builder.Entity<MergeOperation>()
                .HasKey(m => m.MergeOperationId);
            builder.Entity<MergeOperation>()
                .HasOne(m => m.SurvivingSponsor)
                .WithMany()
                .HasForeignKey(m => m.SurvivingSponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MergeOperation>()
                .HasOne(m => m.MergedSponsor)
                .WithMany()
                .HasForeignKey(m => m.MergedSponsorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MergeOperation>()
                .HasOne(m => m.InitiatedByUser)
                .WithMany()
                .HasForeignKey(m => m.InitiatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MergeOperation>()
                .HasIndex(m => m.Status);
            builder.Entity<MergeOperation>()
                .HasIndex(m => m.InitiatedOn);
            builder.Entity<MergeOperation>()
                .HasIndex(m => m.SurvivingSponsorId);
            builder.Entity<MergeOperation>()
                .HasIndex(m => m.MergedSponsorId);

            builder.Entity<SyncLog>()
                .HasKey(s => s.SyncLogId);
            builder.Entity<SyncLog>()
                .HasIndex(s => new { s.EntityType, s.EntityId });
            builder.Entity<SyncLog>()
                .HasIndex(s => s.TargetSystem);
            builder.Entity<SyncLog>()
                .HasIndex(s => s.Status);
            builder.Entity<SyncLog>()
                .HasIndex(s => s.CorrelationId);
            builder.Entity<SyncLog>()
                .HasIndex(s => s.AttemptedAt);
        }
    }
}