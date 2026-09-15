using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.ReturnRate;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVestWebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IndicVestWebApi.Controllers
{
    public class ReturnRateController : BaseApiController
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReturnRateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get return rate configuration",
            Description = "Retrieves the global minimum and maximum return rate configuration."
        )]
        public async Task<IActionResult> Get()
        {
            var rates = await _returnRateService.GetAll();
            var config = rates.FirstOrDefault();
            if (config is null) throw new NotFoundException(nameof(ReturnRate), "singleton config");
            return Ok(config);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReturnRateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Update return rate configuration", Description = "Updates the global minimum and maximum return rate boundaries.")]
        public async Task<IActionResult> Update([FromBody] ReturnRateViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return validation.Errors.ToValidationProblem(this);

            var result = await _returnRateService.UpdateConfigAsync(vm.MinReturnRate, vm.MaxReturnRate);
            return Ok(result);
        }
    }
}
