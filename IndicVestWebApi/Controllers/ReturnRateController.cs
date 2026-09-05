using FluentValidation;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.ViewModels.Financial.ReturnRate;
using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET api/returnrate
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var rates = await _returnRateService.GetAll();
            var config = rates.FirstOrDefault();
            if (config is null) return NotFound("No return rate configuration found.");
            return Ok(config);
        }

        // PUT api/returnrate
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ReturnRateViewModel vm)
        {
            var validation = await _validator.ValidateAsync(vm);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

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
