using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.ReturnRate;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReturnRateController : ControllerBase
    {
        private readonly IReturnRateService _returnRateService;
        private readonly IValidator<ReturnRateViewModel> _validator;

        public ReturnRateController(
            IReturnRateService returnRateService,
            IValidator<ReturnRateViewModel> validator)
        {
            _returnRateService = returnRateService;
            _validator = validator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ReturnRateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            var rates = await _returnRateService.GetAll();
            var config = rates.FirstOrDefault();
            if (config is null) return NotFound("No return rate configuration found.");
            return Ok(config);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ReturnRateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] ReturnRateViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var rates = await _returnRateService.GetAll();
            var existing = rates.FirstOrDefault();
            if (existing is null) return NotFound("No return rate configuration found.");

            var dto = new ReturnRateDto
            {
                IdReturnRate = existing.IdReturnRate,
                MinReturnRate = vm.MinReturnRate,
                MaxReturnRate = vm.MaxReturnRate
            };

            var result = await _returnRateService.UpdateAsync(dto, existing.IdReturnRate);
            return Ok(result);
        }
    }
}
