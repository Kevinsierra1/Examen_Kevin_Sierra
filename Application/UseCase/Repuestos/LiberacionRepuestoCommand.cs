using MediatR;
using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCase.Repuestos;

// Repone (libera) la reserva de stock cuando la orden se cancela, sin afectar el stock actual
public record LiberacionRepuestoCommand(LiberacionRepuestoDto Dto) : IRequest;

public class LiberacionRepuestoCommandHandler : IRequestHandler<LiberacionRepuestoCommand>
{
    private readonly IApplicationDbContext _context;
    public LiberacionRepuestoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(LiberacionRepuestoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var repuesto = await _context.Repuestos.FindAsync([dto.RepuestoId], cancellationToken)
            ?? throw new NotFoundException("Repuesto", dto.RepuestoId);

        repuesto.StockReservado = Math.Max(0, repuesto.StockReservado - dto.Cantidad);

        _context.MovimientosInventario.Add(new MovimientoInventario
        {
            RepuestoId = repuesto.Id,
            Tipo = TipoMovimientoInventario.Liberacion,
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
