using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Services.Financial;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Financial
{
    public class ReturnRateServiceTests
    {
        private readonly Mock<IReturnRateRepository> _returnRateRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly ReturnRateService _service;

        public ReturnRateServiceTests()
        {
            _returnRateRepositoryMock = new Mock<IReturnRateRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new ReturnRateService(
                _returnRateRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetAll_ShouldReturnExistingConfig_WhenConfigIsValid()
        {
            // Arrange
            var entity = new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.20m
            };

            var dto = new ReturnRateDto
            {
                IdReturnRate = 1,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.20m
            };

            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate> { entity });

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(
                    It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto> { dto });

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].IdReturnRate);
            Assert.Equal(0.05m, result[0].MinReturnRate);
            Assert.Equal(0.20m, result[0].MaxReturnRate);
        }

        [Fact]
        public async Task GetAll_ShouldReturnDefaultConfig_WhenConfigDoesNotExist()
        {
            // Arrange
            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate>());

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(
                    It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto>());

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Single(result);
            Assert.Equal(0, result[0].IdReturnRate);
            Assert.Equal(0.02m, result[0].MinReturnRate);
            Assert.Equal(0.15m, result[0].MaxReturnRate);
        }

        [Fact]
        public async Task GetAll_ShouldReturnDefaultConfig_WhenMinRateIsNegative()
        {
            // Arrange
            var entity = new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = -0.05m,
                MaxReturnRate = 0.20m
            };

            var dto = new ReturnRateDto
            {
                IdReturnRate = 1,
                MinReturnRate = -0.05m,
                MaxReturnRate = 0.20m
            };

            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate> { entity });

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(
                    It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto> { dto });

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Single(result);

            Assert.Equal(1, result[0].IdReturnRate);

            Assert.Equal(0.02m, result[0].MinReturnRate);
            Assert.Equal(0.15m, result[0].MaxReturnRate);
        }

        [Fact]
        public async Task GetAll_ShouldReturnDefaultConfig_WhenMaxRateIsLessThanOrEqualToMinRate()
        {
            // Arrange
            var entity = new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = 0.15m,
                MaxReturnRate = 0.10m
            };

            var dto = new ReturnRateDto
            {
                IdReturnRate = 1,
                MinReturnRate = 0.15m,
                MaxReturnRate = 0.10m
            };

            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate> { entity });

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(
                    It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto> { dto });

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].IdReturnRate);
            Assert.Equal(0.02m, result[0].MinReturnRate);
            Assert.Equal(0.15m, result[0].MaxReturnRate);
        }

        [Fact]
        public async Task UpdateConfigAsync_ShouldThrowValidation_WhenMinRateIsGreaterThanMaxRate()
        {
            // Arrange
            const decimal minRate = 0.20m;
            const decimal maxRate = 0.10m;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateConfigAsync(minRate, maxRate)
            );
        }

        [Fact]
        public async Task UpdateConfigAsync_ShouldThrowValidation_WhenMinRateEqualsMaxRate()
        {
            // Arrange
            const decimal minRate = 0.10m;
            const decimal maxRate = 0.10m;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.UpdateConfigAsync(minRate, maxRate)
            );
        }

        [Fact]
        public async Task UpdateConfigAsync_ShouldUpdateDefaultConfig_WhenConfigDoesNotExistInDb()
        {
            // Arrange
            var updatedEntity = new ReturnRate
            {
                IdReturnRate = 0,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.20m
            };

            var updatedDto = new ReturnRateDto
            {
                IdReturnRate = 0,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.20m
            };

            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate>());

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto>());

            _mapperMock
                .Setup(m => m.Map<ReturnRate>(It.Is<ReturnRateDto>(d => d.IdReturnRate == 0 && d.MinReturnRate == 0.05m && d.MaxReturnRate == 0.20m)))
                .Returns(updatedEntity);

            _returnRateRepositoryMock
                .Setup(r => r.UpdateAsync(0, updatedEntity))
                .ReturnsAsync(updatedEntity);

            _mapperMock
                .Setup(m => m.Map<ReturnRateDto>(updatedEntity))
                .Returns(updatedDto);

            // Act
            var result = await _service.UpdateConfigAsync(0.05m, 0.20m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.IdReturnRate);
            Assert.Equal(0.05m, result.MinReturnRate);
            Assert.Equal(0.20m, result.MaxReturnRate);

            _returnRateRepositoryMock.Verify(
                r => r.UpdateAsync(0, It.IsAny<ReturnRate>()),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateConfigAsync_ShouldUpdateConfig_WhenValuesAreValid()
        {
            // Arrange
            var existingEntity = new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = 0.02m,
                MaxReturnRate = 0.15m
            };

            var existingDto = new ReturnRateDto
            {
                IdReturnRate = 1,
                MinReturnRate = 0.02m,
                MaxReturnRate = 0.15m
            };

            var updatedDto = new ReturnRateDto
            {
                IdReturnRate = 1,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.25m
            };

            var updatedEntity = new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = 0.05m,
                MaxReturnRate = 0.25m
            };

            _returnRateRepositoryMock
                .Setup(r => r.GetAllListAsync())
                .ReturnsAsync(new List<ReturnRate> { existingEntity });

            _mapperMock
                .Setup(m => m.Map<List<ReturnRateDto>>(
                    It.IsAny<List<ReturnRate>>()))
                .Returns(new List<ReturnRateDto> { existingDto });

            _mapperMock
                .Setup(m => m.Map<ReturnRate>(
                    It.Is<ReturnRateDto>(dto =>
                        dto.IdReturnRate == 1 &&
                        dto.MinReturnRate == 0.05m &&
                        dto.MaxReturnRate == 0.25m)))
                .Returns(updatedEntity);

            _returnRateRepositoryMock
                .Setup(r => r.UpdateAsync(1, updatedEntity))
                .ReturnsAsync(updatedEntity);

            _mapperMock
                .Setup(m => m.Map<ReturnRateDto>(updatedEntity))
                .Returns(updatedDto);

            // Act
            var result = await _service.UpdateConfigAsync(
                0.05m,
                0.25m
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.IdReturnRate);
            Assert.Equal(0.05m, result.MinReturnRate);
            Assert.Equal(0.25m, result.MaxReturnRate);

            _returnRateRepositoryMock.Verify(
                r => r.UpdateAsync(1, updatedEntity),
                Times.Once
            );
        }
    }
}
