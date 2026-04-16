using System.Collections.Generic;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for sponsor reference data. Returned by sponsor listing endpoints.
    /// Includes cross-system identifiers for integration tracking.
    /// </summary>
    public class SponsorDto
    {
        public string SponsorId { get; set; } = string.Empty;

        public string SponsorName { get; set; } = string.Empty;

        public string LegalName { get; set; } = string.Empty;

        public string Tin { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        /// <summary>
        /// Cross-system reference IDs. Keys: PowerSchoolId, NetSuiteId, StudentChargingPortalId, OnlineBillingSystemId
        /// </summary>
        public Dictionary<string, string> CrossSystemIds { get; set; } = new();

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
