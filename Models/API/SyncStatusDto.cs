using System;
using System.Collections.Generic;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for querying synchronization status of a decision across downstream systems.
    /// Returned by sync status endpoints with CorrelationId as query key.
    /// </summary>
    public class SyncStatusDto
    {
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// List of downstream system sync statuses (PowerSchool, NetSuite, OBS, SCP).
        /// </summary>
        public List<TargetSystemStatusDto> TargetSystems { get; set; } = new();

        /// <summary>
        /// Overall status: Pending, InProgress, Succeeded, PartialSuccess, Failed.
        /// PartialSuccess = some systems synced, others failed.
        /// </summary>
        public string OverallStatus { get; set; } = "Pending";

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for a single downstream system's sync status.
    /// </summary>
    public class TargetSystemStatusDto
    {
        /// <summary>
        /// Target system name: PowerSchool, StudentChargingPortal, NetSuite, OnlineBillingSystem.
        /// </summary>
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// Sync status: Pending, InProgress, Succeeded, Failed, Skipped.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// When the sync was last attempted (UTC).
        /// </summary>
        public DateTime? LastAttemptedAt { get; set; }

        /// <summary>
        /// When the sync last succeeded, if applicable (UTC).
        /// </summary>
        public DateTime? LastSucceededAt { get; set; }

        /// <summary>
        /// Number of retry attempts made.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Error message if sync failed, nullable.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// External reference ID from target system (e.g., NetSuite transaction ID).
        /// </summary>
        public string? ExternalReferenceId { get; set; }
    }
}
