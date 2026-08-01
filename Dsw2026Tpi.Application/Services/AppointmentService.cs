using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService (IPersistence persistence, ILogger<AppointmentService> logger)
    {
        _persistence = persistence;
        _logger=logger;
    }

    public async Task<AppointmentModel.Response> BookAppointment(AppointmentModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Patient.Dni) ||
            request.Patient.Dni.Length < 7 || request.Patient.Dni.Length > 10)
            throw new ValidationException(ErrorCodes.APPOINTMENT_INVALID_DNI, nameof(ErrorCodes.APPOINTMENT_INVALID_DNI));

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
            throw new ValidationException(ErrorCodes.INVALID_REASON, nameof(ErrorCodes.INVALID_REASON));

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId, nameof(Doctor.Specialty)) ?? 
            throw new EntityNotFoundException(nameof(Doctor));

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni) ?? 
            throw new EntityNotFoundException(nameof(Patient));

        if (request.AvailabilityId == Guid.Empty)
            throw new ValidationException(ErrorCodes.INVALID_AVAILABILITY_ID, nameof(ErrorCodes.INVALID_AVAILABILITY_ID));

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId) ?? 
            throw new EntityNotFoundException(nameof(AvailabilitySlot));

        if (slot.DoctorId != doctor.Id)
            throw new ValidationException(ErrorCodes.SLOT_DOCTOR_MISMATCH, nameof(ErrorCodes.SLOT_DOCTOR_MISMATCH));

        var slotDateTime = slot.SlotDate.ToDateTime(slot.StartTime);
        if (slotDateTime < DateTime.Now)
            throw new ValidationException(ErrorCodes.APPOINTMENT_IN_PAST, nameof(ErrorCodes.APPOINTMENT_IN_PAST));
        try
        {
            slot.Book();
        }
        catch(ConflictException e)
        {
            _logger.LogWarning(e, "intento de reserva sobre un turno ya no disponible. DNI {Dni}.", request.Patient.Dni);throw;
        }
        
        var appointment = new Appointment(slot.Id, patient.Id, request.Reason);

        try
        {
            await _persistence.Add(appointment);

        }
        catch (DbUpdateException e)
        {
            _logger.LogWarning(e, "ocurrió un conflicto de concurrencia al reservar turno para el DNI {Dni}.", request.Patient.Dni);
            throw new ConflictException(nameof(ErrorCodes.APPOINTMENT_CONFLICT), ErrorCodes.APPOINTMENT_CONFLICT);

        }
        await _persistence.Update(slot);
        _logger.LogInformation("el turno {AppointmentId} se reservó exitosamente para el DNI {Dni}", appointment.Id, request.Patient.Dni);
        return MapToResponse(appointment, slot, doctor);
    }

    public async Task<AppointmentModel.Response> CancelAppointment(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id,
                nameof(Appointment.AvailabilitySlot),$"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}",
                $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}.{nameof(Doctor.Specialty)}") ?? 
                throw new EntityNotFoundException(nameof(Appointment));

        try
        {
            appointment.Cancel();
        }catch(ConflictException e)
        {
            _logger.LogWarning(e, "intento de cancelar un turno que no está en estado 'Reservada'. Cita: {AppointmentId}.", id);throw;
        }
                                
        appointment.AvailabilitySlot!.Release();

        await _persistence.Update(appointment);
        await _persistence.Update(appointment.AvailabilitySlot);

        _logger.LogInformation("el turno {AppointmentId} se canceló exitosamente", id);

        return MapToResponse(appointment, appointment.AvailabilitySlot, appointment.AvailabilitySlot.Doctor!);
    }

    public async Task<Pagination<AppointmentModel.Response>> GetPatientAppointments (string dni,int pageSize, int pageIndex)
    {
        var patient = await _persistence.First<Patient>(p => p.Dni == dni) ?? 
            throw new EntityNotFoundException(nameof(Patient));

        var result = await _persistence.Paginate<Appointment, DateOnly>(pageSize, pageIndex,
            a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked,
            a => a.AvailabilitySlot!.SlotDate,
            nameof(Appointment.AvailabilitySlot),
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}",
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}.{nameof(Doctor.Specialty)}");

        return result.Map(a => MapToResponse(a, a.AvailabilitySlot!, a.AvailabilitySlot!.Doctor!));
    }

    public async Task<Pagination<AppointmentModel.Response>>GetAppointmentsByDate (DateOnly date, int pageSize,int pageIndex)
    {
        var result = await _persistence.Paginate<Appointment, TimeOnly>(pageSize, pageIndex,
            a =>a.AvailabilitySlot!.SlotDate == date,
            a=> a.AvailabilitySlot!.StartTime,
            nameof(Appointment.AvailabilitySlot),
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}",
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}.{nameof(Doctor.Specialty)}",
            nameof(Appointment.Patient));

        return result.Map(a => MapToResponse(a, a.AvailabilitySlot!, a.AvailabilitySlot!.Doctor!));
    }

    public async Task<Pagination<AppointmentModel.Response>> SearchAppointments(Guid? specialtyId, Guid? doctorId, string? dni, 
                                                                                DateOnly? date, int pageSize, int pageIndex)
    {
        Guid? patientId = null;
        if (!string.IsNullOrWhiteSpace(dni))
        {
            var patient = await _persistence.First<Patient>(p => p.Dni == dni) ?? 
                throw new EntityNotFoundException(nameof(Patient));
            patientId = patient.Id;
        }

        var result = await _persistence.Paginate<Appointment, DateOnly> (pageSize, pageIndex,
            a =>(!doctorId.HasValue ||a.AvailabilitySlot!.DoctorId == doctorId) &&
                (!specialtyId.HasValue || a.AvailabilitySlot!.Doctor!.SpecialtyId== specialtyId) &&
                (!patientId.HasValue || a.PatientId ==patientId) &&
                (!date.HasValue || a.AvailabilitySlot!.SlotDate == date),
            a =>a.AvailabilitySlot!.SlotDate,
            nameof(Appointment.AvailabilitySlot),
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}",
            $"{nameof(Appointment.AvailabilitySlot)}.{nameof(AvailabilitySlot.Doctor)}.{nameof(Doctor.Specialty)}");

        return result.Map (a => MapToResponse(a, a.AvailabilitySlot!, a.AvailabilitySlot!.Doctor!));
    }

    private static AppointmentModel.Response MapToResponse(Appointment appointment, AvailabilitySlot slot, Doctor doctor)
        =>new( appointment.Id,doctor.Name, doctor.Specialty?.Name ?? string.Empty,slot.SlotDate, slot.StartTime, appointment.Status.ToString());
}