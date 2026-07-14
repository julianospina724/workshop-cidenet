import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { ApiError, editProfile } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";

const GENERIC_ERROR_MESSAGE = "Ocurrió un error inesperado. Intenta de nuevo.";

function isValidEmail(email: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
}

export default function ProfilePage() {
  const navigate = useNavigate();
  const { token, user, updateUser } = useAuth();

  const [nombre, setNombre] = useState(user?.nombre ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  function validate(): Record<string, string> {
    const errors: Record<string, string> = {};

    if (!nombre.trim()) {
      errors.nombre = "El nombre es obligatorio.";
    }

    if (!isValidEmail(email)) {
      errors.email = "El email no tiene un formato válido.";
    }

    return errors;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setGeneralError(null);

    const errors = validate();
    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    setFieldErrors({});
    setLoading(true);

    try {
      const updated = await editProfile({ nombre, email }, token!);
      updateUser({ id: updated.id, nombre: updated.nombre, email: updated.email, rol: updated.rol, estado: updated.estado });
      navigate("/");
    } catch (err) {
      if (err instanceof ApiError && err.fieldErrors) {
        setFieldErrors(err.fieldErrors);
      } else {
        setGeneralError(err instanceof ApiError ? err.message : GENERIC_ERROR_MESSAGE);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main>
      <h1>Mi perfil</h1>
      <form onSubmit={handleSubmit} noValidate>
        <div>
          <label htmlFor="nombre">Nombre</label>
          <input id="nombre" value={nombre} onChange={(event) => setNombre(event.target.value)} />
          {fieldErrors.nombre && <p role="alert">{fieldErrors.nombre}</p>}
        </div>

        <div>
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
          {fieldErrors.email && <p role="alert">{fieldErrors.email}</p>}
        </div>

        <button type="submit" disabled={loading}>
          {loading ? "Guardando..." : "Guardar"}
        </button>
      </form>

      {generalError && <p role="alert">{generalError}</p>}
    </main>
  );
}
