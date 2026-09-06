export interface Role {
  id: string;
  name: string;
  isRequestable: boolean;
  isBuiltIn: boolean;
  permissionIds: number[];
}
export interface Permission {
  id: number;
  code: string;
  name: string;
}
export interface TargetSystem {
  id: number;
  name: string;
}
export interface User {
  id: string;
  fullName: string;
  email: string;
  managerId?: string;
  managerName?: string;
  isActive: boolean;
  roles: string[];
}
export interface Approval {
  id: number;
  level: number;
  approverId: string;
  approverName: string;
  decision: string;
  remarks?: string;
  decisionAtUtc?: string;
}
export interface AccessRequest {
  id: number;
  requesterId: string;
  requesterName: string;
  targetSystemId: number;
  targetSystem: string;
  requestedRoleId: string;
  requestedRole: string;
  businessJustification: string;
  status: string;
  createdAtUtc: string;
  submittedAtUtc?: string;
  provisionedById?: string;
  provisionedByName?: string;
  provisionedAtUtc?: string;
  approvals: Approval[];
}
export interface AuditLog {
  id: number;
  user: string;
  action: string;
  entity: string;
  entityId: string;
  timestampUtc: string;
  oldValue?: string;
  newValue?: string;
}
export interface Dashboard {
  pendingApprovals: number;
  requestsByStatus: Record<string, number>;
  usersByRole: Record<string, number>;
  latestAuditLogs: AuditLog[];
}
