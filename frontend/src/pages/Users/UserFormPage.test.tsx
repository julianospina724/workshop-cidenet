import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../auth/AuthContext";
import * as apiClient from "../../api/client";
import UserFormPage from "./UserFormPage";

function setSession(currentUser = { id: "admin-1", nombre: "Admin Uno", email: "admin@mail.com", rol: "Admin", estado: "Activo" }) {
  localStorage.setItem("workshop-cidenet-auth", JSON.stringify({ token: "fake-token", user: currentUser }));
}

function renderCreate() {
  return render(
    <MemoryRouter initialEntries={["/users/new"]}>
      <AuthProvider>
        <Routes>
          <Route path="/users/new" element={<UserFormPage />} />
          <Route path="/" element={<div>Pantalla principal</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

function renderEdit(editingUser: { id: string; nombre: string; email: string; rol: string; estado: string }) {
  return render(
    <MemoryRouter
      initialEntries={[{ pathname: `/users/${editingUser.id}/edit`, state: { user: editingUser } }]}
    >
      <AuthProvider>
        <Routes>
          <Route path="/users/:id/edit" element={<UserFormPage />} />
          <Route path="/" element={<div>Pantalla principal</div>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

const otroUsuario = {
  id: "u-2",
  nombre: "Ana Pérez",
  email: "ana.perez@mail.com",
  rol: "Editor",
  estado: "Activo",
  createdAt: "2026-01-01T00:00:00Z",
};

describe("UserFormPage — crear (US-001)", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
    setSession();
  });

  it("crea una cuenta con datos válidos y redirige a la tabla", async () => {
    const createSpy = vi.spyOn(apiClient, "createUser").mockResolvedValue({
      id: "u-3",
      nombre: "Ana Pérez",
      email: "ana.perez@mail.com",
      rol: "Editor",
      estado: "Activo",
      createdAt: "2026-01-01T00:00:00Z",
    });

    renderCreate();

    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Pérez" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana.perez@mail.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Clave123$" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.change(screen.getByLabelText(/^rol$/i), { target: { value: "Editor" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    await waitFor(() => expect(screen.getByText(/pantalla principal/i)).toBeInTheDocument());
    expect(createSpy).toHaveBeenCalledWith(
      { nombre: "Ana Pérez", email: "ana.perez@mail.com", password: "Clave123$", confirmPassword: "Clave123$", rol: "Editor" },
      "fake-token",
    );
  });

  it("la contraseña y su confirmación no coinciden: no envía el formulario", async () => {
    const createSpy = vi.spyOn(apiClient, "createUser");

    renderCreate();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Pérez" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana.perez@mail.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Clave123$" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Clave999$" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByText(/no coinciden/i)).toBeInTheDocument();
    expect(createSpy).not.toHaveBeenCalled();
  });

  it.each([
    ["nombre vacío", { nombre: "" }, /nombre es obligatorio/i],
    ["email inválido", { email: "correo-invalido" }, /formato válido/i],
    ["contraseña corta", { password: "corta1" }, /8 caracteres/i],
    ["contraseña sin complejidad", { password: "sololetrasminuscul" }, /8 caracteres/i],
  ])("%s: marca el campo inválido sin enviar el formulario", async (_case, overrides, expectedMessage) => {
    const createSpy = vi.spyOn(apiClient, "createUser");
    const values = {
      nombre: "Ana Pérez",
      email: "ana.perez@mail.com",
      password: "Clave123$",
      confirmPassword: "Clave123$",
      ...overrides,
    };

    renderCreate();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: values.nombre } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: values.email } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: values.password } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: values.confirmPassword } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByText(expectedMessage)).toBeInTheDocument();
    expect(createSpy).not.toHaveBeenCalled();
  });

  it("email ya registrado: muestra el mensaje general del backend", async () => {
    vi.spyOn(apiClient, "createUser").mockRejectedValue(new apiClient.ApiError("El correo ya está registrado.", 400));

    renderCreate();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Pérez" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana.perez@mail.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Clave123$" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent("El correo ya está registrado.");
  });

  it("fallo general del backend: muestra un mensaje genérico", async () => {
    vi.spyOn(apiClient, "createUser").mockRejectedValue(new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500));

    renderCreate();
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "Ana Pérez" } });
    fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "ana.perez@mail.com" } });
    fireEvent.change(screen.getByLabelText(/^contraseña$/i), { target: { value: "Clave123$" } });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), { target: { value: "Clave123$" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });
});

describe("UserFormPage — editar (US-003)", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
    setSession();
  });

  it("edita los datos de otro usuario y redirige a la tabla", async () => {
    const editSpy = vi.spyOn(apiClient, "editUser").mockResolvedValue({ ...otroUsuario, rol: "Viewer" });

    renderEdit(otroUsuario);

    expect(screen.getByLabelText(/^nombre$/i)).toHaveValue("Ana Pérez");
    fireEvent.change(screen.getByLabelText(/^rol$/i), { target: { value: "Viewer" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    await waitFor(() => expect(screen.getByText(/pantalla principal/i)).toBeInTheDocument());
    expect(editSpy).toHaveBeenCalledWith(
      "u-2",
      expect.objectContaining({ nombre: "Ana Pérez", email: "ana.perez@mail.com", rol: "Viewer" }),
      "fake-token",
    );
  });

  it("el selector de rol se deshabilita al editar la propia cuenta", () => {
    setSession({ id: "admin-1", nombre: "Admin Uno", email: "admin@mail.com", rol: "Admin", estado: "Activo" });
    renderEdit({ id: "admin-1", nombre: "Admin Uno", email: "admin@mail.com", rol: "Admin", estado: "Activo" });

    expect(screen.getByLabelText(/^rol$/i)).toBeDisabled();
    expect(screen.getByText(/no puedes modificar tu propio rol/i)).toBeInTheDocument();
  });

  it("email duplicado al editar: muestra el mensaje general del backend", async () => {
    vi.spyOn(apiClient, "editUser").mockRejectedValue(new apiClient.ApiError("El correo ya está registrado.", 400));

    renderEdit(otroUsuario);
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent("El correo ya está registrado.");
  });

  it("no se puede inactivar al único Admin activo: muestra el mensaje del backend", async () => {
    vi.spyOn(apiClient, "editUser").mockRejectedValue(
      new apiClient.ApiError("Debe existir al menos un Admin activo en el sistema.", 400),
    );

    renderEdit(otroUsuario);
    fireEvent.change(screen.getByLabelText(/^estado$/i), { target: { value: "Inactivo" } });
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/al menos un Admin activo/i);
  });

  it("fallo general del backend al editar: muestra un mensaje genérico", async () => {
    vi.spyOn(apiClient, "editUser").mockRejectedValue(new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500));

    renderEdit(otroUsuario);
    fireEvent.click(screen.getByRole("button", { name: /guardar/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });
});
