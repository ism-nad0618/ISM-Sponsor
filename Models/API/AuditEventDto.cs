using System;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for audit trail events.
    /// Used by /api/audit/events endpoint to query activity log across coverage decisions, 
    /// sponsor changes, and system operations.
    /// </summary>
    public class AuditEventDto
    {
        public string AuditEventId { get; set; } = string.Empty;

        /// <summary>
        /// Correlation ID for end-to-end traceability (coverage decision, change request, etc.).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Entity type: Sponsor, Student, LoG, CoverageDecision, ChangeRequest, Sync, etc.
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Primary key of the entity being acted upon.
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Action performed: Create, Update, Delete, Approve, Reject, Sync, etc.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// User who performed the action.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name or email of user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// When the action occurred (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// JSON snapshot of field changes (old values, new values).
        /// Populated for Update/Approve/Reject actions.
        /// </summary>
        public string? DataSnapshot { get; set; }

        /// <summary>
        /// Affected resource details (e.g., Sponsor name, Student ID, Rule version).
        /// </summary>
        public string? ResourceDescription { get; set; }

        /// <summary>
        /// IP address or integration source of the action (for security audit).
        /// </summary>
        public string? SourceIp { get; set; }

        /// <summary>
        /// Integration/system source if action originated from external system.
        /// E.g., "PowerSchoolSync", "SCP", "Manual", "ScheduledJob".
        /// </summary>
        public string? SourceSystem { get; set; }

        /// <summary>
        /// Success flag for tracking failed vs. succeeded actions.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Error message if action failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
