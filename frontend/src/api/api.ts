import axios from 'axios';
import type { 
  AuthResponse,
  LoginFormData,
  RegisterFormData,
  User,
  DashboardStats,
  PagedResult,
  Task,
  TaskFormData,
  TaskListQuery,
} from '../types';

const ACCESS_TOKEN_KEY = 'accessToken';
const REFRESH_TOKEN_KEY = 'refreshToken';

const API_BASE_URL = 'http://localhost:5114/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Separate client for refreshing tokens
const refreshClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Save tokens
export const setAuthTokens = (
  accessToken: string,
  refreshToken: string
) => {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
};

// Remove tokens
export const clearAuthTokens = () => {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
};

// Add access token to every request
apiClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

// Handle API responses
apiClient.interceptors.response.use(
  (response) => response,

  async (error) => {
    const originalRequest = error.config as typeof error.config & {
      _retry?: boolean;
    };

    const status = error.response?.status;
    const requestUrl = originalRequest?.url ?? '';

    // Not a 401 → return normal error
    if (status !== 401) {
      const message =
        error.response?.data?.message ||
        error.response?.data?.title ||
        'An unexpected error occurred.';

      return Promise.reject(new Error(message));
    }

    // Don't refresh authentication endpoints
    if (
      requestUrl.includes('/auth/login') ||
      requestUrl.includes('/auth/register') ||
      requestUrl.includes('/auth/refresh')
    ) {
      const message =
        error.response?.data?.message ||
        'Authentication failed.';

      return Promise.reject(new Error(message));
    }

    // Prevent infinite retry loop
    if (originalRequest._retry) {
      clearAuthTokens();
      window.location.href = '/login';

      return Promise.reject(
        new Error('Session expired.')
      );
    }

    // Get refresh token
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    // No refresh token
    if (!refreshToken) {
      clearAuthTokens();
      window.location.href = '/login';

      return Promise.reject(
        new Error('Session expired.')
      );
    }

    // Mark request as retried
    originalRequest._retry = true;

    try {
      // Request new tokens
      const { data } = await refreshClient.post<{
        accessToken: string;
        refreshToken: string;
      }>('/auth/refresh', {
        refreshToken,
      });

      // Save new tokens
      setAuthTokens(
        data.accessToken,
        data.refreshToken
      );

      // Add new access token to original request
      originalRequest.headers.Authorization =
        `Bearer ${data.accessToken}`;

      // Retry original request
      return apiClient(originalRequest);

    } catch {
      // Refresh token failed
      clearAuthTokens();

      // Redirect to login
      window.location.href = '/login';

      return Promise.reject(
        new Error('Session expired.')
      );
    }
  }
);


export const authApi = {
  register: (data: RegisterFormData) =>
    apiClient.post<AuthResponse>('/auth/register', data).then((r) => r.data),

  login: (data: LoginFormData) =>
    apiClient.post<AuthResponse>('/auth/login', data).then((r) => r.data),

  refresh: (refreshToken: string) =>
    apiClient.post<AuthResponse>('/auth/refresh', { refreshToken }).then((r) => r.data),

  logout: (refreshToken: string) =>
    apiClient.post('/auth/logout', { refreshToken }),

  getMe: () => apiClient.get<User>('/auth/profile').then((r) => r.data),
};


export const tasksApi = {
  getTasks: (query: TaskListQuery) =>
    apiClient.get<PagedResult<Task>>('/tasks', { params: query }).then((r) => r.data),

  getTask: (id: number) => apiClient.get<Task>(`/tasks/${id}`).then((r) => r.data),

  createTask: (data: TaskFormData) =>
    apiClient.post<Task>('/tasks', data).then((r) => r.data),

  updateTask: (id: number, data: TaskFormData) =>
    apiClient.put<Task>(`/tasks/${id}`, data).then((r) => r.data),

  deleteTask: (id: number) => apiClient.delete(`/tasks/${id}`),

  getDashboardStats: () =>
    apiClient.get<DashboardStats>('/tasks/dashboard-stats').then((r) => r.data),

  getAssignableUsers: () =>
    apiClient.get<User[]>('/tasks/assignable-users').then((r) => r.data),
};
