using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RankingController : ControllerBase
    {
        private readonly IMacroIndicatorService _macroIndicatorService;
        private readonly IIndicatorService _indicatorService;
        private readonly IRankingCalculationService _rankingCalculationService;

        public RankingController(
            IMacroIndicatorService macroIndicatorService,
            IIndicatorService indicatorService,
            IRankingCalculationService rankingCalculationService)
        {
            _macroIndicatorService = macroIndicatorService;
            _indicatorService = indicatorService;
            _rankingCalculationService = rankingCalculationService;
        }

        [HttpGet("years")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableYears()
        {
            var years = await _indicatorService.GetDistinctYears();
            return Ok(years);
        }

        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Calculate([FromBody] int selectedYear)
        {
            var allMacros = await _macroIndicatorService.GetAll();

            if (!allMacros.Any())
                return BadRequest(new { ErrorMessage = "No macroindicators are configured." });

            var macrosConfig = allMacros.Select(m => new MacroWithWeightDto
            {
                IdMacroIndicator = m.IdMacroIndicator,
                Name = m.Name,
                Weight = m.Weight,
                IsHighBetter = m.IsHighBetter
            }).ToList();

            var result = await _rankingCalculationService.CalculateRanking(selectedYear, macrosConfig);

            if (!result.Success)
                return BadRequest(new { result.ErrorMessage });

            return Ok(new
            {
                Year = selectedYear,
                Rankings = result.Results.Select((r, i) => new
                {
                    Position = i + 1,
                    r.CountryName,
                    r.IsoCode,
                    r.Scoring,
                    r.EstimatedReturnRate
                })
            });
        }
    }

}
