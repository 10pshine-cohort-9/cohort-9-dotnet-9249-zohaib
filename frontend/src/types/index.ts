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

