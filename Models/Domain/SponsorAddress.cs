namespace ISMSponsor.Models.Domain
{
    public class SponsorAddress
    {
        public int SponsorAddressId { get; set; }
        public string SponsorId { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty; // Billing, Mailing, Physical
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;

        public Sponsor? Sponsor { get; set; }
    }
}
