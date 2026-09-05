using FluentValidation;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Ranking.RankingSimulator;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET api/simulation/available-macros
        // Returns all available macroindicators for building a simulation
        [HttpGet("available-macros")]
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

        // POST api/simulation/validate-config
        // Validates that the macro configuration sent by the frontend is valid before running the simulation
        [HttpPost("validate-config")]
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
                    return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var totalWeight = config.Sum(c => c.Weight);
            if (Math.Abs(totalWeight - 1m) > 0.0001m)
                return BadRequest($"Total weight must equal 1. Current: {totalWeight:F4}");

            // Verify that the macros exist
            var allMacros = await _macroIndicatorService.GetAll();
            var allIds = allMacros.Select(m => m.IdMacroIndicator).ToHashSet();
            var invalidIds = config.Where(c => !allIds.Contains(c.IdMacroIndicator)).ToList();

            if (invalidIds.Any())
                return BadRequest("One or more macroindicators in the configuration do not exist.");

            return Ok(new { Valid = true, TotalWeight = totalWeight });
        }

        // POST api/simulation/run
        // The frontend sends the complete configuration + the year and the backend runs the simulation
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] SimulationRequestViewModel vm)
        {
            if (vm.Configuration is null || !vm.Configuration.Any())
                return BadRequest("Simulation configuration cannot be empty.");

            var canAdd = await _simulationService.AddMacroToSimulation(
                new List<MacroWithWeightDto>(), vm.Configuration.First().IdMacroIndicator, vm.Configuration.First().Weight);

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
