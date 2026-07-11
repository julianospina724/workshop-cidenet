import { useAuth } from "../../auth/AuthContext";

export default function HomePage() {
  const { user, logout } = useAuth();

  return (
    <main>
      <h1>Workshop AI-First</h1>
      <p>Bienvenido, {user?.nombre} ({user?.rol}).</p>
      <button type="button" onClick={logout}>
        Cerrar sesión
      </button>
    </main>
  );
}
