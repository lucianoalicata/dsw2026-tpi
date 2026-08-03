using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AvailabilityService> _logger;

    private static readonly string[] DayNames =
        { "DOMINGO", "LUNES", "MARTES", "MIÉRCOLES", "JUEVES", "VIERNES", "SÁBADO" };

    
    private static readonly HashSet<DateOnly> Feriados = CargarFeriados();
    private static HashSet<DateOnly> CargarFeriados()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Sources", "Feriados.json");

        if (!File.Exists(path))
            return new HashSet<DateOnly>();

        var json = File.ReadAllText(path);
        var fechas = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

        return fechas.Select(DateOnly.Parse).ToHashSet();
    }

    public AvailabilityService(IPersistence persistence, ILogger<AvailabilityService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    private record ParsedDay(int DayOfWeek, TimeOnly Start, TimeOnly End);

    private List<ParsedDay> ParseAndValidateDays(List<AvailabilityModel.DayRequest> days)
    {
        if (days is null || days.Count == 0)
            throw new ValidationException(ErrorCodes.AVAILABILITY_EMPTY_DAYS, nameof(ErrorCodes.AVAILABILITY_EMPTY_DAYS));

        var parsed = new List<ParsedDay>();

        foreach (var d in days)
        {
            var dayIndex = Array.FindIndex(DayNames,
                n => string.Equals(Quitar_acentos(n), Quitar_acentos(d.Day?.Trim() ?? ""), StringComparison.OrdinalIgnoreCase));

            if (dayIndex == -1)
                throw new ValidationException(ErrorCodes.AVAILABILITY_INVALID_DAY, nameof(ErrorCodes.AVAILABILITY_INVALID_DAY));

            if (!TimeOnly.TryParse(d.StartTime, out var start) || !TimeOnly.TryParse(d.EndTime, out var end))
                throw new ValidationException(ErrorCodes.AVAILABILITY_INVALID_TIME_RANGE, nameof(ErrorCodes.AVAILABILITY_INVALID_TIME_RANGE));

            if (start >= end)
                throw new ValidationException(ErrorCodes.AVAILABILITY_INVALID_TIME_RANGE, nameof(ErrorCodes.AVAILABILITY_INVALID_TIME_RANGE));

            parsed.Add(new ParsedDay(dayIndex, start, end));
        }

        // Solapamientos DENTRO del mismo payload (ej: mandan Lunes dos veces con horarios que se cruzan)
        foreach (var group in parsed.GroupBy(p => p.DayOfWeek))
        {
            var ordered = group.OrderBy(p => p.Start).ToList();
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                if (ordered[i].End > ordered[i + 1].Start)
                    throw new ValidationException(ErrorCodes.AVAILABILITY_OVERLAP, nameof(ErrorCodes.AVAILABILITY_OVERLAP));
            }
        }

        return parsed;
    }

    private static string Quitar_acentos(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray());
    }

    private List<AvailabilitySlot> GenerateSlotsForRule(AvailabilityRule rule, DateOnly today, DateOnly monthEnd)
    {
        var slots = new List<AvailabilitySlot>();
        var date = today;
        var huboAlgunaOcurrencia = false;

        while (date <= monthEnd)
        {
            if ((int)date.DayOfWeek == rule.DayOfWeek)
            {
                huboAlgunaOcurrencia = true;

                if (Feriados.Contains(date))
                {
                    _logger.LogInformation("Se omite el {Fecha} para el médico {DoctorId} por ser feriado.", date, rule.DoctorId);
                }
                else
                {
                    var slotStart = rule.StartTime;
                    while (slotStart.AddMinutes(30) <= rule.EndTime)
                    {
                        var slotEnd = slotStart.AddMinutes(30);
                        slots.Add(new AvailabilitySlot(rule.DoctorId, rule.Id, date, slotStart, slotEnd));
                        slotStart = slotEnd;
                    }
                }
            }
            date = date.AddDays(1);
        }

        if (!huboAlgunaOcurrencia)
        {
            _logger.LogInformation("No quedan más días {Dia} en lo que resta del mes para el médico {DoctorId}.", DayNames[rule.DayOfWeek], rule.DoctorId);
        }

        return slots;
    }

    public async Task<AvailabilityModel.Response> Add(AvailabilityModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId) ??
            throw new EntityNotFoundException(nameof(Doctor));

        var parsedDays = ParseAndValidateDays(request.Days);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var existingRules = (await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == request.DoctorId && !r.Deleted && r.Month == today.Month && r.Year == today.Year))
            ?.ToList() ?? new List<AvailabilityRule>();

        var existingSlots = (await _persistence.GetFiltered<AvailabilitySlot>(s =>
            s.DoctorId == request.DoctorId && !s.Deleted && s.SlotDate >= today && s.SlotDate <= monthEnd))
            ?.ToList() ?? new List<AvailabilitySlot>();

        int created = 0;
        var resultRules = new List<AvailabilityRule>();

        foreach (var day in parsedDays)
        {
            var duplicate = existingRules.FirstOrDefault(r =>
                r.DayOfWeek == day.DayOfWeek && r.StartTime == day.Start && r.EndTime == day.End);

            if (duplicate is not null)
            {
                resultRules.Add(duplicate); // ya existía exactamente igual, la devolvemos igual en la respuesta
                continue;
            }

            var overlapping = existingRules.Any(r =>
                r.DayOfWeek == day.DayOfWeek && day.Start < r.EndTime && r.StartTime < day.End);

            if (overlapping)
                throw new ValidationException(ErrorCodes.AVAILABILITY_OVERLAP, nameof(ErrorCodes.AVAILABILITY_OVERLAP));

            var rule = new AvailabilityRule(request.DoctorId, today.Month, today.Year, day.DayOfWeek, day.Start, day.End);
            await _persistence.Add(rule);
            resultRules.Add(rule);

            var newSlots = GenerateSlotsForRule(rule, today, monthEnd)
                .Where(ns => !existingSlots.Any(es => es.SlotDate == ns.SlotDate && es.StartTime == ns.StartTime));

            foreach (var slot in newSlots)
            {
                await _persistence.Add(slot);
                created++;
            }
        }

        _logger.LogInformation("Se generó disponibilidad para el médico {DoctorId}: {SlotsCreated} turnos nuevos creados.", request.DoctorId, created);

        var responseDays = resultRules
            .OrderBy(r => r.DayOfWeek)
            .Select(r => new DoctorModel.AvailabilityResponse(r.Id, DayNames[r.DayOfWeek], r.StartTime.ToString("HH:mm"), r.EndTime.ToString("HH:mm")))
            .ToList();

        return new AvailabilityModel.Response(responseDays);
    }

    public async Task<AvailabilityModel.Response> Update(AvailabilityModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId) ??
            throw new EntityNotFoundException(nameof(Doctor));

        var parsedDays = ParseAndValidateDays(request.Days);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var existingRules = (await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == request.DoctorId && !r.Deleted && r.Month == today.Month && r.Year == today.Year))
            ?.ToList() ?? new List<AvailabilityRule>();

        foreach (var rule in existingRules)
        {
            rule.SoftDelete();
            await _persistence.Update(rule);
        }

        var existingSlots = (await _persistence.GetFiltered<AvailabilitySlot>(s =>
            s.DoctorId == request.DoctorId && !s.Deleted && s.SlotDate >= today && s.SlotDate <= monthEnd))
            ?.ToList() ?? new List<AvailabilitySlot>();


        foreach (var slot in existingSlots.Where(s => s.Status == SlotStatus.Available))
        {
            slot.SoftDelete();
            await _persistence.Update(slot);
        }

        var stillBooked = existingSlots.Where(s => s.Status == SlotStatus.Booked).ToList();

        int created = 0;
        var resultRules = new List<AvailabilityRule>();

        foreach (var day in parsedDays)
        {
            var rule = new AvailabilityRule(request.DoctorId, today.Month, today.Year, day.DayOfWeek, day.Start, day.End);
            await _persistence.Add(rule);
            resultRules.Add(rule);

            var newSlots = GenerateSlotsForRule(rule, today, monthEnd)
                .Where(ns => !stillBooked.Any(bs => bs.SlotDate == ns.SlotDate && bs.StartTime == ns.StartTime));

            foreach (var slot in newSlots)
            {
                await _persistence.Add(slot);
                created++;
            }
        }

        _logger.LogInformation("Se sobrescribió la disponibilidad del mes para el médico {DoctorId}: {SlotsCreated} turnos nuevos creados.", request.DoctorId, created);

        var responseDays = resultRules
            .OrderBy(r => r.DayOfWeek)
            .Select(r => new DoctorModel.AvailabilityResponse(r.Id, DayNames[r.DayOfWeek], r.StartTime.ToString("HH:mm"), r.EndTime.ToString("HH:mm")))
            .ToList();

        return new AvailabilityModel.Response(responseDays);
    }


}
