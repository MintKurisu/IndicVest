namespace IndicVest.Core.Domain.Entities.Financial
{
    public class ReturnRate
    {
        public int IdReturnRate { get; set; }

        public required decimal MinReturnRate { get; set; }

        public required decimal MaxReturnRate { get; set; }
    }
}
