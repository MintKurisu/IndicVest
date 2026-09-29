using AutoMapper;
using FluentAssertions;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Interfaces.Base;
using IndicVest.Tests.UnitTests.TestDoubles;
using Moq;

namespace IndicVest.Tests.UnitTests.Services.Base
{
    public class GenericServiceTests
    {
        private readonly Mock<IGenericRepository<TestEntity>> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GenericService<TestEntity, TestDto> _sut;

        public GenericServiceTests()
        {
            _repositoryMock = new Mock<IGenericRepository<TestEntity>>();
            _mapperMock = new Mock<IMapper>();

            _sut = new GenericService<TestEntity, TestDto>(
                _repositoryMock.Object,
                _mapperMock.Object
            );
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ShouldReturnAllEntities()
        {
            var entities = new List<TestEntity>
            {
                new() { Id = 1, Name = "Entity 1", Description = "Description 1" },
                new() { Id = 2, Name = "Entity 2", Description = "Description 2" }
            };

            _repositoryMock.Setup(r => r.GetAllListAsync()).ReturnsAsync(entities);

            var dtos = new List<TestDto>
            {
                new() { Id = 1, Name = "Entity 1", Description = "Description 1" },
                new() { Id = 2, Name = "Entity 2", Description = "Description 2" }
            };

            _mapperMock.Setup(m => m.Map<List<TestDto>>(entities)).Returns(dtos);

            var result = await _sut.GetAll();

            result.Should().BeEquivalentTo(dtos);
            _repositoryMock.Verify(r => r.GetAllListAsync(), Times.Once);
        }

        #endregion

        #region GetAllWithIncluded Tests

        [Fact]
        public async Task GetAllWithIncluded_ShouldPassPropertiesToRepository()
        {
            var properties = new List<string> { "SomeNavProp" };
            var entities = new List<TestEntity> { new() { Id = 1, Name = "Entity 1", Description = "Description 1" } };

            _repositoryMock.Setup(r => r.GetAllListWithIncludeAsync(properties)).ReturnsAsync(entities);

            var dtos = new List<TestDto> { new() { Id = 1, Name = "Entity 1", Description = "Description 1" } };
            _mapperMock.Setup(m => m.Map<List<TestDto>>(entities)).Returns(dtos);

            var result = await _sut.GetAllWithIncluded(properties);

            result.Should().BeEquivalentTo(dtos);
            _repositoryMock.Verify(r => r.GetAllListWithIncludeAsync(properties), Times.Once);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WhenEntityExists_ShouldReturnDto()
        {
            var entityId = 1;
            var entity = new TestEntity { Id = entityId, Name = "Test Entity", Description = "Test Description" };

            _repositoryMock.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);

            var dto = new TestDto { Id = entityId, Name = "Test Entity", Description = "Test Description" };
            _mapperMock.Setup(m => m.Map<TestDto>(entity)).Returns(dto);

            var result = await _sut.GetById(entityId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(entityId);
            _repositoryMock.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenEntityDoesNotExist_ShouldReturnNull()
        {
            var entityId = 999;
            _repositoryMock.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync((TestEntity?)null);

            var result = await _sut.GetById(entityId);

            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<TestDto>(It.IsAny<TestEntity>()), Times.Never);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WhenValid_ShouldAddAndReturnDto()
        {
            var inputDto = new TestDto { Name = "New Entity", Description = "New Description" };
            var entity = new TestEntity { Name = "New Entity", Description = "New Description" };
            var createdEntity = new TestEntity { Id = 1, Name = "New Entity", Description = "New Description" };
            var resultDto = new TestDto { Id = 1, Name = "New Entity", Description = "New Description" };

            _mapperMock.Setup(m => m.Map<TestEntity>(inputDto)).Returns(entity);
            _repositoryMock.Setup(r => r.AddAsync(entity)).ReturnsAsync(createdEntity);
            _mapperMock.Setup(m => m.Map<TestDto>(createdEntity)).Returns(resultDto);

            var result = await _sut.AddAsync(inputDto);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            _repositoryMock.Verify(r => r.AddAsync(entity), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryReturnsNull_ShouldReturnNull()
        {
            var inputDto = new TestDto { Name = "New Entity", Description = "New Description" };
            var entity = new TestEntity { Name = "New Entity", Description = "New Description" };

            _mapperMock.Setup(m => m.Map<TestEntity>(inputDto)).Returns(entity);
            _repositoryMock.Setup(r => r.AddAsync(entity)).ReturnsAsync((TestEntity?)null);

            var result = await _sut.AddAsync(inputDto);

            result.Should().BeNull();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenValid_ShouldUpdateAndReturnDto()
        {
            var entityId = 1;
            var inputDto = new TestDto { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };
            var entity = new TestEntity { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };
            var updatedEntity = new TestEntity { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };
            var resultDto = new TestDto { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };

            _mapperMock.Setup(m => m.Map<TestEntity>(inputDto)).Returns(entity);
            _repositoryMock.Setup(r => r.UpdateAsync(entityId, entity)).ReturnsAsync(updatedEntity);
            _mapperMock.Setup(m => m.Map<TestDto>(updatedEntity)).Returns(resultDto);

            var result = await _sut.UpdateAsync(inputDto, entityId);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Entity");
            _repositoryMock.Verify(r => r.UpdateAsync(entityId, entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenEntityDoesNotExist_ShouldReturnNull()
        {
            var entityId = 999;
            var inputDto = new TestDto { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };
            var entity = new TestEntity { Id = entityId, Name = "Updated Entity", Description = "Updated Description" };

            _mapperMock.Setup(m => m.Map<TestEntity>(inputDto)).Returns(entity);
            _repositoryMock.Setup(r => r.UpdateAsync(entityId, entity)).ReturnsAsync((TestEntity?)null);

            var result = await _sut.UpdateAsync(inputDto, entityId);

            result.Should().BeNull();
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryAndReturnTrue()
        {
            var entityId = 1;
            _repositoryMock.Setup(r => r.DeleteAsync(entityId)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(entityId);

            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(entityId), Times.Once);
        }

        #endregion
    }
}
