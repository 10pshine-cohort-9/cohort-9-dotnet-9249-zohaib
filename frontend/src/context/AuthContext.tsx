import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { authApi } from '../api/api';
import { clearAuthTokens, setAuthTokens } from '../api/api';
import type { LoginFormData, RegisterFormData, User } from '../types';


interface AuthContextValue {
  user: User | null;
  accessToken: string | null;
  isLoading: boolean;
  isAdmin: boolean;
  login: (data: LoginFormData) => Promise<void>;
  register: (data: RegisterFormData) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [accessToken, setAccessTokenState] = useState<string | null>(localStorage.getItem('accessToken'));
  const [isLoading, setIsLoading] = useState(true);

  const loadCurrentUser = async () => {
    const currentUser = await authApi.getMe();
    setUser(currentUser);
    return currentUser;
  };

  useEffect(() => {
    const initializeAuth = async () => {
      const token = localStorage.getItem('accessToken');
      const refreshToken = localStorage.getItem('refreshToken');

      if (!token || !refreshToken) {
        clearAuthTokens();
        setAccessTokenState(null);
        setUser(null);
        setIsLoading(false);
        return;
      }

      try {
        await loadCurrentUser();
        setAccessTokenState(token);
      } catch {
        clearAuthTokens();
        setAccessTokenState(null);
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (data: LoginFormData) => {
    const response = await authApi.login(data);
    setAuthTokens(response.accessToken, response.refreshToken);
    setAccessTokenState(response.accessToken);
    await loadCurrentUser();
  };

  const register = async (data: RegisterFormData) => {
    const response = await authApi.register(data);
    setAuthTokens(response.accessToken, response.refreshToken);
    setAccessTokenState(response.accessToken);
    await loadCurrentUser();
  };

  const logout = async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    try {
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } finally {
      clearAuthTokens();
      setAccessTokenState(null);
      setUser(null);
    }
  };

  const value = useMemo(
    () => ({
      user,
      accessToken,
      isLoading,
      isAdmin: user?.role?.toLowerCase() === 'admin',
      login,
      register,
      logout,
    }),
    [user, accessToken, isLoading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
