namespace ISMSponsor.Models.Domain
{
    public class ActivityLog
    {
        public int ActivityLogId { get; set; }
        public DateTime Date { get; set; }
        public string Item { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string UserDisplay { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string SchoolYearId { get; set; } = string.Empty;
    }
}