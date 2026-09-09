using AutoMapper;
using IndicVest.Core.Application.Interfaces.Base;
using IndicVest.Core.Domain.Interfaces.Base;

namespace IndicVest.Core.Application.Services.Base
{
    public class GenericService<Entity, DtoModel> : IGenericService<DtoModel>
        where Entity : class
        where DtoModel : class
    {
        protected readonly IGenericRepository<Entity> _repository;
        protected readonly IMapper _mapper;

        public GenericService(IGenericRepository<Entity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<DtoModel?> AddAsync(DtoModel dto)
        {
            var entity = _mapper.Map<Entity>(dto);
            var result = await _repository.AddAsync(entity);
            return result is null ? null : _mapper.Map<DtoModel>(result);
        }

        public virtual async Task<DtoModel?> UpdateAsync(DtoModel dto, int id)
        {
            var entity = _mapper.Map<Entity>(dto);
            var result = await _repository.UpdateAsync(id, entity);
            return result is null ? null : _mapper.Map<DtoModel>(result);
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            return true;
        }

        public virtual async Task<DtoModel?> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity is null ? null : _mapper.Map<DtoModel>(entity);
        }

        public virtual async Task<List<DtoModel>> GetAll()
        {
            var entities = await _repository.GetAllListAsync();
            return _mapper.Map<List<DtoModel>>(entities);
        }

        public virtual async Task<List<DtoModel>> GetAllWithIncluded(List<string> properties)
        {
            var entities = await _repository.GetAllListWithIncludeAsync(properties);
            return _mapper.Map<List<DtoModel>>(entities);
        }
    }
}
