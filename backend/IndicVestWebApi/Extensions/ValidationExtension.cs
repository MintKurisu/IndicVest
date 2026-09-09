using Microsoft.AspNetCore.Mvc;

namespace IndicVestWebApi.Extensions
{
    public static class ValidationExtension
    {
        public static IActionResult ToValidationProblem(
            this IEnumerable<FluentValidation.Results.ValidationFailure> errors,
            ControllerBase controller)
        {
            var groupedErrors = errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => char.ToLower(g.Key[0]) + g.Key[1..],
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problem = new ValidationProblemDetails(groupedErrors)
            {
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest
            };

            return controller.BadRequest(problem);
        }
    }
}
