using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => d.IsActive &&
                                           (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> Add(DoctorModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException(ErrorCodes.DOCTOR_INVALID_NAME, nameof(ErrorCodes.DOCTOR_INVALID_NAME));

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ValidationException(ErrorCodes.DOCTOR_INVALID_LICENSE, nameof(ErrorCodes.DOCTOR_INVALID_LICENSE));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId) ??
            throw new EntityNotFoundException(nameof(Speciality));

        var doctor = new Doctor(request.Name, request.LicenseNumber, speciality);

        await _persistence.Add(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            throw new ValidationException(ErrorCodes.DOCTOR_INVALID_NAME, nameof(ErrorCodes.DOCTOR_INVALID_NAME));

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ValidationException(ErrorCodes.DOCTOR_INVALID_LICENSE, nameof(ErrorCodes.DOCTOR_INVALID_LICENSE));

        var doctor = await _persistence.GetById<Doctor>(id) ??
            throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId) ??
            throw new EntityNotFoundException(nameof(Speciality));

        doctor.UpdateData(request.Name, request.LicenseNumber, speciality);

        await _persistence.Update(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name)); }

        public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id) ??
            throw new EntityNotFoundException(nameof(Doctor));

        doctor.Deactivate();

        await _persistence.Update(doctor);
    }

    private static readonly string[] DayNames = { "DOMINGO", "LUNES", "MARTES", "MIÉRCOLES", "JUEVES", "VIERNES", "SÁBADO" };

    public async Task<IEnumerable<DoctorModel.AvailabilityResponse>> GetAvailabilities(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id) ??
            throw new EntityNotFoundException(nameof(Doctor));

        var today = DateTime.UtcNow;

        var rules = await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == id && !r.Deleted && r.Month == today.Month && r.Year == today.Year);

        return rules?.Select(r => new DoctorModel.AvailabilityResponse(
            DayNames[r.DayOfWeek],
            r.StartTime.ToString("HH:mm"),
            r.EndTime.ToString("HH:mm")))
            ?? Enumerable.Empty<DoctorModel.AvailabilityResponse>();
    }
}


