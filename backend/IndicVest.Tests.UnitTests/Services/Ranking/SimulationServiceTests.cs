using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;
using IndicVest.Core.Application.Services.Ranking;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Ranking
{
    public class SimulationServiceTests
    {
        private readonly Mock<IMacroIndicatorService> _macroIndicatorServiceMock;
        private readonly Mock<IRankingCalculationService> _rankingCalculationServiceMock;

        private readonly SimulationService _service;

        public SimulationServiceTests()
        {
            _macroIndicatorServiceMock = new Mock<IMacroIndicatorService>();
            _rankingCalculationServiceMock = new Mock<IRankingCalculationService>();

            _service = new SimulationService(
                _macroIndicatorServiceMock.Object,
                _rankingCalculationServiceMock.Object
            );
        }

        private static MacroWithWeightDto Macro(int id, string name, decimal weight, bool highBetter = true) =>
            new() { IdMacroIndicator = id, Name = name, Weight = weight, IsHighBetter = highBetter };

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnFalse_WhenMacroAlreadyInConfig()
        {
            // Arrange
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.3m) };

            // Act
            var result = await _service.AddMacroToSimulation(config, 1, 0.2m);

            // Assert
            Assert.False(result);
            _macroIndicatorServiceMock.Verify(s => s.GetById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnFalse_WhenTotalWeightExceedsOne()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.6m),
            Macro(2, "Inflacion", 0.3m)
        };

            // Act: 0.9 + 0.2 = 1.1
            var result = await _service.AddMacroToSimulation(config, 3, 0.2m);

            // Assert
            Assert.False(result);
            _macroIndicatorServiceMock.Verify(s => s.GetById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnFalse_WhenMacroDoesNotExistInDb()
        {
            // Arrange
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.3m) };

            _macroIndicatorServiceMock
                .Setup(s => s.GetById(99))
                .ReturnsAsync((MacroIndicatorDto?)null);

            // Act
            var result = await _service.AddMacroToSimulation(config, 99, 0.2m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnTrue_WhenValid()
        {
            // Arrange
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.3m) };

            _macroIndicatorServiceMock
                .Setup(s => s.GetById(2))
                .ReturnsAsync(new MacroIndicatorDto { IdMacroIndicator = 2, Name = "Inflacion", Weight = 0.4m });

            // Act
            var result = await _service.AddMacroToSimulation(config, 2, 0.2m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnTrue_WhenTotalWeightIsExactlyOne()
        {
            // Arrange: 0.5 + 0.5 = 1.0 
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.5m) };

            _macroIndicatorServiceMock
                .Setup(s => s.GetById(2))
                .ReturnsAsync(new MacroIndicatorDto { IdMacroIndicator = 2, Name = "Inflacion" });

            // Act
            var result = await _service.AddMacroToSimulation(config, 2, 0.5m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddMacroToSimulation_ShouldReturnFalse_WhenServiceThrows()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>();

            _macroIndicatorServiceMock
                .Setup(s => s.GetById(It.IsAny<int>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _service.AddMacroToSimulation(config, 1, 0.2m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateMacroInSimulation_ShouldReturnTrue_WhenNewWeightIsWithinLimit()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.3m),
            Macro(2, "Inflacion", 0.2m)
        };

            // Act
            var result = await _service.UpdateMacroInSimulation(config, 1, 0.5m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateMacroInSimulation_ShouldReturnFalse_WhenNewWeightExceedsLimit()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.3m),
            Macro(2, "Inflacion", 0.4m)
        };

            // Act
            var result = await _service.UpdateMacroInSimulation(config, 1, 0.7m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateMacroInSimulation_ShouldIgnoreOwnCurrentWeight()
        {
            // Arrange
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.9m) };

            // Act
            var result = await _service.UpdateMacroInSimulation(config, 1, 0.9m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateMacroInSimulation_ShouldReturnTrue_WhenTotalWeightIsExactlyOne()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.3m),
            Macro(2, "Inflacion", 0.4m)
        };

            // Act
            var result = await _service.UpdateMacroInSimulation(config, 1, 0.6m);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RunSimulation_ShouldDelegateToRankingService_WithSameYearAndConfiguration()
        {
            // Arrange
            var config = new List<MacroWithWeightDto>
        {
            Macro(1, "PIB", 0.2m),
            Macro(2, "Inflacion", 0.8m, false)
        };

            var expected = new List<RankingResultDto>
        {
            new() { IdCountry = 1, CountryName = "A", IsoCode = "AA", Scoring = 0.7m, EstimatedReturnRate = 0.111m }
        };

            _rankingCalculationServiceMock
                .Setup(s => s.CalculateRanking(2024, config))
                .ReturnsAsync((true, "", expected));

            // Act
            var (success, error, results) = await _service.RunSimulation(config, 2024);

            // Assert
            Assert.True(success);
            Assert.Equal("", error);
            Assert.Same(expected, results);

            _rankingCalculationServiceMock.Verify(
                s => s.CalculateRanking(2024, config),
                Times.Once
            );
        }

        [Fact]
        public async Task RunSimulation_ShouldPropagateFailure_FromRankingService()
        {
            // Arrange
            var config = new List<MacroWithWeightDto> { Macro(1, "PIB", 0.5m) };

            _rankingCalculationServiceMock
                .Setup(s => s.CalculateRanking(2024, config))
                .ReturnsAsync((false, "Weights must sum to 1.", new List<RankingResultDto>()));

            // Act
            var (success, error, results) = await _service.RunSimulation(config, 2024);

            // Assert
            Assert.False(success);
            Assert.Equal("Weights must sum to 1.", error);
            Assert.Empty(results);
        }
    }
}
