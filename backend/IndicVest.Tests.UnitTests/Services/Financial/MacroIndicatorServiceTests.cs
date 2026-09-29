using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Services.Financial;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Financial
{
    public class MacroIndicatorServiceTests
    {
        private readonly Mock<IMacroIndicatorRepository> _macroIndicatorRepositoryMock;
        private readonly Mock<IIndicatorRepository> _indicatorRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly MacroIndicatorService _service;

        public MacroIndicatorServiceTests()
        {
            _macroIndicatorRepositoryMock = new Mock<IMacroIndicatorRepository>();
            _indicatorRepositoryMock = new Mock<IIndicatorRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new MacroIndicatorService(
                _macroIndicatorRepositoryMock.Object,
                _indicatorRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                Name = "GDP",
                Weight = 0.20m,
                IsHighBetter = true
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidation_WhenTotalWeightIsAlreadyOne()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                Name = "Inflation",
                Weight = 0.10m,
                IsHighBetter = false
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.60m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.60m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldThrowValidation_WhenWeightExceedsLimit()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                Name = "Inflation",
                Weight = 0.30m,
                IsHighBetter = false
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.80m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.80m,
                    IsHighBetter = true
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.AddAsync(dto)
            );
        }

        [Fact]
        public async Task AddAsync_ShouldSucceed_WhenWeightIsWithinLimit()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                Name = "Inflation",
                Weight = 0.20m,
                IsHighBetter = false
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.50m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.50m,
                    IsHighBetter = true
                }
            };

            var entity = new MacroIndicator
            {
                IdMacroIndicator = 2,
                Name = "Inflation",
                Weight = 0.20m,
                IsHighBetter = false
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            _mapperMock
                .Setup(m => m.Map<MacroIndicator>(dto))
                .Returns(entity);

            _macroIndicatorRepositoryMock
                .Setup(r => r.AddAsync(entity))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<MacroIndicatorDto>(entity))
                .Returns(dto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Inflation", result!.Name);
            Assert.Equal(0.20m, result.Weight);

            _macroIndicatorRepositoryMock.Verify(
                r => r.AddAsync(entity),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowConflict_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                IdMacroIndicator = 1,
                Name = "GDP",
                Weight = 0.30m,
                IsHighBetter = true
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "Inflation",
                    Weight = 0.30m,
                    IsHighBetter = false
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "GDP",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "Inflation",
                    Weight = 0.30m,
                    IsHighBetter = false
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "GDP",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _service.UpdateAsync(dto, 1)
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowValidation_WhenWeightExceedsLimit()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                IdMacroIndicator = 1,
                Name = "GDP",
                Weight = 0.70m,
                IsHighBetter = true
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.40m,
                    IsHighBetter = true
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            // otherWeight = 0.40
            // 0.40 + 0.70 = 1.10 -> invalid

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateAsync(dto, 1)
            );
        }

        [Fact]
        public async Task UpdateAsync_ShouldSucceed_WhenWeightIsWithinLimit()
        {
            // Arrange
            var dto = new MacroIndicatorDto
            {
                IdMacroIndicator = 1,
                Name = "GDP",
                Weight = 0.50m,
                IsHighBetter = true
            };

            var existing = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.20m,
                    IsHighBetter = true
                }
            };

            var existingDtos = new List<MacroIndicatorDto>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.30m,
                    IsHighBetter = true
                },
                new()
                {
                    IdMacroIndicator = 2,
                    Name = "Population",
                    Weight = 0.20m,
                    IsHighBetter = true
                }
            };

            var entity = new MacroIndicator
            {
                IdMacroIndicator = 1,
                Name = "GDP",
                Weight = 0.50m,
                IsHighBetter = true
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map<List<MacroIndicatorDto>>(existing))
                .Returns(existingDtos);

            _mapperMock
                .Setup(m => m.Map<MacroIndicator>(dto))
                .Returns(entity);

            _macroIndicatorRepositoryMock
                .Setup(r => r.UpdateAsync(1, entity))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<MacroIndicatorDto>(entity))
                .Returns(dto);

            // Act
            var result = await _service.UpdateAsync(dto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.IdMacroIndicator);
            Assert.Equal(0.50m, result.Weight);

            _macroIndicatorRepositoryMock.Verify(
                r => r.UpdateAsync(1, entity),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllWithIncluded_ShouldMapIndicatorsQuantity()
        {
            // Arrange
            var macroIndicators = new List<MacroIndicator>
            {
                new()
                {
                    IdMacroIndicator = 1,
                    Name = "GDP",
                    Weight = 0.50m,
                    IsHighBetter = true,
                    Indicators = new List<Indicator>
                    {
                        new()
                        {
                            IdCountry = 1,
                            IdMacroIndicator = 1
                        },
                        new()
                        {
                            IdCountry = 2,
                            IdMacroIndicator = 1
                        }
                    }
                }
            };

            _macroIndicatorRepositoryMock
                .Setup(r => r.GetAllListWithIncludeAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(macroIndicators);

            // Act
            var result = await _service.GetAllWithIncluded(
                new List<string> { "Indicators" }
            );

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].IndicatorsQuantity);
            Assert.Equal("GDP", result[0].Name);
            Assert.Equal(0.50m, result[0].Weight);
        }

        [Fact]
        public async Task DeleteAsync_WithCascade_ShouldDeleteIndicatorsBeforeMacroIndicator()
        {
            // Arrange
            const int macroIndicatorId = 1;

            _indicatorRepositoryMock
                .Setup(r => r.DeleteByMacroIndicatorIdAsync(macroIndicatorId))
                .Returns(Task.CompletedTask);

            _macroIndicatorRepositoryMock
                .Setup(r => r.DeleteAsync(macroIndicatorId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(
                macroIndicatorId,
                cascade: true
            );

            // Assert
            Assert.True(result);

            _indicatorRepositoryMock.Verify(
                r => r.DeleteByMacroIndicatorIdAsync(macroIndicatorId),
                Times.Once
            );

            _macroIndicatorRepositoryMock.Verify(
                r => r.DeleteAsync(macroIndicatorId),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_WithoutCascade_ShouldNotDeleteIndicators()
        {
            // Arrange
            const int macroIndicatorId = 1;

            _macroIndicatorRepositoryMock
                .Setup(r => r.DeleteAsync(macroIndicatorId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(
                macroIndicatorId,
                cascade: false
            );

            // Assert
            Assert.True(result);

            _indicatorRepositoryMock.Verify(
                r => r.DeleteByMacroIndicatorIdAsync(It.IsAny<int>()),
                Times.Never
            );

            _macroIndicatorRepositoryMock.Verify(
                r => r.DeleteAsync(macroIndicatorId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetDependentsAsync_ShouldReturnCountAndDistinctYears()
        {
            // Arrange
            const int macroIndicatorId = 1;

            var indicators = new List<Indicator>
            {
                new()
                {
                    IdCountry = 1,
                    IdMacroIndicator = macroIndicatorId,
                    Year = 2024
                },
                new()
                {
                    IdCountry = 2,
                    IdMacroIndicator = macroIndicatorId,
                    Year = 2023
                },
                new()
                {
                    IdCountry = 3,
                    IdMacroIndicator = macroIndicatorId,
                    Year = 2024
                },
                new()
                {
                    IdCountry = 4,
                    IdMacroIndicator = macroIndicatorId,
                    Year = 2022
                }
            };

            _indicatorRepositoryMock
                .Setup(r => r.GetByMacroIndicatorIdAsync(macroIndicatorId))
                .ReturnsAsync(indicators);

            // Act
            var result = await _service.GetDependentsAsync(macroIndicatorId);

            // Assert
            Assert.Equal(4, result.IndicatorCount);
            Assert.Equal(
                new[] { 2022, 2023, 2024 },
                result.Years
            );
        }
    }
}
