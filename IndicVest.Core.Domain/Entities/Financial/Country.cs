namespace IndicVest.Core.Domain.Entities.Financial
{
    public class Country
    {
        public int IdCountry { get; set; }

        public required string Name { get; set; }

        public required string ISOCode { get; set; }

        public ICollection<Indicator> Indicators { get; set; } = new List<Indicator>();
    }
}
