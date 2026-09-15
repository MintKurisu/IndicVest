using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.Indicator;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IndicVestWebApi.Controllers
{
    public class IndicatorController : BaseApiController
    {
        private readonly IIndicatorService _indicatorService;
        private readonly IMacroIndicatorService _macroIndicatorService;
        private readonly IValidator<SaveIndicatorViewModel> _validator;

        public IndicatorController(
            IIndicatorService indicatorService,
            IMacroIndicatorService macroIndicatorService,
            IValidator<SaveIndicatorViewModel> validator)
        {
            _indicatorService = indicatorService;
            _macroIndicatorService = macroIndicatorService;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<IndicatorDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all indicators",
            Description = "Retrieves all registered indicator entries including Country and MacroIndicator details."
        )]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _indicatorService.GetAllWithIncluded(
                new List<string> { "Country", "MacroIndicator" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IndicatorDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get indicator by ID",
            Description = "Retrieves an indicator entry by its unique identifier."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _indicatorService.GetById(id);
            if (dto is null) throw new NotFoundException(nameof(Indicator), id);
            return Ok(dto);
        }

        [HttpGet("years")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<int>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get distinct years",
            Description = "Retrieves a distinct list of years containing indicator data."
        )]
        public async Task<IActionResult> GetDistinctYears()
        {
            var years = await _indicatorService.GetDistinctYears();
            return Ok(years);
        }

        [HttpGet("by-country/{countryId}/year/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<IndicatorDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get indicators by country and year",
            Description = "Retrieves all indicators associated with a specific country ID and year."
        )]
        public async Task<IActionResult> GetByCountryAndYear(int countryId, int year)
        {
            var countryIds = new List<int> { countryId };
            var allMacros = await _macroIndicatorService.GetAll();
            var macroIds = allMacros.Select(m => m.IdMacroIndicator).ToList();

            var dtos = await _indicatorService.GetByCountryAndYear(year, countryIds, macroIds);
            return Ok(dtos);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IndicatorDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Create indicator", Description = "Creates a new indicator record for a country, macroindicator, and year.")]
        public async Task<IActionResult> Create([FromBody] SaveIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var dto = new IndicatorDto
            {
                IdCountry = vm.IdCountry,
                IdMacroIndicator = vm.IdMacroIndicator,
                Value = vm.Value,
                Year = vm.Year
            };

            var result = await _indicatorService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result!.IdIndicator }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IndicatorDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Update indicator", Description = "Updates an existing indicator record by its unique identifier.")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var dto = new IndicatorDto
            {
                IdIndicator = id,
                IdCountry = vm.IdCountry,
                IdMacroIndicator = vm.IdMacroIndicator,
                Value = vm.Value,
                Year = vm.Year
            };

            var result = await _indicatorService.UpdateAsync(dto, id);
            if (result is null) throw new NotFoundException(nameof(Indicator), id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete indicator",
            Description = "Deletes an indicator entry from the system by its ID."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _indicatorService.GetById(id);
            if (exists is null) throw new NotFoundException(nameof(Indicator), id);
            await _indicatorService.DeleteAsync(id);
            return NoContent();
        }
    }
}