namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Request payload to post a persisted coverage decision to a downstream system.
    /// </summary>
    public class CoverageDecisionSyncRequestDto
    {
        /// <summary>
        /// Persisted audit decision ID from /api/coverage/evaluate or /api/coverage/commit.
        /// </summary>
        public int AuditId { get; set; }
    }
}
