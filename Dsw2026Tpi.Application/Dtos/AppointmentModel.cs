namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);
    public record PatientDto(string Dni);
    public record Response(Guid Id, string DoctorName, string Specialty, DateOnly Date, TimeOnly Time, string Status);


    public record SearchResponse(Guid AppointmentsId, string AppointmentsStatus, PatientSummary Patient, DoctorSummary Doctor);
    public record PatientSummary(long Dni, string FullName);
    public record DoctorSummary(Guid DoctorId, string Name, SpecialtySummary Specialty);
    public record SpecialtySummary(Guid SpecialtyId, string Name);
}
