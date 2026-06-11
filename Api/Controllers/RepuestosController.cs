using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.UseCase.Repuestos;
using Application.Common;

namespace Api.Controllers;

/// <summary>Gestión de Repuestos</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepuestosController : ControllerBase
{
    private readonly IMediator _mediator;
    public RepuestosController(IMediator mediator) => _mediator = mediator;

    /// <summary>Obtiene lista paginada de repuestos</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RepuestoDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] RepuestoFiltroDto filtro, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRepuestosQuery(filtro), ct);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(ApiResponse<PagedResult<RepuestoDto>>.Success(result));
    }

    /// <summary>Obtiene un repuesto por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RepuestoDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRepuestoByIdQuery(id), ct);
        return Ok(ApiResponse<RepuestoDto>.Success(result));
    }

    /// <summary>Crea un nuevo repuesto</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista")]
    [ProducesResponseType(typeof(ApiResponse<RepuestoDto>), 201)]
    public async Task<IActionResult> Create([FromBody] CreateRepuestoDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateRepuestoCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RepuestoDto>.Success(result));
    }

    /// <summary>Actualiza un repuesto</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Recepcionista")]
    [ProducesResponseType(typeof(ApiResponse<RepuestoDto>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRepuestoDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateRepuestoCommand(id, dto), ct);
        return Ok(ApiResponse<RepuestoDto>.Success(result));
    }

    /// <summary>Elimina un repuesto</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteRepuestoCommand(id), ct);
        return NoContent();
    }

    /// <summary>Reserva stock de un repuesto para una orden de servicio</summary>
    [HttpPost("reservas")]
    [Authorize(Roles = "Admin,Recepcionista,JefeAlmacen,JefeBodega,Mecánico,MecanicoDiagnostico,MecanicoArea")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Reservar([FromBody] ReservaRepuestoDto dto, CancellationToken ct)
    {
        await _mediator.Send(new ReservaRepuestoCommand(dto), ct);
        return NoContent();
    }

    /// <summary>Descuenta definitivamente el stock de un repuesto al cerrar la orden</summary>
    [HttpPost("consumos")]
    [Authorize(Roles = "Admin,Recepcionista,JefeAlmacen,JefeBodega,Mecánico,MecanicoDiagnostico,MecanicoArea")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Consumir([FromBody] ConsumoRepuestoDto dto, CancellationToken ct)
    {
        await _mediator.Send(new ConsumoRepuestoCommand(dto), ct);
        return NoContent();
    }

    /// <summary>Libera una reserva de stock de un repuesto cuando la orden se cancela</summary>
    [HttpPost("liberaciones")]
    [Authorize(Roles = "Admin,Recepcionista,JefeAlmacen,JefeBodega,Mecánico,MecanicoDiagnostico,MecanicoArea")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Liberar([FromBody] LiberacionRepuestoDto dto, CancellationToken ct)
    {
        await _mediator.Send(new LiberacionRepuestoCommand(dto), ct);
        return NoContent();
    }
}
