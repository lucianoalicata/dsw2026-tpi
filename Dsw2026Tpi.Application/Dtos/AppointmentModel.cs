namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid DoctorId, Guid AvailabilityId, PatientDto Patient, string Reason);
    public record PatientDto(string Dni);
    public record Response(Guid Id, string DoctorName, string Speciality, DateOnly Date, TimeOnly Time, string Status);
}
