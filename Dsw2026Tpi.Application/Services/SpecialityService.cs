using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;
    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }
    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex,
            s => s.IsActive && (string.IsNullOrWhiteSpace(name) || s.Name.Contains(name)), s => s.Name);

        return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
    }

    public async Task<SpecialityModel.Response> Add(SpecialityModel.Request request)
    {
        Validate(request);
        await CheckNameAvailable(request.Name);

        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        Validate(request);

        var speciality = await _persistence.GetById<Speciality>(id) ??
            throw new EntityNotFoundException(nameof(Speciality));

        await CheckNameAvailable(request.Name, excludeId: id);

        speciality.UpdateData(request.Name, request.Description);
        await _persistence.Update(speciality);

        return new SpecialityModel.Response(speciality.Id, speciality.Name, speciality.Description);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id) ??
            throw new EntityNotFoundException(nameof(Speciality));

        speciality.Deactivate();
        await _persistence.Update(speciality);
    }

    private async Task CheckNameAvailable(string name, Guid? excludeId = null)
    {
        var duplicate = await _persistence.First<Speciality>(s =>
            s.IsActive && s.Name == name && (excludeId == null || s.Id != excludeId));

        if (duplicate is not null)
            throw new ConflictException(nameof(ErrorCodes.SPECIALITY_NAME_TAKEN), ErrorCodes.SPECIALITY_NAME_TAKEN);
    }

    private static void Validate(SpecialityModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException(ErrorCodes.SPECIALITY_INVALID_NAME, nameof(ErrorCodes.SPECIALITY_INVALID_NAME));

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10 || request.Description.Length > 100)
            throw new ValidationException(ErrorCodes.SPECIALITY_INVALID_DESCRIPTION, nameof(ErrorCodes.SPECIALITY_INVALID_DESCRIPTION));
    }
}
