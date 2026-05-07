namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Represents a single billing allocation between sponsor and parent.
    /// Used when coverage decision results in a split billing arrangement.
    /// </summary>
    public class BillingAllocationDto
    {
        /// <summary>
        /// Party type: Sponsor or Parent
        /// </summary>
        public string PartyType { get; set; } = string.Empty;

        /// <summary>
        /// Party ID: SponsorId for Sponsor, null for Parent
        /// </summary>
        public string? PartyId { get; set; }

        /// <summary>
        /// Party name: Sponsor name for Sponsor, null for Parent
        /// </summary>
        public string? PartyName { get; set; }

        /// <summary>
        /// Amount allocated to this party
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency for the amount (e.g., "US Dollar", "PHP")
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Bill to: Sponsor or Parent
        /// </summary>
        public string BillTo { get; set; } = string.Empty;

        /// <summary>
        /// Charge code for the item
        /// </summary>
        public string ChargeCode { get; set; } = string.Empty;

        /// <summary>
        /// Charge description
        /// </summary>
        public string? ChargeDescription { get; set; }
    }
}
