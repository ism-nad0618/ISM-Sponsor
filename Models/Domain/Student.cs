namespace ISMSponsor.Models.Domain
{
    public class Student
    {
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public string StudentStatus { get; set; } = string.Empty;

        public SchoolYear? SchoolYear { get; set; }
        public Sponsor? Sponsor { get; set; }
    }
}