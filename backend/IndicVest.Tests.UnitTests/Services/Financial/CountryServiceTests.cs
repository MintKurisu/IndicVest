using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Services.Financial;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Financial
{
    public class CountryServiceTests
    {
        private readonly Mock<ICountryRepository> _countryRepositoryMock;
        private readonly Mock<IIndicatorRepository> _indicatorRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CountryService _service;

        public CountryServiceTests()
        {
            _countryRepositoryMock = new Mock<ICountryRepository>();
            _indicatorRepositoryMock = new Mock<IIndicatorRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new CountryService(
                _countryRepositoryMock.Object,
                _indicatorRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = new CountryDto
            {
                IdCountry = 0,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            var existingCountries = new List<CountryDto>
            {
                new()
                {
                    IdCountry = 1,
                    Name = "Dominican Republic",
                    ISOCode = "DO"
                }
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<Country>());

            _mapperMock
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<List<Country>>()))
                .Returns(existingCountries);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowConflict_WhenIsoCodeAlreadyExists()
        {
            // Arrange
            var dto = new CountryDto
            {
                IdCountry = 0,
                Name = "Another Country",
                ISOCode = "DO"
            };

            var existingCountries = new List<CountryDto>
            {
                new()
                {
                    IdCountry = 1,
                    Name = "Dominican Republic",
                    ISOCode = "DO"
                }
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<Country>());

            _mapperMock
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<List<Country>>()))
                .Returns(existingCountries);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldSucceed_WhenCountryIsUnique()
        {
            // Arrange
            var dto = new CountryDto
            {
                IdCountry = 0,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            var entity = new Country
            {
                IdCountry = 1,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<Country>());

            // AGREGAR ESTA LÍNEA:
            _mapperMock
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<List<Country>>()))
                .Returns(new List<CountryDto>());

            _mapperMock
                .Setup(m => m.Map<Country>(dto))
                .Returns(entity);

            _countryRepositoryMock
                .Setup(r => r.AddAsync(entity))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<CountryDto>(entity))
                .Returns(dto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result!.Name);
            Assert.Equal(dto.ISOCode, result.ISOCode);

            _countryRepositoryMock.Verify(
                r => r.AddAsync(entity),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = new CountryDto
            {
                IdCountry = 1,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            var existingCountries = new List<CountryDto>
            {
                new()
                {
                    IdCountry = 1,
                    Name = "Old Name",
                    ISOCode = "DO"
                },
                new()
                {
                    IdCountry = 2,
                    Name = "Dominican Republic",
                    ISOCode = "US"
                }
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<Country>());

            _mapperMock
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<List<Country>>()))
                .Returns(existingCountries);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.UpdateAsync(dto, 1)
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldSucceed_WhenUpdatingOwnCountry()
        {
            // Arrange
            var dto = new CountryDto
            {
                IdCountry = 1,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            var entity = new Country
            {
                IdCountry = 1,
                Name = "Dominican Republic",
                ISOCode = "DO"
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<Country>());

            // AGREGAR ESTA LÍNEA:
            _mapperMock
                .Setup(m => m.Map<List<CountryDto>>(It.IsAny<List<Country>>()))
                .Returns(new List<CountryDto>());

            _mapperMock
                .Setup(m => m.Map<Country>(dto))
                .Returns(entity);

            _countryRepositoryMock
                .Setup(r => r.UpdateAsync(1, entity))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<CountryDto>(entity))
                .Returns(dto);

            // Act
            var result = await _service.UpdateAsync(dto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.IdCountry);

            _countryRepositoryMock.Verify(
                r => r.UpdateAsync(1, entity),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllWithIncluded_ShouldMapIndicatorsQuantity()
        {
            // Arrange
            var countries = new List<Country>
            {
                new()
                {
                    IdCountry = 1,
                    Name = "Dominican Republic",
                    ISOCode = "DO",
                    Indicators = new List<Indicator>
                    {
                        new()
                        {
                            IdCountry = 1,
                            IdMacroIndicator = 1
                        },
                        new()
                        {
                            IdCountry = 1,
                            IdMacroIndicator = 2
                        },
                        new()
                        {
                            IdCountry = 1,
                            IdMacroIndicator = 3
                        }
                    }
                }
            };

            _countryRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(countries);

            // Act
            var result = await _service.GetAllWithIncluded(
                new List<string> { "Indicators" }
            );

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].IndicatorsQuantity);
            Assert.Equal("Dominican Republic", result[0].Name);
            Assert.Equal("DO", result[0].ISOCode);
        }

        [Fact]
        public async Task DeleteAsync_WithCascade_ShouldDeleteIndicatorsBeforeCountry()
        {
            // Arrange
            const int countryId = 1;

            _indicatorRepositoryMock
                .Setup(r => r.DeleteByCountryIdAsync(countryId))
                .Returns(Task.CompletedTask);

            _countryRepositoryMock
                .Setup(r => r.DeleteAsync(countryId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(countryId, cascade: true);

            // Assert
            Assert.True(result);

            _indicatorRepositoryMock.Verify(
                r => r.DeleteByCountryIdAsync(countryId),
                Times.Once
            );

            _countryRepositoryMock.Verify(
                r => r.DeleteAsync(countryId),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_WithoutCascade_ShouldNotDeleteIndicators()
        {
            // Arrange
            const int countryId = 1;

            _countryRepositoryMock
                .Setup(r => r.DeleteAsync(countryId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(countryId, cascade: false);

            // Assert
            Assert.True(result);

            _indicatorRepositoryMock.Verify(
                r => r.DeleteByCountryIdAsync(It.IsAny<int>()),
                Times.Never
            );

            _countryRepositoryMock.Verify(
                r => r.DeleteAsync(countryId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetDependentsAsync_ShouldReturnCountAndDistinctYears()
        {
            // Arrange
            const int countryId = 1;

            var indicators = new List<Indicator>
            {
                new()
                {
                    IdCountry = countryId,
                    IdMacroIndicator = 1,
                    Year = 2024
                },
                new()
                {
                    IdCountry = countryId,
                    IdMacroIndicator = 2,
                    Year = 2023
                },
                new()
                {
                    IdCountry = countryId,
                    IdMacroIndicator = 1,
                    Year = 2024
                },
                new()
                {
                    IdCountry = countryId,
                    IdMacroIndicator = 3,
                    Year = 2022
                }
            };

            _indicatorRepositoryMock
                .Setup(r => r.GetByCountryIdAsync(countryId))
                .ReturnsAsync(indicators);

            // Act
            var result = await _service.GetDependentsAsync(countryId);

            // Assert
            Assert.Equal(4, result.IndicatorCount);
            Assert.Equal(
                new[] { 2022, 2023, 2024 },
                result.Years
            );
        }
    }
}
