using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions;
using Application.Extensions;
using Application.Common;

namespace Application.UseCase.Vehiculos;

public record GetVehiculosQuery(VehiculoFiltroDto Filtro) : IRequest<PagedResult<VehiculoDto>>;

public class GetVehiculosQueryHandler : IRequestHandler<GetVehiculosQuery, PagedResult<VehiculoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetVehiculosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<VehiculoDto>> Handle(GetVehiculosQuery request, CancellationToken cancellationToken)
    {
        var q = _context.Vehiculos.AsQueryable();
        var f = request.Filtro;
        if (!string.IsNullOrWhiteSpace(f.Placa))
            q = q.Where(v => EF.Functions.ILike(v.Placa, $"%{f.Placa.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(f.Vin))
            q = q.Where(v => v.Vin == f.Vin.Trim());
        if (!string.IsNullOrWhiteSpace(f.Marca))
            q = q.Where(v => EF.Functions.ILike(v.ModeloVehiculo.Marca.Nombre, $"%{f.Marca.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(f.Modelo))
            q = q.Where(v => EF.Functions.ILike(v.ModeloVehiculo.Nombre, $"%{f.Modelo.Trim()}%"));
        if (f.AnioDesde.HasValue) q = q.Where(v => v.Anio >= f.AnioDesde);
        if (f.AnioHasta.HasValue) q = q.Where(v => v.Anio <= f.AnioHasta);
        if (f.Activo.HasValue) q = q.Where(v => v.Activo == f.Activo);
        // Solo vehículos que pertenecen al cliente indicado (propietario activo)
        if (f.ClienteId.HasValue)
            q = q.Where(v => v.Propietarios != null &&
                v.Propietarios.Any(p => p.ClienteId == f.ClienteId && p.Activo));
        q = q.OrderBy(v => v.ModeloVehiculo.Marca.Nombre).ThenBy(v => v.ModeloVehiculo.Nombre);
        return await q.ProjectTo<VehiculoDto>(_mapper.ConfigurationProvider).ToPagedResultAsync(f.PageNumber, f.PageSize, cancellationToken);
    }
}
