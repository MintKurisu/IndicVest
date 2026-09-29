using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Services.Financial;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;
using MockQueryable;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Financial
{
    public class IndicatorServiceTests
    {
        private readonly Mock<IIndicatorRepository> _indicatorRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly IndicatorService _service;

        public IndicatorServiceTests()
        {
            _indicatorRepositoryMock = new Mock<IIndicatorRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new IndicatorService(
                _indicatorRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowConflict_WhenIndicatorAlreadyExistsForCountryYearAndMacro()
        {
            // Arrange
            var dto = new IndicatorDto
            {
                IdIndicator = 0,
                IdCountry = 1,
                IdMacroIndicator = 1,
                Year = 2024,
                Value = 5.4m
            };

            var existingList = new List<Indicator>
            {
                new()
                {
                    IdIndicator = 10,
                    IdCountry = 1,
                    IdMacroIndicator = 1,
                    Year = 2024,
                    Value = 5.0m
                }
            };

            var mockQueryable = existingList.BuildMock();

            _indicatorRepositoryMock
                .Setup(r => r.GetAllQuery())
                .Returns(mockQueryable);

            _indicatorRepositoryMock
                .Setup(r => r.GetAllQueryWithInclude(It.IsAny<List<string>>()))
                .Returns(mockQueryable);

            _indicatorRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(existingList);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldSucceed_WhenIndicatorIsUnique()
        {
            // Arrange
            var dto = new IndicatorDto
            {
                IdIndicator = 0,
                IdCountry = 1,
                IdMacroIndicator = 1,
                Year = 2024,
                Value = 5.4m
            };

            var entity = new Indicator
            {
                IdIndicator = 1,
                IdCountry = 1,
                IdMacroIndicator = 1,
                Year = 2024,
                Value = 5.4m
            };

            var emptyQueryable = new List<Indicator>().BuildMock();

            _indicatorRepositoryMock
                .Setup(r => r.GetAllQuery())
                .Returns(emptyQueryable);

            _indicatorRepositoryMock
                .Setup(r => r.GetAllQueryWithInclude(It.IsAny<List<string>>()))
                .Returns(emptyQueryable);

            _indicatorRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Indicator>());

            _mapperMock
                .Setup(m => m.Map<Indicator>(dto))
                .Returns(entity);

            _indicatorRepositoryMock
                .Setup(r => r.AddAsync(entity))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<IndicatorDto>(entity))
                .Returns(dto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.IdCountry, result!.IdCountry);
            Assert.Equal(dto.IdMacroIndicator, result.IdMacroIndicator);
            Assert.Equal(dto.Year, result.Year);

            _indicatorRepositoryMock.Verify(
                r => r.AddAsync(entity),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllWithIncluded_ShouldMapNavigationsCorrectly()
        {
            // Arrange
            var entities = new List<Indicator>
            {
                new()
                {
                    IdIndicator = 1,
                    IdCountry = 1,
                    IdMacroIndicator = 2,
                    Year = 2024,
                    Value = 10.5m,
                    Country = new Country { IdCountry = 1, Name = "Dominican Republic", ISOCode = "DO" },
                    MacroIndicator = new MacroIndicator { IdMacroIndicator = 2, Name = "GDP Growth", Weight = 0.20m, IsHighBetter = true }
                }
            };

            _indicatorRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(entities);

            // Act
            var result = await _service.GetAllWithIncluded(new List<string> { "Country", "MacroIndicator" });

            // Assert
            Assert.Single(result);
            Assert.Equal("Dominican Republic", result[0].CountryName);
            Assert.Equal("GDP Growth", result[0].MacroIndicatorName);
            Assert.Equal(10.5m, result[0].Value);
        }

        [Fact]
        public async Task GetDistinctYears_ShouldReturnOrderedYearsDescending()
        {
            // Arrange
            var indicators = new List<Indicator>
            {
                new() { IdIndicator = 1, IdCountry = 1, IdMacroIndicator = 1, Year = 2021, Value = 1.0m },
                new() { IdIndicator = 2, IdCountry = 1, IdMacroIndicator = 1, Year = 2024, Value = 2.0m },
                new() { IdIndicator = 3, IdCountry = 2, IdMacroIndicator = 1, Year = 2021, Value = 3.0m },
                new() { IdIndicator = 4, IdCountry = 1, IdMacroIndicator = 2, Year = 2023, Value = 4.0m }
            };

            var mockQueryable = indicators.BuildMock();

            _indicatorRepositoryMock
                .Setup(r => r.GetAllQuery())
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetDistinctYears();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { 2024, 2023, 2021 }, result);
        }

        [Fact]
        public async Task GetByMacroAndYear_ShouldReturnMatchingIndicators()
        {
            // Arrange
            const int macroId = 5;
            const int year = 2024;

            var entities = new List<Indicator>
            {
                new()
                {
                    IdIndicator = 1,
                    IdCountry = 1,
                    IdMacroIndicator = macroId,
                    Year = year,
                    Value = 12.5m,
                    Country = new Country { IdCountry = 1, Name = "Canada", ISOCode = "CA" },
                    MacroIndicator = new MacroIndicator { IdMacroIndicator = macroId, Name = "FDI", Weight = 0.30m, IsHighBetter = true }
                }
            };

            _indicatorRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(entities);

            // Act
            var result = await _service.GetByMacroAndYear(macroId, year);

            // Assert
            Assert.Single(result);
            Assert.Equal(macroId, result[0].IdMacroIndicator);
            Assert.Equal("Canada", result[0].CountryName);
        }
    }
}
