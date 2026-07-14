import { useEffect, useState } from "react";
import { ApiError, getPermissions, updatePermissions, type PermissionEntry } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";

const GENERIC_ERROR_MESSAGE = "Ocurrió un error inesperado. Intenta de nuevo.";
const OWN_ROLE_TOOLTIP = "No puedes modificar los permisos de tu propio rol.";

const ROLES = ["Admin", "Editor", "Viewer"];
const RESOURCES = ["Users", "Roles", "Permissions", "Reports"];
const ACTIONS: { key: string; label: string }[] = [
  { key: "Create", label: "Crear" },
  { key: "Read", label: "Leer" },
  { key: "Update", label: "Actualizar" },
  { key: "Delete", label: "Eliminar" },
];

function findEntry(entries: PermissionEntry[], rol: string, recurso: string, accion: string): PermissionEntry | undefined {
  return entries.find((entry) => entry.rol === rol && entry.recurso === recurso && entry.accion === accion);
}

export default function PermissionsPage() {
  const { token, user } = useAuth();
  const [savedEntries, setSavedEntries] = useState<PermissionEntry[] | null>(null);
  const [draftEntries, setDraftEntries] = useState<PermissionEntry[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!token) {
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    getPermissions(token)
      .then((result) => {
        if (!cancelled) {
          setSavedEntries(result.entries);
          setDraftEntries(result.entries);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : GENERIC_ERROR_MESSAGE);
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
  }, [token]);

  function handleToggle(rol: string, recurso: string, accion: string) {
    setSuccessMessage(null);
    setDraftEntries(
      (prev) =>
        prev?.map((entry) =>
          entry.rol === rol && entry.recurso === recurso && entry.accion === accion
            ? { ...entry, permitido: !entry.permitido }
            : entry,
        ) ?? null,
    );
  }

  async function handleSave() {
    if (!draftEntries || !savedEntries || !token) {
      return;
    }

    const changes = draftEntries.filter((entry) => {
      const previous = findEntry(savedEntries, entry.rol, entry.recurso, entry.accion);
      return previous !== undefined && previous.permitido !== entry.permitido;
    });

    setSuccessMessage(null);
    setError(null);
    setSaving(true);

    try {
      await updatePermissions(changes, token);
      setSavedEntries(draftEntries);
      setSuccessMessage("Cambios guardados.");
    } catch (err) {
      setDraftEntries(savedEntries);
      setError(err instanceof ApiError ? err.message : GENERIC_ERROR_MESSAGE);
    } finally {
      setSaving(false);
    }
  }

  return (
    <main>
      <h1>Matriz de permisos</h1>

      {loading && <p>Cargando...</p>}
      {error && <p role="alert">{error}</p>}

      {!loading && draftEntries && (
        <>
          <table>
            <thead>
              <tr>
                <th>Rol</th>
                {RESOURCES.map((recurso) =>
                  ACTIONS.map((accion) => (
                    <th key={`${recurso}-${accion.key}`}>
                      {recurso} · {accion.label}
                    </th>
                  )),
                )}
              </tr>
            </thead>
            <tbody>
              {ROLES.map((rol) => {
                const isOwnRole = rol === user?.rol;
                return (
                  <tr key={rol}>
                    <th scope="row">{rol}</th>
                    {RESOURCES.map((recurso) =>
                      ACTIONS.map((accion) => {
                        const entry = findEntry(draftEntries, rol, recurso, accion.key);
                        return (
                          <td key={`${rol}-${recurso}-${accion.key}`}>
                            <input
                              type="checkbox"
                              aria-label={`${accion.label} en ${recurso} para ${rol}`}
                              title={isOwnRole ? OWN_ROLE_TOOLTIP : undefined}
                              checked={entry?.permitido ?? false}
                              disabled={isOwnRole}
                              onChange={() => handleToggle(rol, recurso, accion.key)}
                            />
                          </td>
                        );
                      }),
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>

          <button type="button" onClick={handleSave} disabled={saving}>
            {saving ? "Guardando..." : "Guardar cambios"}
          </button>

          {successMessage && <p>{successMessage}</p>}
        </>
      )}
    </main>
  );
}
