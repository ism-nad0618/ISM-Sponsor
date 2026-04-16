namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Response DTO for PowerSchool student-sponsor synchronization.
    /// </summary>
    public class StudentSponsorSyncResponse
    {
        /// <summary>
        /// Correlation ID for tracking this sync operation.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Overall success status.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Total number of mappings in the request.
        /// </summary>
        public int TotalMappings { get; set; }

        /// <summary>
        /// Number of mappings successfully processed.
        /// </summary>
        public int SuccessfulMappings { get; set; }

        /// <summary>
        /// Number of mappings that failed processing.
        /// </summary>
        public int FailedMappings { get; set; }

        /// <summary>
        /// Detailed results for each mapping.
        /// </summary>
        public List<MappingSyncResult> Results { get; set; } = new();

        /// <summary>
        /// Overall sync message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when sync completed (UTC).
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result for a single student-sponsor mapping sync.
    /// </summary>
    public class MappingSyncResult
    {
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public int? LogId { get; set; }
    }
}
