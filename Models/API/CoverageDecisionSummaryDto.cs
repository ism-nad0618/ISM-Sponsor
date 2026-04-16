using System;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for coverage decision statistics and reporting.
    /// Used by admin dashboard endpoints to display decision trends, coverage rates, etc.
    /// </summary>
    public class CoverageDecisionSummaryDto
    {
        public string StudentId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string SchoolYearId { get; set; } = string.Empty;

        public string SchoolYearName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of coverage decisions evaluated for this student.
        /// </summary>
        public int TotalDecisionsCount { get; set; }

        /// <summary>
        /// Number of COVERED decisions (sponsor responsible for 100%).
        /// </summary>
        public int CoveredCount { get; set; }

        /// <summary>
        /// Number of SPLIT decisions (shared sponsor/parent responsibility).
        /// </summary>
        public int SplitCount { get; set; }

        /// <summary>
        /// Number of NOT_COVERED decisions (parent responsible for 100%).
        /// </summary>
        public int NotCoveredCount { get; set; }

        /// <summary>
        /// Total amount evaluated (sum of requested amounts).
        /// </summary>
        public decimal TotalAmountEvaluated { get; set; }

        /// <summary>
        /// Total amount approved to sponsor.
        /// </summary>
        public decimal TotalSponsorAmount { get; set; }

        /// <summary>
        /// Total amount approved to parent.
        /// </summary>
        public decimal TotalParentAmount { get; set; }

        /// <summary>
        /// Percentage of evaluated amount covered by sponsor (SponsorAmount / Total).
        /// </summary>
        public decimal SponsorCoveragePercentage { get; set; }

        /// <summary>
        /// Percentage of evaluated amount covered by parent (ParentAmount / Total).
        /// </summary>
        public decimal ParentCoveragePercentage { get; set; }

        /// <summary>
        /// Date of the first coverage decision for this student/year.
        /// </summary>
        public DateTime? FirstDecisionDate { get; set; }

        /// <summary>
        /// Date of the most recent coverage decision for this student/year.
        /// </summary>
        public DateTime? LastDecisionDate { get; set; }

        /// <summary>
        /// Primary letter of guarantee version used (e.g., "2024-01").
        /// </summary>
        public string? PrimaryLoGVersion { get; set; }

        /// <summary>
        /// Count of decisions that required manual override or exception handling.
        /// </summary>
        public int ExceptionCount { get; set; }
    }

    /// <summary>
    /// DTO for coverage decision aggregated reporting by reason code.
    /// Shows decision distribution by reason code for analytics and audit purposes.
    /// </summary>
    public class CoverageReasonSummaryDto
    {
        /// <summary>
        /// Reason code (e.g., "EthnicMinority", "FirstGeneration", "IncomeLevel", etc.).
        /// </summary>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable reason description.
        /// </summary>
        public string ReasonDescription { get; set; } = string.Empty;

        /// <summary>
        /// Number of decisions justified by this reason code.
        /// </summary>
        public int DecisionCount { get; set; }

        /// <summary>
        /// Average sponsor coverage percentage for decisions with this reason.
        /// </summary>
        public decimal AverageSponsorCoveragePercent { get; set; }

        /// <summary>
        /// Total amount approved to sponsor for this reason code.
        /// </summary>
        public decimal TotalSponsorAmount { get; set; }

        /// <summary>
        /// Most recent decision using this reason code.
        /// </summary>
        public DateTime? LastUsedAt { get; set; }
    }
}
