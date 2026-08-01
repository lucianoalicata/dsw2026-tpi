using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface ISpecialtyService
{
    Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
    Task<SpecialtyModel.Response> Add(SpecialtyModel.Request request);
    Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request);
    Task Delete(Guid id);
}
