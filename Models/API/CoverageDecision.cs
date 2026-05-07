using System.Text.Json.Serialization;

namespace ISMSponsor.Models.API
{
    [JsonConverter(typeof(JsonStringEnumConverter<CoverageDecision>))]
    public enum CoverageDecision
    {
        Covered,
        Split,
        NotCovered
    }
}
