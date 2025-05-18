/**
 * Authentication related models
 */

/**
 * Authentication result from the server
 */
export interface AuthResult {
  success: boolean;
  token: string;
  username: string;
  roles: string[];
  error?: string;
}

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Registration request payload
 */
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  accountType: string;
}

/**
 * User model
 */
export interface User {
  id?: string;
  username: string;
  email: string;
  roles: string[];
}
