using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly IPersistence _persistence;
    public SpecialtyService(IPersistence persistence)
    {
        _persistence = persistence;
    }
    public async Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        if (!string.IsNullOrWhiteSpace(name) && (name.Length < 3 || name.Length > 100))
            throw new ValidationException(ErrorCodes.SPECIALTY_INVALID_NAME, nameof(ErrorCodes.SPECIALTY_INVALID_NAME));

        var specialties = await _persistence.Paginate<Specialty, string>(pageSize, pageIndex,
            s => !s.Deleted && (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)), s => s.Name);

        return specialties.Map(s => new SpecialtyModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialtyModel.Response> Add(SpecialtyModel.Request request)
    {
        Validate(request);
        await CheckNameAvailable(request.Name);

        var specialty = new Specialty(request.Name, request.Description);
        await _persistence.Add(specialty);

        return new SpecialtyModel.Response(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task<SpecialtyModel.Response> Update(Guid id, SpecialtyModel.Request request)
    {
        Validate(request);

        var specialty = await _persistence.GetById<Specialty>(id) ??
            throw new EntityNotFoundException(nameof(Specialty));

        await CheckNameAvailable(request.Name, excludeId: id);

        specialty.UpdateData(request.Name, request.Description);
        await _persistence.Update(specialty);

        return new SpecialtyModel.Response(specialty.Id, specialty.Name, specialty.Description);
    }

    public async Task Delete(Guid id)
    {
        var specialty = await _persistence.GetById<Specialty>(id) ??
            throw new EntityNotFoundException(nameof(Specialty));
        specialty.SoftDelete();

        await _persistence.Update(specialty);
    }

    private async Task CheckNameAvailable(string name, Guid? excludeId = null)
    {
        var duplicate = await _persistence.First<Specialty>(s =>
            !s.Deleted && s.Name == name && (excludeId == null || s.Id != excludeId));
        if (duplicate is not null)
            throw new ConflictException(nameof(ErrorCodes.SPECIALTY_NAME_TAKEN), ErrorCodes.SPECIALTY_NAME_TAKEN);
    }

    private static void Validate(SpecialtyModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException(ErrorCodes.SPECIALTY_INVALID_NAME, nameof(ErrorCodes.SPECIALTY_INVALID_NAME));

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
            throw new ValidationException(ErrorCodes.SPECIALTY_INVALID_DESCRIPTION, nameof(ErrorCodes.SPECIALTY_INVALID_DESCRIPTION));
    }
}
