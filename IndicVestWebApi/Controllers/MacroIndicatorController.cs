using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.MacroIndicator;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET api/macroindicator
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _macroIndicatorService.GetAllWithIncluded(
                new List<string> { "Indicators" });
            return Ok(dtos);
        }

        // GET api/macroindicator/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _macroIndicatorService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // GET api/macroindicator/remaining-weight
        [HttpGet("remaining-weight")]
        public async Task<IActionResult> GetRemainingWeight()
        {
            var all = await _macroIndicatorService.GetAll();
            var totalWeight = all.Sum(m => m.Weight);
            return Ok(new { RemainingWeight = 1m - totalWeight, TotalWeight = totalWeight });
        }

        // POST api/macroindicator
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

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

        // PUT api/macroindicator/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

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

        // DELETE api/macroindicator/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _macroIndicatorService.GetById(id);
            if (exists is null) return NotFound();
            await _macroIndicatorService.DeleteAsync(id);
            return NoContent();
        }
    }
}
