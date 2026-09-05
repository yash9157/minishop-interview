import { CurrentUser } from './current-user';

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: CurrentUser;
}
