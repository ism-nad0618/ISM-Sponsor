namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for charge item reference data. Returned by item listing endpoints.
    /// Used by SCP to populate dropdown menus and validate charge item codes.
    /// </summary>
    public class ItemDto
    {
        public string ItemId { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? Description { get; set; }

        public string GradeLevel { get; set; } = string.Empty;

        public string Currency { get; set; } = "USD";

        public bool IsActive { get; set; }
    }
}
