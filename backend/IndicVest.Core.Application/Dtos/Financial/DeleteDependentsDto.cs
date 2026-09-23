namespace IndicVest.Core.Application.Dtos.Financial
{
    public class DeleteDependentsDto
    {
        public int IndicatorCount { get; set; }
        public List<int> Years { get; set; } = new();
    }
}
