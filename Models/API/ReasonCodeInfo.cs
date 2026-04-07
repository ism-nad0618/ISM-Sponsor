namespace ISMSponsor.Models.API
{
    public class ReasonCodeInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Success, Partial, Failure
    }
}
