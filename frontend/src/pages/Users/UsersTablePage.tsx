import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { ApiError, getUsers, type PagedUsers, type UserRecord, type UsersQuery } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import DeleteUserModal from "./DeleteUserModal";

interface Filters {
  rol: string;
  nombre: string;
  estado: string;
  fechaDesde: string;
  fechaHasta: string;
}

const EMPTY_FILTERS: Filters = { rol: "", nombre: "", estado: "", fechaDesde: "", fechaHasta: "" };

export default function UsersTablePage() {
  const { token, user, logout } = useAuth();
  const [draftFilters, setDraftFilters] = useState<Filters>(EMPTY_FILTERS);
  const [appliedFilters, setAppliedFilters] = useState<Filters>(EMPTY_FILTERS);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [data, setData] = useState<PagedUsers | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [userToDelete, setUserToDelete] = useState<UserRecord | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  useEffect(() => {
    if (!token) {
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    const query: UsersQuery = {
      rol: appliedFilters.rol || undefined,
      nombre: appliedFilters.nombre || undefined,
      estado: appliedFilters.estado || undefined,
      fechaDesde: appliedFilters.fechaDesde || undefined,
      fechaHasta: appliedFilters.fechaHasta || undefined,
      page,
      pageSize,
    };

    getUsers(query, token)
      .then((result) => {
        if (!cancelled) {
          setData(result);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : "Ocurrió un error inesperado. Intenta de nuevo.");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [appliedFilters, page, pageSize, token, refreshTrigger]);

  function handleFilterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPage(1);
    setAppliedFilters(draftFilters);
  }

  function handlePageSizeChange(newPageSize: number) {
    setPage(1);
    setPageSize(newPageSize);
  }

  return (
    <main>
      <header>
        <h1>Usuarios</h1>
        <p>
          Sesión: {user?.nombre} ({user?.rol})
        </p>
        <button type="button" onClick={logout}>
          Cerrar sesión
        </button>
        <Link to="/profile">Mi perfil</Link>
        {user?.rol === "Admin" && <Link to="/users/new">+ Nuevo usuario</Link>}
        {user?.rol === "Admin" && <Link to="/permissions">Matriz de permisos</Link>}
      </header>

      <form onSubmit={handleFilterSubmit}>
        <div>
          <label htmlFor="filtro-nombre">Nombre</label>
          <input
            id="filtro-nombre"
            value={draftFilters.nombre}
            onChange={(event) => setDraftFilters({ ...draftFilters, nombre: event.target.value })}
          />
        </div>
        <div>
          <label htmlFor="filtro-rol">Rol</label>
          <select
            id="filtro-rol"
            value={draftFilters.rol}
            onChange={(event) => setDraftFilters({ ...draftFilters, rol: event.target.value })}
          >
            <option value="">Todos</option>
            <option value="Admin">Admin</option>
            <option value="Editor">Editor</option>
            <option value="Viewer">Viewer</option>
          </select>
        </div>
        <div>
          <label htmlFor="filtro-estado">Estado</label>
          <select
            id="filtro-estado"
            value={draftFilters.estado}
            onChange={(event) => setDraftFilters({ ...draftFilters, estado: event.target.value })}
          >
            <option value="">Todos</option>
            <option value="Activo">Activo</option>
            <option value="Inactivo">Inactivo</option>
          </select>
        </div>
        <div>
          <label htmlFor="filtro-fecha-desde">Fecha desde</label>
          <input
            id="filtro-fecha-desde"
            type="date"
            value={draftFilters.fechaDesde}
            onChange={(event) => setDraftFilters({ ...draftFilters, fechaDesde: event.target.value })}
          />
        </div>
        <div>
          <label htmlFor="filtro-fecha-hasta">Fecha hasta</label>
          <input
            id="filtro-fecha-hasta"
            type="date"
            min={draftFilters.fechaDesde || undefined}
            value={draftFilters.fechaHasta}
            onChange={(event) => setDraftFilters({ ...draftFilters, fechaHasta: event.target.value })}
          />
        </div>
        <button type="submit">Filtrar</button>
      </form>

      {loading && <p>Cargando...</p>}
      {error && <p role="alert">{error}</p>}

      {!loading && !error && data && data.items.length === 0 && <p>No se encontraron registros.</p>}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Email</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Fecha de alta</th>
                {user?.rol === "Admin" && <th>Acciones</th>}
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.id}>
                  <td>{item.nombre}</td>
                  <td>{item.email}</td>
                  <td>{item.rol}</td>
                  <td>{item.estado}</td>
                  <td>{new Date(item.createdAt).toLocaleDateString()}</td>
                  {user?.rol === "Admin" && (
                    <td>
                      <Link to={`/users/${item.id}/edit`} state={{ user: item }}>
                        Editar
                      </Link>{" "}
                      <button type="button" onClick={() => setUserToDelete(item)}>
                        Eliminar
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>

          <div>
            <label htmlFor="pagina-tamano">Registros por página</label>
            <select
              id="pagina-tamano"
              value={pageSize}
              onChange={(event) => handlePageSizeChange(Number(event.target.value))}
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>

            <button type="button" disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>
              Anterior
            </button>
            <span>
              Página {data.page} — {data.totalCount} en total
            </span>
            <button
              type="button"
              disabled={page * pageSize >= data.totalCount}
              onClick={() => setPage((current) => current + 1)}
            >
              Siguiente
            </button>
          </div>
        </>
      )}

      {userToDelete && token && (
        <DeleteUserModal
          user={userToDelete}
          token={token}
          onCancel={() => setUserToDelete(null)}
          onDeleted={() => {
            setUserToDelete(null);
            setRefreshTrigger((current) => current + 1);
          }}
        />
      )}
    </main>
  );
}
