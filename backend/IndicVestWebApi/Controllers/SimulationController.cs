using FluentValidation;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Ranking.RankingSimulator;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;
        private readonly IMacroIndicatorService _macroIndicatorService;
        private readonly IValidator<SaveSimulationMacroViewModel> _validator;

        public SimulationController(
            ISimulationService simulationService,
            IMacroIndicatorService macroIndicatorService,
            IValidator<SaveSimulationMacroViewModel> validator)
        {
            _simulationService = simulationService;
            _macroIndicatorService = macroIndicatorService;
            _validator = validator;
        }

        [HttpGet("available-macros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableMacros()
        {
            var all = await _macroIndicatorService.GetAll();
            return Ok(all.Select(m => new
            {
                m.IdMacroIndicator,
                m.Name,
                m.Weight,
                m.IsHighBetter
            }));
        }

        [HttpPost("validate-config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateConfig([FromBody] List<MacroWithWeightDto> config)
        {
            if (config is null || !config.Any())
                return BadRequest("Simulation configuration cannot be empty.");

            foreach (var item in config)
            {
                var vm = new SaveSimulationMacroViewModel
                {
                    SelectedMacroIndicator = item.IdMacroIndicator,
                    Weight = item.Weight
                };

                var validation = await _validator.ValidateAsync(vm);
                if (!validation.IsValid)
                    return validation.Errors.ToValidationProblem(this);
            }

            var totalWeight = config.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1m) > 0.0001m)
                return BadRequest($"Total weight must equal 1. Current: {totalWeight:F4}");

            var allMacros = await _macroIndicatorService.GetAll();
            var allIds = allMacros.Select(m => m.IdMacroIndicator).ToHashSet();
            var invalidIds = config.Where(c => !allIds.Contains(c.IdMacroIndicator)).ToList();

            if (invalidIds.Any())
                return BadRequest("One or more macroindicators in the configuration do not exist.");

            return Ok(new { Valid = true, TotalWeight = totalWeight });
        }

        [HttpPost("run")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Run([FromBody] SimulationRequestViewModel vm)
        {
            if (vm.Configuration is null || !vm.Configuration.Any())
                return BadRequest("Simulation configuration cannot be empty.");

            var result = await _simulationService.RunSimulation(vm.Configuration, vm.Year);

            if (!result.Success)
                return BadRequest(new { result.ErrorMessage });

            return Ok(new
            {
                vm.Year,
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
