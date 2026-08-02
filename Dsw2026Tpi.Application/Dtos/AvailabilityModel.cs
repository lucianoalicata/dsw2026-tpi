using System;
using System.Collections.Generic;

namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityModel
{
    public record Request(Guid DoctorId, List<DayRequest> Days);
    public record DayRequest(string Day, string StartTime, string EndTime);
    public record Response(List<DoctorModel.AvailabilityResponse> Days);
}