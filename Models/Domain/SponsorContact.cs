namespace ISMSponsor.Models.Domain
{
    public class SponsorContact
    {
        public int SponsorContactId { get; set; }
        public string SponsorId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public Sponsor? Sponsor { get; set; }
    }
}