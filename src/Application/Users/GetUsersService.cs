namespace Application.Users;

public class GetUsersService
{
    private const int DefaultPageSize = 20;

    private readonly IUserRepository _repository;

    public GetUsersService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetUsersResult> GetAsync(GetUsersQuery query)
    {
        if (query.FechaDesde.HasValue && query.FechaHasta.HasValue && query.FechaDesde.Value > query.FechaHasta.Value)
        {
            return GetUsersResult.Failure("El rango de fechas es inválido: 'desde' no puede ser posterior a 'hasta'.");
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : query.PageSize;

        var normalizedQuery = query with { Page = page, PageSize = pageSize };
        var (items, total) = await _repository.SearchAsync(normalizedQuery);

        return GetUsersResult.Success(items, total, page, pageSize);
    }
}
