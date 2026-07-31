using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/appointments")]

[Authorize]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController (IAppointmentService service)
    {
        _service=service;
    }

    [HttpPost]
    [Authorize(Policy =Policies.PatientPolicy)]
    [ProducesResponseType (StatusCodes.Status200OK)]
    [ProducesResponseType (StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType (StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add ([FromBody] AppointmentModel.Request request)
    {
        var result = await _service.BookAppointment(request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel (Guid id)
    {
        var result = await _service.CancelAppointment(id );
        return Ok(result);
    }

    [HttpGet("patient")]
    [Authorize(Policy= Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(
        [FromQuery] string dni, 
        [FromQuery] int pageSize =10, 
        [FromQuery] int pageIndex=1)
    {
        var appointments = await _service.GetPatientAppointments(dni ,pageSize,pageIndex);
        return Ok(appointments);
    }

    [HttpGet]
    [Authorize(Policy=Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateOnly date, 
        [FromQuery] int pageSize=10, 
        [FromQuery] int pageIndex =1)
    {
        var appointments =await _service.GetAppointmentsByDate(date,pageSize,pageIndex);

        return Ok( appointments);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? specialtyId, [FromQuery] Guid? doctorId, 
        [FromQuery] string? dni,[FromQuery] DateOnly? date, 
        [FromQuery] int pageSize= 10, [FromQuery] int pageIndex =1)
    {
        var appointments= await _service.SearchAppointments(specialtyId,doctorId,dni, date, pageSize ,pageIndex);
        return Ok (appointments);
    }
}