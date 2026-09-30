using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Ranking;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Ranking
{
    public class RankingCalculationServiceTests
    {
        private readonly Mock<IIndicatorService> _indicatorServiceMock;
        private readonly Mock<ICountryService> _countryServiceMock;
        private readonly Mock<IReturnRateService> _returnRateServiceMock;

        private readonly RankingCalculationService _service;

        public RankingCalculationServiceTests()
        {
            _indicatorServiceMock = new Mock<IIndicatorService>();
            _countryServiceMock = new Mock<ICountryService>();
            _returnRateServiceMock = new Mock<IReturnRateService>();

            _service = new RankingCalculationService(
                _indicatorServiceMock.Object,
                _countryServiceMock.Object,
                _returnRateServiceMock.Object
            );
        }

        private static CountryDto Country(int id, string name, string iso) =>
            new() { IdCountry = id, Name = name, ISOCode = iso };

        private static IndicatorDto Ind(int countryId, int macroId, decimal value, int year = 2024) =>
            new() { IdCountry = countryId, IdMacroIndicator = macroId, Value = value, Year = year };

        private static MacroWithWeightDto Macro(int id, string name, decimal weight, bool highBetter) =>
            new() { IdMacroIndicator = id, Name = name, Weight = weight, IsHighBetter = highBetter };

        private void SetupData(List<CountryDto> countries, List<IndicatorDto> indicators)
        {
            _countryServiceMock
                .Setup(s => s.GetAll())
                .ReturnsAsync(countries);

            _indicatorServiceMock
                .Setup(s => s.GetByCountryAndYear(
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<List<int>>()))
                .ReturnsAsync((int year, List<int> countryIds, List<int> macroIds) =>
                    indicators
                        .Where(i => i.Year == year
                                    && countryIds.Contains(i.IdCountry)
                                    && macroIds.Contains(i.IdMacroIndicator))
                        .ToList());
        }

        private void SetupReturnRate(params ReturnRateDto[] rates)
        {
            _returnRateServiceMock
                .Setup(s => s.GetAll())
                .ReturnsAsync(rates.ToList());
        }

        private (List<CountryDto> Countries, List<IndicatorDto> Indicators, List<MacroWithWeightDto> Macros) DocExample()
        {
            var countries = new List<CountryDto>
        {
            Country(1, "A", "AA"),
            Country(2, "B", "BB"),
            Country(3, "C", "CC")
        };

            var indicators = new List<IndicatorDto>
        {
            Ind(1, 1, 40000m), Ind(1, 2, 5.0m),
            Ind(2, 1, 30000m), Ind(2, 2, 8.0m),
            Ind(3, 1, 50000m), Ind(3, 2, 10.0m)
        };

            var macros = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB per capita", 0.6m, true),
            Macro(2, "Inflacion", 0.4m, false)
        };

            return (countries, indicators, macros);
        }

        [Fact]
        public async Task CalculateRanking_ShouldFail_WhenWeightsDoNotSumToOne()
        {
            // Arrange
            var macros = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.5m, true),
            Macro(2, "Inflacion", 0.3m, false)
        };

            // Act
            var (success, error, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.False(success);
            Assert.False(string.IsNullOrEmpty(error));
            Assert.Empty(results);

            _countryServiceMock.Verify(s => s.GetAll(), Times.Never);
        }

        [Fact]
        public async Task CalculateRanking_ShouldFail_WhenNoEligibleCountries()
        {
            // Arrange
            var (countries, _, macros) = DocExample();
            SetupData(countries, new List<IndicatorDto>());

            // Act
            var (success, error, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.False(success);
            Assert.Contains("2024", error);
            Assert.Empty(results);
        }

        [Fact]
        public async Task CalculateRanking_ShouldFail_WhenOnlyOneCountryIsEligible()
        {
            // Arrange
            var (countries, _, macros) = DocExample();
            var indicators = new List<IndicatorDto>
        {
            Ind(1, 1, 40000m), Ind(1, 2, 5.0m),
            Ind(2, 1, 30000m)
        };
            SetupData(countries, indicators);

            // Act
            var (success, error, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.False(success);
            Assert.Contains("A", error);
            Assert.Empty(results);
        }

        [Fact]
        public async Task CalculateRanking_ShouldExcludeCountry_WhenMissingRequiredIndicator()
        {
            // Arrange
            var (countries, _, macros) = DocExample();
            var indicators = new List<IndicatorDto>
        {
            Ind(1, 1, 40000m), Ind(1, 2, 5.0m),
            Ind(2, 1, 30000m), Ind(2, 2, 8.0m),
            Ind(3, 1, 50000m)
        };
            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (success, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.True(success);
            Assert.Equal(2, results.Count);
            Assert.DoesNotContain(results, r => r.CountryName == "C");
        }

        [Fact]
        public async Task CalculateRanking_ShouldKeepCountryEligible_WhenMissingIndicatorHasZeroWeight()
        {
            // Arrange
            var (countries, indicators, _) = DocExample();
            var macros = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB per capita", 0.6m, true),
            Macro(2, "Inflacion", 0.4m, false),
            Macro(3, "Riesgo pais", 0m, false)
        };
            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (success, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.True(success);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task CalculateRanking_ShouldMatchDocumentExample_ScoringAndReturnRate()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate(new ReturnRateDto { IdReturnRate = 1, MinReturnRate = 0.02m, MaxReturnRate = 0.15m });

            // Act
            var (success, error, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.True(success);
            Assert.Equal("", error);
            Assert.Equal(3, results.Count);

            Assert.Equal(new[] { "A", "C", "B" }, results.Select(r => r.CountryName).ToArray());

            Assert.Equal(0.70m, results[0].Scoring, 4);
            Assert.Equal(0.111m, results[0].EstimatedReturnRate, 4);

            Assert.Equal(0.60m, results[1].Scoring, 4);
            Assert.Equal(0.098m, results[1].EstimatedReturnRate, 4);

            Assert.Equal(0.16m, results[2].Scoring, 4);
            Assert.Equal(0.0408m, results[2].EstimatedReturnRate, 4);
        }

        [Fact]
        public async Task CalculateRanking_ShouldReturnResultsOrderedByScoringDescending()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            var scores = results.Select(r => r.Scoring).ToList();
            Assert.Equal(scores.OrderByDescending(s => s).ToList(), scores);
        }

        [Fact]
        public async Task CalculateRanking_ShouldKeepScoringBetweenZeroAndOne()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.All(results, r => Assert.InRange(r.Scoring, 0m, 1m));
        }

        [Fact]
        public async Task CalculateRanking_ShouldNormalizeToHalf_WhenMinEqualsMax()
        {
            // Arrange
            var countries = new List<CountryDto>
        {
            Country(1, "A", "AA"),
            Country(2, "B", "BB")
        };
            var indicators = new List<IndicatorDto>
        {
            Ind(1, 1, 5m),
            Ind(2, 1, 5m)
        };
            var macros = new List<MacroWithWeightDto> { Macro(1, "Inflacion", 1m, false) };

            SetupData(countries, indicators);
            SetupReturnRate(new ReturnRateDto { MinReturnRate = 0.02m, MaxReturnRate = 0.15m });

            // Act
            var (success, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.True(success);
            Assert.All(results, r =>
            {
                Assert.Equal(0.5m, r.Scoring, 4);
                Assert.Equal(0.085m, r.EstimatedReturnRate, 4);
            });
        }

        [Fact]
        public async Task CalculateRanking_ShouldInvertNormalization_WhenLowerIsBetter()
        {
            // Arrange
            var countries = new List<CountryDto>
        {
            Country(1, "A", "AA"),
            Country(2, "B", "BB")
        };
            var indicators = new List<IndicatorDto>
        {
            Ind(1, 1, 5m),
            Ind(2, 1, 10m)
        };
            var macros = new List<MacroWithWeightDto> { Macro(1, "Inflacion", 1m, false) };

            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.Equal("A", results[0].CountryName);
            Assert.Equal(1m, results[0].Scoring, 4);
            Assert.Equal("B", results[1].CountryName);
            Assert.Equal(0m, results[1].Scoring, 4);
        }

        [Fact]
        public async Task CalculateRanking_ShouldUseDefaultRates_WhenNoConfigExists()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate();

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.Equal(0.111m, results.First(r => r.CountryName == "A").EstimatedReturnRate, 4);
        }

        [Fact]
        public async Task CalculateRanking_ShouldUseDefaultRates_WhenConfigIsInvalid()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate(new ReturnRateDto { MinReturnRate = 0.20m, MaxReturnRate = 0.10m });

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.Equal(0.111m, results.First(r => r.CountryName == "A").EstimatedReturnRate, 4);
        }

        [Fact]
        public async Task CalculateRanking_ShouldUseConfiguredRates_WhenConfigIsValid()
        {
            // Arrange
            var (countries, indicators, macros) = DocExample();
            SetupData(countries, indicators);
            SetupReturnRate(new ReturnRateDto { MinReturnRate = 0.05m, MaxReturnRate = 0.25m });

            // Act
            var (_, _, results) = await _service.CalculateRanking(2024, macros);

            // Assert
            Assert.Equal(0.19m, results.First(r => r.CountryName == "A").EstimatedReturnRate, 4);
        }
    }
}
