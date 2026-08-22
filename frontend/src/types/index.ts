export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export interface Task {
  id: number;
  title: string;
  description?: string;
  status: string;
  priority: string;
  category?: string;
  dueDate?: string;
  assignedUserId: number;
  assignedUserName: string;
  createdById: number;
  createdByName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardStats {
  pendingCount: number;
  inProgressCount: number;
  completedCount: number;
}

export interface TaskListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  priority?: string;
  category?: string;
}

export interface TaskFormData {
  title: string;
  description?: string;
  status: string;
  priority: string;
  category?: string;
  dueDate?: string;
  assignedUserId?: number;
}

export interface RegisterFormData {
  name: string;
  email: string;
  password: string;
  passwordConfirm: string;
}

export interface LoginFormData {
  email: string;
  password: string;
}

export const ACCESS_TOKEN_KEY = 'accessToken';
export const REFRESH_TOKEN_KEY = 'refreshToken';
