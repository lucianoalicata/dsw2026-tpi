using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("specialities")]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;
    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Paciente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize= 10, [FromQuery] int pageIndex= 1, [FromQuery] string? name = null)
    {
        var result = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] SpecialityModel.Request request)
    {
        var result = await _service.Add(request);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialityModel.Request request)
    {
        var result = await _service.Update(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok();
    }

}
