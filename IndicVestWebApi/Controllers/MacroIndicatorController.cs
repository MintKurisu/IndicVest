using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.MacroIndicator;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MacroIndicatorController : ControllerBase
    {
        private readonly IMacroIndicatorService _macroIndicatorService;
        private readonly IValidator<SaveMacroIndicatorViewModel> _validator;

        public MacroIndicatorController(
            IMacroIndicatorService macroIndicatorService,
            IValidator<SaveMacroIndicatorViewModel> validator)
        {
            _macroIndicatorService = macroIndicatorService;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<MacroIndicatorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _macroIndicatorService.GetAllWithIncluded(
                new List<string> { "Indicators" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MacroIndicatorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _macroIndicatorService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("remaining-weight")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRemainingWeight()
        {
            var all = await _macroIndicatorService.GetAll();
            var totalWeight = all.Sum(m => m.Weight);
            return Ok(new { RemainingWeight = 1m - totalWeight, TotalWeight = totalWeight });
        }

        [HttpPost]
        [ProducesResponseType(typeof(MacroIndicatorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _macroIndicatorService.GetAll();

            if (existing.Any(m => m.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A macroindicator with this name already exists.");

            var totalWeight = existing.Sum(m => m.Weight);

            if (totalWeight >= 1m)
                return BadRequest("No more macroindicators can be added — total weight is already 1.");

            if (totalWeight + vm.Weight > 1m)
                return BadRequest($"Weight exceeds the limit. Available: {1m - totalWeight:F4}");

            var dto = new MacroIndicatorDto
            {
                Name = vm.Name,
                Weight = vm.Weight,
                IsHighBetter = vm.IsHighBetter
            };

            var result = await _macroIndicatorService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result!.IdMacroIndicator }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MacroIndicatorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _macroIndicatorService.GetAll();

            if (existing.Any(m => m.IdMacroIndicator != id &&
                m.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A macroindicator with this name already exists.");

            var otherWeight = existing
                .Where(m => m.IdMacroIndicator != id)
                .Sum(m => m.Weight);

            if (otherWeight + vm.Weight > 1m)
                return BadRequest($"Weight exceeds the limit. Available: {1m - otherWeight:F4}");

            var dto = new MacroIndicatorDto
            {
                IdMacroIndicator = id,
                Name = vm.Name,
                Weight = vm.Weight,
                IsHighBetter = vm.IsHighBetter
            };

            var result = await _macroIndicatorService.UpdateAsync(dto, id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _macroIndicatorService.GetById(id);
            if (exists is null) return NotFound();
            await _macroIndicatorService.DeleteAsync(id);
            return NoContent();
        }
    }
}
