const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export async function checkHealth(): Promise<{ status: string }> {
  const response = await fetch(`${API_BASE_URL}/health`);
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }
  return response.json();
}

export class ApiError extends Error {
  status: number;
  fieldErrors?: Record<string, string>;

  constructor(message: string, status: number, fieldErrors?: Record<string, string>) {
    super(message);
    this.status = status;
    this.fieldErrors = fieldErrors;
  }
}

export interface AuthUser {
  id: string;
  nombre: string;
  email: string;
  rol: string;
  estado: string;
}

export interface LoginResponse {
  token: string;
  user: AuthUser;
}

const GENERIC_ERROR_MESSAGE = "Ocurrió un error inesperado. Intenta de nuevo.";

export async function login(email: string, password: string): Promise<LoginResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status, body?.fieldErrors);
  }

  return response.json();
}

export interface UserRecord {
  id: string;
  nombre: string;
  email: string;
  rol: string;
  estado: string;
  createdAt: string;
}

export interface UsersQuery {
  rol?: string;
  nombre?: string;
  email?: string;
  estado?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  page: number;
  pageSize: number;
}

export interface PagedUsers {
  items: UserRecord[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function getUsers(query: UsersQuery, token: string): Promise<PagedUsers> {
  const params = new URLSearchParams();
  if (query.rol) params.set("rol", query.rol);
  if (query.nombre) params.set("nombre", query.nombre);
  if (query.email) params.set("email", query.email);
  if (query.estado) params.set("estado", query.estado);
  if (query.fechaDesde) params.set("fechaDesde", query.fechaDesde);
  if (query.fechaHasta) params.set("fechaHasta", query.fechaHasta);
  params.set("page", String(query.page));
  params.set("pageSize", String(query.pageSize));

  const response = await fetch(`${API_BASE_URL}/api/users?${params.toString()}`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status);
  }

  return response.json();
}

export interface CreateUserPayload {
  nombre: string;
  email: string;
  password: string;
  confirmPassword: string;
  rol: string;
}

export async function createUser(payload: CreateUserPayload, token: string): Promise<UserRecord> {
  const response = await fetch(`${API_BASE_URL}/api/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status, body?.fieldErrors);
  }

  return response.json();
}

export interface EditUserPayload {
  nombre: string;
  email: string;
  rol?: string;
  estado?: string;
}

export async function editUser(id: string, payload: EditUserPayload, token: string): Promise<UserRecord> {
  const response = await fetch(`${API_BASE_URL}/api/users/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status, body?.fieldErrors);
  }

  return response.json();
}

export async function deleteUser(id: string, token: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/users/${id}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status);
  }
}

export interface EditProfilePayload {
  nombre: string;
  email: string;
}

export async function editProfile(payload: EditProfilePayload, token: string): Promise<UserRecord> {
  const response = await fetch(`${API_BASE_URL}/api/users/me`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status, body?.fieldErrors);
  }

  return response.json();
}

export interface PermissionEntry {
  rol: string;
  recurso: string;
  accion: string;
  permitido: boolean;
}

export interface PermissionMatrixResponse {
  entries: PermissionEntry[];
}

export async function getPermissions(token: string): Promise<PermissionMatrixResponse> {
  const response = await fetch(`${API_BASE_URL}/api/permissions`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status);
  }

  return response.json();
}

export async function updatePermissions(changes: PermissionEntry[], token: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/permissions`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
    body: JSON.stringify({ changes }),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status);
  }
}
