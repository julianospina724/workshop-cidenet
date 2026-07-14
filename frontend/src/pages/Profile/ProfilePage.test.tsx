import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../auth/AuthContext";
import * as apiClient from "../../api/client";
import ProfilePage from "./ProfilePage";

function renderProfile() {
  localStorage.setItem(
    "workshop-cidenet-auth",
    JSON.stringify({
      token: "fake-token",
      user: { id: "u-1", nombre: "Ana Pérez", email: "ana.perez@mail.com", rol: "Editor", estado: "Activo" },
    }),
  );

  return render(
    <MemoryRouter initialEntries={["/profile"]}>
      <AuthProvider>
        <Routes>
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/" element={<div>Pantalla principal</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

describe("ProfilePage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("muestra mi nombre y email actuales, sin campos de rol ni estado", () => {
    renderProfile();

    expect(screen.getByLabelText(/^nombre$/i)).toHaveValue("Ana Pérez");
    expect(screen.getByLabelText(/^email$/i)).toHaveValue("ana.perez@mail.com");
    expect(screen.queryByLabelText(/^rol$/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/^estado$/i)).not.toBeInTheDocument();
  });

  it("edito mi nombre y email y vuelvo a la pantalla principal", async () => {
    const editSpy = vi.spyOn(apiClient, "editProfile").mockResolvedValue({
      id: "u-1",
      nombre: "Ana Actualizada",
      email: "ana.nueva@mail.com",
      rol: "Editor",
      estado: "Activo",
      createdAt: "2026-01-01T00:00:00Z",
    });

    renderProfile();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Actualizada" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana.nueva@mail.com" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    await waitFor(() => expect(screen.getByText(/pantalla principal/i)).toBeInTheDocument());
    expect(editSpy).toHaveBeenCalledWith({ nombre: "Ana Actualizada", email: "ana.nueva@mail.com" }, "fake-token");
  });

  it("actualiza mis datos de sesión tras guardar (el header refleja el nuevo nombre)", async () => {
    vi.spyOn(apiClient, "editProfile").mockResolvedValue({
      id: "u-1",
      nombre: "Ana Actualizada",
      email: "ana.perez@mail.com",
      rol: "Editor",
      estado: "Activo",
      createdAt: "2026-01-01T00:00:00Z",
    });

    renderProfile();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Actualizada" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    await waitFor(() => expect(screen.getByText(/pantalla principal/i)).toBeInTheDocument());
    const stored = JSON.parse(localStorage.getItem("workshop-cidenet-auth")!);
    expect(stored.user.nombre).toBe("Ana Actualizada");
  });

  it("nombre vacío: marca el campo inválido sin enviar el formulario", async () => {
    const editSpy = vi.spyOn(apiClient, "editProfile");

    renderProfile();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByText(/nombre es obligatorio/i)).toBeInTheDocument();
    expect(editSpy).not.toHaveBeenCalled();
  });

  it("email inválido: marca el campo inválido sin enviar el formulario", async () => {
    const editSpy = vi.spyOn(apiClient, "editProfile");

    renderProfile();
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "correo-invalido" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByText(/formato válido/i)).toBeInTheDocument();
    expect(editSpy).not.toHaveBeenCalled();
  });

  it("email ya registrado: muestra el mensaje general del backend", async () => {
    vi.spyOn(apiClient, "editProfile").mockRejectedValue(new apiClient.ApiError("El correo ya está registrado.", 400));

    renderProfile();
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent("El correo ya está registrado.");
  });

  it("fallo general del backend: muestra un mensaje genérico", async () => {
    vi.spyOn(apiClient, "editProfile").mockRejectedValue(
      new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500),
    );

    renderProfile();
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });
});
