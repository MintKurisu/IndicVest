using IndicVest.Core.Application.ViewModels.Financial.Country;
using IndicVest.Core.Application.ViewModels.Financial.MacroIndicator;

namespace IndicVest.Core.Application.ViewModels.Financial.Indicator
{
    public class SaveIndicatorViewModel
    {
        public int IdIndicator { get; set; }
        public int IdCountry { get; set; }
        public int IdMacroIndicator { get; set; }
        public decimal Value { get; set; }
        public int Year { get; set; }
        public string? CountryName { get; set; }
        public string? MacroIndicatorName { get; set; }
        public List<CountryViewModel> Countries { get; set; } = new();
        public List<MacroIndicatorViewModel> MacroIndicators { get; set; } = new();
    }
}
