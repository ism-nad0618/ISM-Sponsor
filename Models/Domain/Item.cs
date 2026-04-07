namespace ISMSponsor.Models.Domain
{
    public class Item
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Active";
        public string? CategoryId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ItemCategory? Category { get; set; }
    }
}