using MediatR;
using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCase.Repuestos;

// Descuenta definitivamente el stock al cerrar la orden, liberando la reserva asociada
public record ConsumoRepuestoCommand(ConsumoRepuestoDto Dto) : IRequest;

public class ConsumoRepuestoCommandHandler : IRequestHandler<ConsumoRepuestoCommand>
{
    private readonly IApplicationDbContext _context;
    public ConsumoRepuestoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ConsumoRepuestoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var repuesto = await _context.Repuestos.FindAsync([dto.RepuestoId], cancellationToken)
            ?? throw new NotFoundException("Repuesto", dto.RepuestoId);

        if (repuesto.StockActual < dto.Cantidad)
            throw new StockInsuficienteException(repuesto.Nombre, repuesto.StockActual, dto.Cantidad);

        var anterior = repuesto.StockActual;
        repuesto.StockActual -= dto.Cantidad;
        repuesto.StockReservado = Math.Max(0, repuesto.StockReservado - dto.Cantidad);

        _context.MovimientosInventario.Add(new MovimientoInventario
        {
            RepuestoId = repuesto.Id,
            Tipo = TipoMovimientoInventario.Consumo,
            Cantidad = dto.Cantidad,
            CantidadAnterior = anterior,
            CantidadNueva = repuesto.StockActual,
            Motivo = dto.Motivo,
            OrdenServicioId = dto.OrdenServicioId,
            FechaMovimiento = DateTime.UtcNow
        });

        if (repuesto.StockActual <= repuesto.StockMinimo)
        {
            _context.Notificaciones.Add(new Notificacion
            {
                Titulo = "Stock bajo",
                Mensaje = $"El repuesto '{repuesto.Nombre}' ({repuesto.Codigo}) quedó con stock bajo: {repuesto.StockActual}/{repuesto.StockMinimo}.",
                Tipo = "StockBajo",
                FechaCreacion = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
