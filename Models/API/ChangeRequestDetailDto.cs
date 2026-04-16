using System;
using System.Collections.Generic;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for sponsor change request details.
    /// Used to display change workflow status, original values, proposed values, and approver actions.
    /// </summary>
    public class ChangeRequestDetailDto
    {
        public string ChangeRequestId { get; set; } = string.Empty;

        public string SponsorId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor legal name for UI reference.
        /// </summary>
        public string SponsorName { get; set; } = string.Empty;

        /// <summary>
        /// Who requested the change.
        /// </summary>
        public string RequestedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name or email of requester.
        /// </summary>
        public string RequestedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// When the change was requested (UTC).
        /// </summary>
        public DateTime RequestedOn { get; set; }

        /// <summary>
        /// Current status: Pending, Approved, Rejected, Withdrawn.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// If Approved/Rejected, who made the decision.
        /// </summary>
        public string? ApprovedByUserId { get; set; }

        /// <summary>
        /// Display name/email of approver, if applicable.
        /// </summary>
        public string? ApprovedByUserName { get; set; }

        /// <summary>
        /// When the change was approved/rejected (UTC).
        /// </summary>
        public DateTime? ApprovedOn { get; set; }

        /// <summary>
        /// Approval comment from approver.
        /// </summary>
        public string? ApprovalNotes { get; set; }

        /// <summary>
        /// List of individual field changes in this request.
        /// </summary>
        public List<FieldChangeDto> FieldChanges { get; set; } = new();
    }

    /// <summary>
    /// DTO for a single field change within a change request.
    /// </summary>
    public class FieldChangeDto
    {
        /// <summary>
        /// Field name being changed (e.g., "Name", "TIN", "Address", "ContactEmail").
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Current/original value in the system.
        /// </summary>
        public string? OriginalValue { get; set; }

        /// <summary>
        /// Proposed new value.
        /// </summary>
        public string? ProposedValue { get; set; }

        /// <summary>
        /// Human-readable reason for the change request.
        /// </summary>
        public string? ChangeReason { get; set; }

        /// <summary>
        /// Data type for UI rendering hints (string, date, decimal, etc.).
        /// </summary>
        public string DataType { get; set; } = "string";
    }
}
