using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.Country;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IndicVestWebApi.Controllers
{
    public class CountryController : BaseApiController
    {
        private readonly ICountryService _countryService;
        private readonly IValidator<SaveCountryViewModel> _validator;

        public CountryController(ICountryService countryService, IValidator<SaveCountryViewModel> validator)
        {
            _countryService = countryService;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CountryDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all countries",
            Description = "Retrieves a list of all registered countries in the system including their associated indicators."
        )]
        public async Task<IActionResult> GetAll()
        {
            var dtos = await _countryService.GetAllWithIncluded(new List<string> { "Indicators" });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountryDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get country by ID",
            Description = "Retrieves specific country details using its unique identifier."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _countryService.GetById(id);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CountryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Create country", Description = "Creates a new country record in the system after validating unique name and ISO code.")]
        public async Task<IActionResult> Create([FromBody] SaveCountryViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var dto = new CountryDto { Name = vm.Name, ISOCode = vm.ISOCode };
            var result = await _countryService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result!.IdCountry }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Update country", Description = "Updates existing country information by its ID after validating uniqueness rules.")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveCountryViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var dto = new CountryDto { IdCountry = id, Name = vm.Name, ISOCode = vm.ISOCode };
            var result = await _countryService.UpdateAsync(dto, id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete country",
            Description = "Deletes a country record from the system by its ID."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _countryService.GetById(id);
            if (exists is null) return NotFound();
            await _countryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
