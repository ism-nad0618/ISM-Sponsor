namespace ISMSponsor.Models.Domain
{
    public class Sponsor
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Tin { get; set; } = string.Empty;

        public ICollection<SponsorContact>? Contacts { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<ChangeRequest>? ChangeRequests { get; set; }
    }
}