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

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
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
    throw new ApiError(body?.message ?? GENERIC_ERROR_MESSAGE, response.status);
  }

  return response.json();
}
