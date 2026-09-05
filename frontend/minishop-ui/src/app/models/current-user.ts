export interface CurrentUser {
  id: string;
  fullName: string;
  email: string;
  tenantId: number;
  tenantName: string;
  roles: string[];
}
