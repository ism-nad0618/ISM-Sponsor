namespace ISMSponsor.Models.Domain
{
    public class SchoolYear
    {
        public string SchoolYearId { get; set; } = string.Empty; // e.g. "25-26"
        public string Name { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Student>? Students { get; set; }
    }
}