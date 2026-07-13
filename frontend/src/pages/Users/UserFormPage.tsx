import { useState, type FormEvent } from "react";
import { Navigate, useLocation, useNavigate, useParams } from "react-router-dom";
import { ApiError, createUser, editUser, type UserRecord } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";

const PASSWORD_SYMBOLS = "$&*#@";
const GENERIC_ERROR_MESSAGE = "Ocurrió un error inesperado. Intenta de nuevo.";

function isValidEmail(email: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
}

function isValidPassword(password: string): boolean {
  if (password.length < 8) return false;
  if (!/[A-Z]/.test(password)) return false;
  if (!/[a-z]/.test(password)) return false;
  if (!/[0-9]/.test(password)) return false;
  if (![...password].some((char) => PASSWORD_SYMBOLS.includes(char))) return false;
  return true;
}

export default function UserFormPage() {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const { token, user: currentUser } = useAuth();

  const editingUser = (location.state as { user?: UserRecord } | null)?.user ?? null;
  const isEditMode = Boolean(id);

  const [nombre, setNombre] = useState(editingUser?.nombre ?? "");
  const [email, setEmail] = useState(editingUser?.email ?? "");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [rol, setRol] = useState(editingUser?.rol ?? "Editor");
  const [estado, setEstado] = useState(editingUser?.estado ?? "Activo");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (isEditMode && !editingUser) {
    return <Navigate to="/" replace />;
  }

  const isEditingSelf = isEditMode && editingUser?.id === currentUser?.id;

  function validate(): Record<string, string> {
    const errors: Record<string, string> = {};

    if (!nombre.trim()) {
      errors.nombre = "El nombre es obligatorio.";
    }

    if (!isValidEmail(email)) {
      errors.email = "El email no tiene un formato válido.";
    }

    if (!isEditMode) {
      if (!isValidPassword(password)) {
        errors.password = "La contraseña debe tener mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo ($&*#@).";
      } else if (password !== confirmPassword) {
        errors.confirmPassword = "La contraseña y su confirmación no coinciden.";
      }
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
      if (isEditMode && editingUser) {
        await editUser(editingUser.id, { nombre, email, rol: isEditingSelf ? undefined : rol, estado }, token!);
      } else {
        await createUser({ nombre, email, password, confirmPassword, rol }, token!);
      }
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
      <h1>{isEditMode ? "Editar usuario" : "Crear usuario"}</h1>
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

        {!isEditMode && (
          <>
            <div>
              <label htmlFor="password">Contraseña</label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />
              {fieldErrors.password && <p role="alert">{fieldErrors.password}</p>}
            </div>

            <div>
              <label htmlFor="confirmPassword">Confirmar contraseña</label>
              <input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
              />
              {fieldErrors.confirmPassword && <p role="alert">{fieldErrors.confirmPassword}</p>}
            </div>
          </>
        )}

        <div>
          <label htmlFor="rol">Rol</label>
          <select id="rol" value={rol} onChange={(event) => setRol(event.target.value)} disabled={isEditingSelf}>
            <option value="Admin">Admin</option>
            <option value="Editor">Editor</option>
            <option value="Viewer">Viewer</option>
          </select>
          {isEditingSelf && <p>No puedes modificar tu propio rol.</p>}
        </div>

        {isEditMode && (
          <div>
            <label htmlFor="estado">Estado</label>
            <select id="estado" value={estado} onChange={(event) => setEstado(event.target.value)}>
              <option value="Activo">Activo</option>
              <option value="Inactivo">Inactivo</option>
            </select>
          </div>
        )}

        <button type="submit" disabled={loading}>
          {loading ? "Guardando..." : "Guardar"}
        </button>
      </form>

      {generalError && <p role="alert">{generalError}</p>}
    </main>
  );
}
