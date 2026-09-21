using IndicVest.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IndicVestWebApi.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        private static readonly Dictionary<string, string> DeleteFkMessages =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["FK_Indicators_Countries_IdCountry"] =
                    "Cannot delete: country has associated indicators.",
                ["FK_Indicators_MacroIndicators_IdMacroIndicator"] =
                    "Cannot delete: macroindicator has associated indicators."
            };

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Error", exception.Message),

                DbUpdateException dbEx => (
                    StatusCodes.Status409Conflict,
                    "Database Conflict",
                    MapDbUpdateMessage(dbEx, context.Request.Method)),

                _ => (StatusCodes.Status500InternalServerError, "Server Error",
                      "An unexpected error occurred.")
            };

            if (statusCode >= 500)
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                _logger.LogWarning(exception, "Handled exception ({Status}): {Message}", statusCode, exception.Message);

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };
            problem.Extensions["errorMessage"] = detail;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        private static string MapDbUpdateMessage(DbUpdateException ex, string httpMethod)
        {
            if (ex.InnerException is PostgresException pg)
            {
                switch (pg.SqlState)
                {
                    // 23503 = foreign_key_violation
                    case PostgresErrorCodes.ForeignKeyViolation:
                        if (HttpMethods.IsDelete(httpMethod)
                            && pg.ConstraintName is not null
                            && DeleteFkMessages.TryGetValue(pg.ConstraintName, out var message))
                            return message;

                        return "The operation violates a relationship with other records.";

                    // 23505 = unique_violation
                    case PostgresErrorCodes.UniqueViolation:
                        return "A record with the same unique value already exists.";
                }
            }

            return "The operation conflicts with existing data.";
        }
    }
}