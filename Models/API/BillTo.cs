using System.Text.Json.Serialization;

namespace ISMSponsor.Models.API
{
    [JsonConverter(typeof(JsonStringEnumConverter<BillTo>))]
    public enum BillTo
    {
        Sponsor,
        Parent,
        Split,
        SponsorAndParent
    }
}
