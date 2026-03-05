namespace ISMSponsor.Models.Domain
{
    public class Item
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}