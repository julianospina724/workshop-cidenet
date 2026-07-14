import { createContext, useContext, useState, type ReactNode } from "react";
import type { AuthUser } from "../api/client";

const STORAGE_KEY = "workshop-cidenet-auth";

interface StoredAuth {
  token: string;
  user: AuthUser;
}

interface AuthContextValue {
  token: string | null;
  user: AuthUser | null;
  login: (token: string, user: AuthUser) => void;
  logout: () => void;
  updateUser: (user: AuthUser) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function loadStoredAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as StoredAuth;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const stored = loadStoredAuth();
  const [token, setToken] = useState<string | null>(stored?.token ?? null);
  const [user, setUser] = useState<AuthUser | null>(stored?.user ?? null);

  function login(newToken: string, newUser: AuthUser) {
    setToken(newToken);
    setUser(newUser);
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: newToken, user: newUser }));
  }

  function logout() {
    setToken(null);
    setUser(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  function updateUser(newUser: AuthUser) {
    setUser(newUser);
    if (token) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ token, user: newUser }));
    }
  }

  return <AuthContext.Provider value={{ token, user, login, logout, updateUser }}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth debe usarse dentro de un AuthProvider");
  }
  return context;
}
