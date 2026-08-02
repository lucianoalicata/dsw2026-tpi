using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.Response> BookAppointment (AppointmentModel.Request request);
    Task<AppointmentModel.Response> CancelAppointment(Guid id);
    Task<Pagination<AppointmentModel.Response>> GetPatientAppointments(string dni, int pageSize,int pageIndex);
    Task<Pagination<AppointmentModel.Response>>  GetAppointmentsByDate(DateOnly date,int pageSize, int pageIndex);
    Task<Pagination<AppointmentModel.SearchResponse>> SearchAppointments(Guid? specialtyId, Guid? doctorId, string? dni, 
                                                                            DateOnly? date, int pageSize, int pageIndex);
}
