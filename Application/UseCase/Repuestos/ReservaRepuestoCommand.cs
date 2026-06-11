using MediatR;
using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCase.Repuestos;

// Reserva stock disponible (StockActual - StockReservado) para una orden de servicio
public record ReservaRepuestoCommand(ReservaRepuestoDto Dto) : IRequest;

public class ReservaRepuestoCommandHandler : IRequestHandler<ReservaRepuestoCommand>
{
    private readonly IApplicationDbContext _context;
    public ReservaRepuestoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReservaRepuestoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var repuesto = await _context.Repuestos.FindAsync([dto.RepuestoId], cancellationToken)
            ?? throw new NotFoundException("Repuesto", dto.RepuestoId);

        var disponible = repuesto.StockActual - repuesto.StockReservado;
        if (disponible < dto.Cantidad)
            throw new StockInsuficienteException(repuesto.Nombre, disponible, dto.Cantidad);

        repuesto.StockReservado += dto.Cantidad;

        _context.MovimientosInventario.Add(new MovimientoInventario
        {
            RepuestoId = repuesto.Id,
            Tipo = TipoMovimientoInventario.Reserva,
            Cantidad = dto.Cantidad,
            CantidadAnterior = repuesto.StockActual,
            CantidadNueva = repuesto.StockActual,
            Motivo = dto.Motivo,
            OrdenServicioId = dto.OrdenServicioId,
            FechaMovimiento = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
