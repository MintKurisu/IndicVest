using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.Country;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IValidator<SaveCountryViewModel> _validator;

        public CountryController(ICountryService countryService, IValidator<SaveCountryViewModel> validator)
        {
            _countryService = countryService;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CountryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _countryService.GetAllWithIncluded(new List<string> { "Indicators" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _countryService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] SaveCountryViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _countryService.GetAll();
            if (existing.Any(c => c.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A country with this name already exists.");
            if (existing.Any(c => c.ISOCode.Equals(vm.ISOCode, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A country with this ISO code already exists.");

            var dto = new CountryDto { Name = vm.Name, ISOCode = vm.ISOCode };
            var result = await _countryService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result!.IdCountry }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveCountryViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var existing = await _countryService.GetAll();
            if (existing.Any(c => c.IdCountry != id && c.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A country with this name already exists.");
            if (existing.Any(c => c.IdCountry != id && c.ISOCode.Equals(vm.ISOCode, StringComparison.OrdinalIgnoreCase)))
                return Conflict("A country with this ISO code already exists.");

            var dto = new CountryDto { IdCountry = id, Name = vm.Name, ISOCode = vm.ISOCode };
            var result = await _countryService.UpdateAsync(dto, id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _countryService.GetById(id);
            if (exists is null) return NotFound();
            await _countryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
