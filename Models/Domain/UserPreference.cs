namespace ISMSponsor.Models.Domain
{
    public class UserPreference
    {
        public int UserPreferenceId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ActiveSchoolYearId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
        public SchoolYear? ActiveSchoolYear { get; set; }
    }
}