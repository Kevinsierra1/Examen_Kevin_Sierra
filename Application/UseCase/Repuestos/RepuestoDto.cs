namespace Application.UseCase.Repuestos;

public record RepuestoDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    Guid CategoriaRepuestoId,
    string? Categoria,
    Guid? TipoServicioId,
    string? TipoServicioNombre,
    decimal PrecioCompra,
    decimal PrecioVenta,
    int StockActual,
    int StockMinimo,
    int StockReservado,
    bool StockBajo,
    string? Unidad,
    bool Activo
);

public record CreateRepuestoDto(
    string Codigo,
    string Nombre,
    string? Descripcion,
    Guid CategoriaRepuestoId,
    Guid? TipoServicioId,
    decimal PrecioCompra,
    decimal PrecioVenta,
    int StockActual,
    int StockMinimo,
    string? Unidad
);

public record UpdateRepuestoDto(
    string? Nombre,
    string? Descripcion,
    Guid? TipoServicioId,
    decimal? PrecioCompra,
    decimal? PrecioVenta,
    int? StockMinimo,
    string? Unidad,
    bool? Activo
);

public record RepuestoFiltroDto(
    string? Busqueda,
    Guid? CategoriaId,
    Guid? TipoServicioId,
    bool? StockCritico,
    bool? Activo,
    int PageNumber = 1,
    int PageSize = 10
);

// Reserva el stock disponible (StockActual - StockReservado) para una orden de servicio
public record ReservaRepuestoDto(
    Guid RepuestoId,
    int Cantidad,
    Guid OrdenServicioId,
    string? Motivo
);

// Descuenta definitivamente el stock al cerrar la orden, liberando la reserva asociada
public record ConsumoRepuestoDto(
    Guid RepuestoId,
    int Cantidad,
    Guid OrdenServicioId,
    string? Motivo
);

// Libera una reserva de stock cuando la orden se cancela, sin afectar el stock actual
public record LiberacionRepuestoDto(
    Guid RepuestoId,
    int Cantidad,
    Guid OrdenServicioId,
    string? Motivo
);
