using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;
using IndicVest.Core.Application.ViewModels.Ranking;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IndicVestWebApi.Controllers
{
    public class RankingController : BaseApiController
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get available years for ranking",
            Description = "Retrieves a list of years for which indicator data is available to calculate rankings."
        )]
        public async Task<IActionResult> GetAvailableYears()
        {
            var years = await _indicatorService.GetDistinctYears();
            return Ok(years);
        }

        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RankingResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Calculate country ranking",
            Description = "Calculates investment attraction ranking and scoring for all countries for a specific selected year based on configured macroindicator weights."
        )]
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

            return Ok(new RankingResponseDto
            {
                Year = selectedYear,
                Rankings = result.Results.Select((r, i) => new RankingItemResponseDto
                {
                    Position = i + 1,
                    CountryName = r.CountryName,
                    IsoCode = r.IsoCode,
                    Scoring = r.Scoring,
                    EstimatedReturnRate = r.EstimatedReturnRate
                }).ToList()
            });
        }
    }

}
