using System;
using System.Collections.Generic;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for a duplicate sponsor candidate pair.
    /// Used by /api/duplicates endpoint to display candidates and manage merge operations.
    /// </summary>
    public class DuplicateSponsorCandidateDto
    {
        public string CandidatePairId { get; set; } = string.Empty;

        /// <summary>
        /// First sponsor in the pair (primary record, typically kept during merge).
        /// </summary>
        public SponsorSummaryDto PrimarySponsor { get; set; } = new();

        /// <summary>
        /// Second sponsor in the pair (potential duplicate, typically merged into primary).
        /// </summary>
        public SponsorSummaryDto DuplicateSponsor { get; set; } = new();

        /// <summary>
        /// Matching confidence score (0-100).
        /// Higher score = stronger likelihood of true duplicate.
        /// Based on name similarity, TIN match, address overlap, etc.
        /// </summary>
        public decimal MatchScore { get; set; }

        /// <summary>
        /// List of matching criteria that triggered this candidate pair.
        /// Examples: "SameTIN", "SimilarName", "SameAddress", "CrosSystemMatch".
        /// </summary>
        public List<string> MatchReasons { get; set; } = new();

        /// <summary>
        /// Current status: Pending, Approved, Merged, Rejected, Archived.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Who reviewed this candidate pair.
        /// </summary>
        public string? ReviewedByUserId { get; set; }

        /// <summary>
        /// Display name/email of reviewer.
        /// </summary>
        public string? ReviewedByUserName { get; set; }

        /// <summary>
        /// When the pair was reviewed/actioned (UTC).
        /// </summary>
        public DateTime? ReviewedOn { get; set; }

        /// <summary>
        /// Reviewer's notes on the decision.
        /// </summary>
        public string? ReviewNotes { get; set; }

        /// <summary>
        /// When the merge was executed, if applicable (UTC).
        /// </summary>
        public DateTime? MergedOn { get; set; }

        /// <summary>
        /// Who executed the merge, if applicable.
        /// </summary>
        public string? MergedByUserId { get; set; }
    }

    /// <summary>
    /// DTO for summary info about a sponsor (used in duplicate candidate pair).
    /// </summary>
    public class SponsorSummaryDto
    {
        public string SponsorId { get; set; } = string.Empty;

        public string SponsorName { get; set; } = string.Empty;

        public string? LegalName { get; set; }

        public string? TIN { get; set; }

        /// <summary>
        /// Address for comparison purposes.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// City, State, Zip concatenated.
        /// </summary>
        public string? CityStateZip { get; set; }

        /// <summary>
        /// Primary contact email.
        /// </summary>
        public string? PrimaryEmail { get; set; }

        /// <summary>
        /// Cross-system IDs (PowerSchool, NetSuite, etc.).
        /// </summary>
        public Dictionary<string, string>? CrossSystemIds { get; set; }

        /// <summary>
        /// Active status.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of students associated with this sponsor.
        /// </summary>
        public int StudentCount { get; set; }

        /// <summary>
        /// Last time this sponsor record was modified.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }
    }
}
