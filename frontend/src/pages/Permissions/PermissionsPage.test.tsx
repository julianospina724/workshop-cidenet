import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../auth/AuthContext";
import * as apiClient from "../../api/client";
import PermissionsPage from "./PermissionsPage";

const ROLES = ["Admin", "Editor", "Viewer"];
const RESOURCES = ["Users", "Roles", "Permissions", "Reports"];
const ACTIONS = ["Create", "Read", "Update", "Delete"];

const CRUD_BY_ROLE_RESOURCE: Record<string, Record<string, boolean[]>> = {
  Admin: { Users: [true, true, true, true], Roles: [true, true, true, true], Permissions: [true, true, true, true], Reports: [true, true, true, true] },
  Editor: { Users: [false, true, false, false], Roles: [false, true, false, false], Permissions: [false, true, false, false], Reports: [true, true, true, true] },
  Viewer: { Users: [false, false, false, false], Roles: [false, false, false, false], Permissions: [false, false, false, false], Reports: [false, true, false, false] },
};

function buildDefaultEntries() {
  const entries: { rol: string; recurso: string; accion: string; permitido: boolean }[] = [];
  for (const rol of ROLES) {
    for (const recurso of RESOURCES) {
      ACTIONS.forEach((accion, index) => {
        entries.push({ rol, recurso, accion, permitido: CRUD_BY_ROLE_RESOURCE[rol][recurso][index] });
      });
    }
  }
  return entries;
}

function renderPage() {
  localStorage.setItem(
    "workshop-cidenet-auth",
    JSON.stringify({
      token: "fake-token",
      user: { id: "u-1", nombre: "Admin Inicial", email: "admin@workshop-cidenet.local", rol: "Admin", estado: "Activo" },
    }),
  );

  return render(
    <MemoryRouter>
      <AuthProvider>
        <PermissionsPage />
      </AuthProvider>
    </MemoryRouter>,
  );
}

describe("PermissionsPage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("muestra un spinner mientras carga la matriz", async () => {
    let resolveGetPermissions!: (value: { entries: ReturnType<typeof buildDefaultEntries> }) => void;
    vi.spyOn(apiClient, "getPermissions").mockReturnValue(
      new Promise((resolve) => {
        resolveGetPermissions = resolve;
      }),
    );

    renderPage();

    expect(screen.getByText(/cargando/i)).toBeInTheDocument();

    resolveGetPermissions({ entries: buildDefaultEntries() });
    await waitFor(() => expect(screen.queryByText(/cargando/i)).not.toBeInTheDocument());
  });

  it("muestra la matriz con los valores actuales y deshabilita la fila de mi propio rol", async () => {
    vi.spyOn(apiClient, "getPermissions").mockResolvedValue({ entries: buildDefaultEntries() });

    renderPage();

    const editorReportsCrear = await screen.findByLabelText(/crear.*reports.*editor/i);
    expect(editorReportsCrear).toBeChecked();

    const viewerUsersCrear = screen.getByLabelText(/crear.*users.*viewer/i);
    expect(viewerUsersCrear).not.toBeChecked();

    const adminUsersCrear = screen.getByLabelText(/crear.*users.*admin/i);
    expect(adminUsersCrear).toBeDisabled();
  });

  it("los cambios no se persisten hasta guardar explícitamente", async () => {
    const updateSpy = vi.spyOn(apiClient, "updatePermissions");
    vi.spyOn(apiClient, "getPermissions").mockResolvedValue({ entries: buildDefaultEntries() });

    renderPage();

    const editorUsersEliminar = await screen.findByLabelText(/eliminar.*users.*editor/i);
    fireEvent.click(editorUsersEliminar);

    expect(updateSpy).not.toHaveBeenCalled();
  });

  it("desactivo un permiso y guardo: llama a updatePermissions con el cambio y muestra confirmación", async () => {
    vi.spyOn(apiClient, "getPermissions").mockResolvedValue({ entries: buildDefaultEntries() });
    const updateSpy = vi.spyOn(apiClient, "updatePermissions").mockResolvedValue(undefined);

    renderPage();

    const editorReportsCrear = await screen.findByLabelText(/crear.*reports.*editor/i);
    fireEvent.click(editorReportsCrear);
    fireEvent.click(screen.getByRole("button", { name: /guardar cambios/i }));

    await waitFor(() =>
      expect(updateSpy).toHaveBeenCalledWith(
        [{ rol: "Editor", recurso: "Reports", accion: "Create", permitido: false }],
        "fake-token",
      ),
    );
    expect(await screen.findByText(/cambios guardados/i)).toBeInTheDocument();
  });

  it("fallo general al guardar: muestra un mensaje genérico y revierte los toggles", async () => {
    vi.spyOn(apiClient, "getPermissions").mockResolvedValue({ entries: buildDefaultEntries() });
    vi.spyOn(apiClient, "updatePermissions").mockRejectedValue(
      new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500),
    );

    renderPage();

    const editorReportsCrear = await screen.findByLabelText(/crear.*reports.*editor/i);
    fireEvent.click(editorReportsCrear);
    expect(editorReportsCrear).not.toBeChecked();

    fireEvent.click(screen.getByRole("button", { name: /guardar cambios/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
    await waitFor(() => expect(screen.getByLabelText(/crear.*reports.*editor/i)).toBeChecked());
  });

  it("no ofrece ninguna opción para agregar roles o recursos nuevos", async () => {
    vi.spyOn(apiClient, "getPermissions").mockResolvedValue({ entries: buildDefaultEntries() });

    renderPage();

    await screen.findByLabelText(/crear.*reports.*editor/i);
    expect(screen.queryByRole("button", { name: /agregar/i })).not.toBeInTheDocument();
  });
});
