import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as apiClient from "../../api/client";
import DeleteUserModal from "./DeleteUserModal";

const targetUser = {
  id: "u-2",
  nombre: "Ana Pérez",
  email: "ana.perez@mail.com",
  rol: "Editor",
  estado: "Activo",
  createdAt: "2026-01-01T00:00:00Z",
};

describe("DeleteUserModal", () => {
  const onCancel = vi.fn();
  const onDeleted = vi.fn();

  beforeEach(() => {
    vi.restoreAllMocks();
    onCancel.mockClear();
    onDeleted.mockClear();
  });

  it("muestra la información del usuario a eliminar", () => {
    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);

    expect(screen.getByText(/ana pérez/i)).toBeInTheDocument();
    expect(screen.getByText(/ana\.perez@mail\.com/i)).toBeInTheDocument();
    expect(screen.getByText(/editor/i)).toBeInTheDocument();
  });

  it("cancelar no llama a deleteUser", () => {
    const deleteSpy = vi.spyOn(apiClient, "deleteUser");

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /cancelar/i }));

    expect(deleteSpy).not.toHaveBeenCalled();
    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it("confirmar elimina al usuario y notifica onDeleted", async () => {
    const deleteSpy = vi.spyOn(apiClient, "deleteUser").mockResolvedValue(undefined);

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    await waitFor(() => expect(onDeleted).toHaveBeenCalledTimes(1));
    expect(deleteSpy).toHaveBeenCalledWith("u-2", "fake-token");
  });

  it("muestra un indicador de carga mientras se procesa", async () => {
    let resolveDelete!: () => void;
    vi.spyOn(apiClient, "deleteUser").mockReturnValue(
      new Promise((resolve) => {
        resolveDelete = () => resolve(undefined);
      }),
    );

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(screen.getByRole("button", { name: /eliminando/i })).toBeDisabled();

    resolveDelete();
    await waitFor(() => expect(onDeleted).toHaveBeenCalledTimes(1));
  });

  it("si es el único Admin activo, muestra el mensaje del backend y no cierra el modal", async () => {
    vi.spyOn(apiClient, "deleteUser").mockRejectedValue(
      new apiClient.ApiError("Debe existir al menos un Admin activo en el sistema.", 400),
    );

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/al menos un Admin activo/i);
    expect(onDeleted).not.toHaveBeenCalled();
  });

  it("usuario ya no existe: muestra un mensaje explicando que ya no existe", async () => {
    vi.spyOn(apiClient, "deleteUser").mockRejectedValue(new apiClient.ApiError("El usuario ya no existe.", 404));

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ya no existe/i);
  });

  it("fallo general del backend: muestra un mensaje genérico", async () => {
    vi.spyOn(apiClient, "deleteUser").mockRejectedValue(new apiClient.ApiError("Ocurrió un error inesperado. Intenta de nuevo.", 500));

    render(<DeleteUserModal user={targetUser} token="fake-token" onCancel={onCancel} onDeleted={onDeleted} />);
    fireEvent.click(screen.getByRole("button", { name: /confirmar eliminación/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/ocurrió un error inesperado/i);
  });
});
