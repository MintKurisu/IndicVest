using FluentValidation;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;
using IndicVest.Core.Application.Services.Financial;
using IndicVest.Core.Application.Services.Ranking;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IndicVest.Core.Application.IOC
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Mappings
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            #endregion

            #region FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            #endregion

            #region Services IOC
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IIndicatorService, IndicatorService>();
            services.AddScoped<IMacroIndicatorService, MacroIndicatorService>();
            services.AddScoped<IReturnRateService, ReturnRateService>();
            services.AddScoped<ISimulationService, SimulationService>();
            services.AddScoped<IRankingCalculationService, RankingCalculationService>();
            #endregion
        }
    }
}
