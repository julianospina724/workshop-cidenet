import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "./auth/AuthContext";
import App from "./App";

function renderApp(initialPath = "/") {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <AuthProvider>
        <App />
      </AuthProvider>
    </MemoryRouter>,
  );
}

describe("App", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ status: "ok" }),
      }),
    );
  });

  it("redirige a login cuando no hay una sesión activa", () => {
    renderApp("/");
    expect(screen.getByRole("heading", { name: /iniciar sesión/i })).toBeInTheDocument();
  });

  it("muestra la pantalla principal cuando ya hay una sesión activa", () => {
    localStorage.setItem(
      "workshop-cidenet-auth",
      JSON.stringify({ token: "fake-token", user: { id: "1", nombre: "Ana", email: "ana@mail.com", rol: "Admin", estado: "Activo" } }),
    );

    renderApp("/");

    expect(screen.getByText(/Workshop AI-First/i)).toBeInTheDocument();
  });
});
