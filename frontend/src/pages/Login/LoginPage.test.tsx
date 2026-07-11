import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../auth/AuthContext";
import * as apiClient from "../../api/client";
import LoginPage from "./LoginPage";

function renderLoginPage() {
  return render(
    <MemoryRouter initialEntries={["/login"]}>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<div>Pantalla principal</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

const validUser = { id: "1", nombre: "Ana Pérez", email: "ana@mail.com", rol: "Admin", estado: "Activo" };

describe("LoginPage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("envía email y contraseña, y redirige tras un login exitoso", async () => {
    const loginSpy = vi.spyOn(apiClient, "login").mockResolvedValue({ token: "fake-token", user: validUser });

    renderLoginPage();

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    expect(await screen.findByText(/pantalla principal/i)).toBeInTheDocument();
    expect(loginSpy).toHaveBeenCalledWith("ana@mail.com", "Clave123$");
  });

  it("guarda el token recibido para peticiones futuras", async () => {
    vi.spyOn(apiClient, "login").mockResolvedValue({ token: "fake-token", user: validUser });

    renderLoginPage();

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    await screen.findByText(/pantalla principal/i);

    expect(localStorage.getItem("workshop-cidenet-auth")).toContain("fake-token");
  });

  it("muestra un indicador de carga mientras se procesa el login", async () => {
    let resolveLogin!: (value: { token: string; user: typeof validUser }) => void;
    vi.spyOn(apiClient, "login").mockReturnValue(
      new Promise((resolve) => {
        resolveLogin = resolve;
      }),
    );

    renderLoginPage();
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    expect(screen.getByRole("button", { name: /ingresando/i })).toBeDisabled();

    resolveLogin({ token: "fake-token", user: validUser });
    await waitFor(() => expect(screen.getByRole("button")).toBeEnabled());
  });

  it("muestra el mensaje de error cuando las credenciales son incorrectas", async () => {
    vi.spyOn(apiClient, "login").mockRejectedValue(new apiClient.ApiError("Email o contraseña incorrectos.", 401));

    renderLoginPage();
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "mala" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Email o contraseña incorrectos.");
  });

  it("muestra el mensaje de bloqueo temporal cuando la cuenta está bloqueada", async () => {
    vi.spyOn(apiClient, "login").mockRejectedValue(
      new apiClient.ApiError("La cuenta está bloqueada temporalmente por intentos fallidos. Intenta de nuevo más tarde.", 423),
    );

    renderLoginPage();
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/bloqueada temporalmente/i);
  });

  it("muestra un mensaje genérico ante un fallo general del backend", async () => {
    vi.spyOn(apiClient, "login").mockRejectedValue(new Error("network down"));

    renderLoginPage();
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "ana@mail.com" } });
    fireEvent.change(screen.getByLabelText(/contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });
});
