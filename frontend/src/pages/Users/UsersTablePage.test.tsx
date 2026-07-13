import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider } from "../../auth/AuthContext";
import * as apiClient from "../../api/client";
import UsersTablePage from "./UsersTablePage";

function renderPage() {
  localStorage.setItem(
    "workshop-cidenet-auth",
    JSON.stringify({ token: "fake-token", user: { id: "1", nombre: "Ana", email: "ana@mail.com", rol: "Admin", estado: "Activo" } }),
  );

  return render(
    <MemoryRouter>
      <AuthProvider>
        <UsersTablePage />
      </AuthProvider>
    </MemoryRouter>,
  );
}

const twoUsers = {
  items: [
    { id: "1", nombre: "Ana Pérez", email: "ana@mail.com", rol: "Admin", estado: "Activo", createdAt: "2026-01-01T00:00:00Z" },
    { id: "2", nombre: "Beto Ruiz", email: "beto@mail.com", rol: "Editor", estado: "Activo", createdAt: "2026-01-02T00:00:00Z" },
  ],
  totalCount: 2,
  page: 1,
  pageSize: 20,
};

describe("UsersTablePage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("muestra la tabla con el orden que devuelve el backend tras cargar", async () => {
    vi.spyOn(apiClient, "getUsers").mockResolvedValue(twoUsers);

    renderPage();

    await waitFor(() => expect(screen.getByText("Ana Pérez")).toBeInTheDocument());
    const rows = screen.getAllByRole("row").slice(1); // sin el encabezado
    expect(rows[0]).toHaveTextContent("Ana Pérez");
    expect(rows[1]).toHaveTextContent("Beto Ruiz");
  });

  it("muestra un spinner mientras carga", async () => {
    let resolveGetUsers!: (value: typeof twoUsers) => void;
    vi.spyOn(apiClient, "getUsers").mockReturnValue(
      new Promise((resolve) => {
        resolveGetUsers = resolve;
      }),
    );

    renderPage();

    expect(screen.getByText(/cargando/i)).toBeInTheDocument();

    resolveGetUsers(twoUsers);
    await waitFor(() => expect(screen.queryByText(/cargando/i)).not.toBeInTheDocument());
  });

  it("combina el filtro de rol con la búsqueda de nombre", async () => {
    const getUsersSpy = vi.spyOn(apiClient, "getUsers").mockResolvedValue(twoUsers);

    renderPage();
    await waitFor(() => expect(getUsersSpy).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/rol/i), { target: { value: "Editor" } });
    fireEvent.change(screen.getByLabelText(/^nombre$/i), { target: { value: "juan" } });
    fireEvent.click(screen.getByRole("button", { name: /filtrar/i }));

    await waitFor(() => expect(getUsersSpy).toHaveBeenCalledTimes(2));
    expect(getUsersSpy).toHaveBeenLastCalledWith(
      expect.objectContaining({ rol: "Editor", nombre: "juan", page: 1 }),
      "fake-token",
    );
  });

  it("filtra por estado", async () => {
    const getUsersSpy = vi.spyOn(apiClient, "getUsers").mockResolvedValue(twoUsers);

    renderPage();
    await waitFor(() => expect(getUsersSpy).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/estado/i), { target: { value: "Inactivo" } });
    fireEvent.click(screen.getByRole("button", { name: /filtrar/i }));

    await waitFor(() =>
      expect(getUsersSpy).toHaveBeenLastCalledWith(expect.objectContaining({ estado: "Inactivo" }), "fake-token"),
    );
  });

  it("el datepicker de 'hasta' restringe fechas anteriores a 'desde'", async () => {
    vi.spyOn(apiClient, "getUsers").mockResolvedValue(twoUsers);

    renderPage();
    await waitFor(() => expect(apiClient.getUsers).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/fecha desde/i), { target: { value: "2026-03-01" } });

    expect(screen.getByLabelText(/fecha hasta/i)).toHaveAttribute("min", "2026-03-01");
  });

  it("muestra un mensaje cuando no hay resultados", async () => {
    vi.spyOn(apiClient, "getUsers").mockResolvedValue({ items: [], totalCount: 0, page: 1, pageSize: 20 });

    renderPage();

    expect(await screen.findByText(/no se encontraron registros/i)).toBeInTheDocument();
  });

  it("muestra un mensaje general ante un error técnico", async () => {
    vi.spyOn(apiClient, "getUsers").mockRejectedValue(new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500));

    renderPage();

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });

  it("cambiar el tamaño de página reinicia a la primera página", async () => {
    const getUsersSpy = vi.spyOn(apiClient, "getUsers").mockResolvedValue(twoUsers);

    renderPage();
    await waitFor(() => expect(getUsersSpy).toHaveBeenCalledTimes(1));

    fireEvent.change(screen.getByLabelText(/registros por página/i), { target: { value: "50" } });

    await waitFor(() =>
      expect(getUsersSpy).toHaveBeenLastCalledWith(expect.objectContaining({ pageSize: 50, page: 1 }), "fake-token"),
    );
  });
});
