namespace ISMSponsor.Models.Domain
{
    public class ItemCategory
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Item>? Items { get; set; }
    }
}
