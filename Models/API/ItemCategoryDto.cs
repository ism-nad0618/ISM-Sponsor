namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for charge item category reference data.
    /// Used by SCP to populate category dropdowns and organize items.
    /// </summary>
    public class ItemCategoryDto
    {
        public string CategoryId { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
