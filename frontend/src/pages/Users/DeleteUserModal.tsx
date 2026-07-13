import { useState } from "react";
import { ApiError, deleteUser, type UserRecord } from "../../api/client";

const GENERIC_ERROR_MESSAGE = "Ocurrió un error inesperado. Intenta de nuevo.";
const NOT_FOUND_MESSAGE = "El usuario ya no existe.";

interface DeleteUserModalProps {
  user: UserRecord;
  token: string;
  onCancel: () => void;
  onDeleted: () => void;
}

export default function DeleteUserModal({ user, token, onCancel, onDeleted }: DeleteUserModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleConfirm() {
    setLoading(true);
    setError(null);

    try {
      await deleteUser(user.id, token);
      onDeleted();
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setError(NOT_FOUND_MESSAGE);
      } else if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError(GENERIC_ERROR_MESSAGE);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div role="dialog" aria-label="Confirmar eliminación">
      <h2>Eliminar usuario</h2>
      <p>¿Seguro que quieres eliminar a este usuario?</p>
      <dl>
        <dt>Nombre</dt>
        <dd>{user.nombre}</dd>
        <dt>Email</dt>
        <dd>{user.email}</dd>
        <dt>Rol</dt>
        <dd>{user.rol}</dd>
      </dl>

      {error && <p role="alert">{error}</p>}

      <button type="button" onClick={onCancel} disabled={loading}>
        Cancelar
      </button>
      <button type="button" onClick={handleConfirm} disabled={loading}>
        {loading ? "Eliminando..." : "Confirmar eliminación"}
      </button>
    </div>
  );
}
