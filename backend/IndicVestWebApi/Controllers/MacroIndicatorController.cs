using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.MacroIndicator;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IndicVestWebApi.Controllers
{
    public class MacroIndicatorController : BaseApiController
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MacroIndicatorDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all macroindicators",
            Description = "Retrieves all macroindicators along with their assigned weights and associated indicators."
        )]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _macroIndicatorService.GetAllWithIncluded(
                new List<string> { "Indicators" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MacroIndicatorDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get macroindicator by ID",
            Description = "Retrieves a specific macroindicator details using its unique identifier."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _macroIndicatorService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("remaining-weight")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get remaining weight",
            Description = "Retrieves the remaining unallocated weight capacity and current total weight for macroindicators."
        )]
        public async Task<IActionResult> GetRemainingWeight()
        {
            var all = await _macroIndicatorService.GetAll();
            var totalWeight = all.Sum(m => m.Weight);
            return Ok(new { RemainingWeight = 1m - totalWeight, TotalWeight = totalWeight });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MacroIndicatorDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Create macroindicator", Description = "Creates a new macroindicator ensuring the total weight limit of 1.0 is not exceeded.")]
        public async Task<IActionResult> Create([FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MacroIndicatorDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Update macroindicator", Description = "Updates an existing macroindicator details or weight allocation by its ID.")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveMacroIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete macroindicator",
            Description = "Deletes a macroindicator entry from the system by its ID."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _macroIndicatorService.GetById(id);
            if (exists is null) return NotFound();
            await _macroIndicatorService.DeleteAsync(id);
            return NoContent();
        }
    }
}
