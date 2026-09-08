using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.Indicator;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class IndicatorController : ControllerBase
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
        [ProducesResponseType(typeof(List<IndicatorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _indicatorService.GetAllWithIncluded(
                new List<string> { "Country", "MacroIndicator" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IndicatorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _indicatorService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("years")]
        [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDistinctYears()
        {
            var years = await _indicatorService.GetDistinctYears();
            return Ok(years);
        }

        [HttpGet("by-country/{countryId}/year/{year}")]
        [ProducesResponseType(typeof(List<IndicatorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCountryAndYear(int countryId, int year)
        {
            var countryIds = new List<int> { countryId };
            var allMacros = await _macroIndicatorService.GetAll();
            var macroIds = allMacros.Select(m => m.IdMacroIndicator).ToList();

            var dtos = await _indicatorService.GetByCountryAndYear(year, countryIds, macroIds);
            return Ok(dtos);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IndicatorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] SaveIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _indicatorService.GetByCountryYearAndMacro(
                vm.IdCountry, vm.Year, vm.IdMacroIndicator);

            if (existing is not null)
                return Conflict("An indicator already exists for this country, year and macroindicator.");

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
        [ProducesResponseType(typeof(IndicatorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveIndicatorViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _indicatorService.GetByCountryYearAndMacro(
                vm.IdCountry, vm.Year, vm.IdMacroIndicator);

            if (existing is not null && existing.IdIndicator != id)
                return Conflict("An indicator already exists for this country, year and macroindicator.");

            var dto = new IndicatorDto
            {
                IdIndicator = id,
                IdCountry = vm.IdCountry,
                IdMacroIndicator = vm.IdMacroIndicator,
                Value = vm.Value,
                Year = vm.Year
            };

            var result = await _indicatorService.UpdateAsync(dto, id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _indicatorService.GetById(id);
            if (exists is null) return NotFound();
            await _indicatorService.DeleteAsync(id);
            return NoContent();
        }
    }
}