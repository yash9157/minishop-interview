-- 1. Active employees who did not log in during the last 30 days.
SELECT e.EmployeeId, e.Name
FROM Employees e
WHERE e.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 FROM LoginAudit la
      WHERE la.EmployeeId = e.EmployeeId
        AND la.LoginTime >= DATEADD(DAY, -30, SYSUTCDATETIME())
  );

-- 2. Department-wise active employee count.
SELECT d.DepartmentId, d.DepartmentName, COUNT(e.EmployeeId) AS ActiveEmployeeCount
FROM Departments d
LEFT JOIN Employees e
    ON e.DepartmentId = d.DepartmentId AND e.IsActive = 1
GROUP BY d.DepartmentId, d.DepartmentName;

-- 3. Latest successful login per employee.
SELECT e.EmployeeId, e.Name, MAX(la.LoginTime) AS LatestSuccessfulLogin
FROM Employees e
LEFT JOIN LoginAudit la
    ON la.EmployeeId = e.EmployeeId AND la.SuccessFlag = 1
GROUP BY e.EmployeeId, e.Name;

-- 4. Employees with active access to more than one system.
SELECT e.EmployeeId, e.Name, COUNT(DISTINCT ag.SystemName) AS ActiveSystemCount
FROM Employees e
JOIN AccessGrant ag ON ag.EmployeeId = e.EmployeeId
WHERE ag.RevokedOn IS NULL
GROUP BY e.EmployeeId, e.Name
HAVING COUNT(DISTINCT ag.SystemName) > 1;

IF OBJECT_ID('dbo.AccessAudit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AccessAudit (
        AuditId BIGINT IDENTITY PRIMARY KEY,
        EmployeeId INT NOT NULL,
        ActionName VARCHAR(50) NOT NULL,
        AffectedGrantCount INT NOT NULL,
        ActionOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
GO

-- 5. Revoke active grants and log the action atomically.
CREATE OR ALTER PROCEDURE dbo.RevokeAllEmployeeAccess @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        UPDATE AccessGrant
        SET RevokedOn = SYSUTCDATETIME()
        WHERE EmployeeId = @EmployeeId AND RevokedOn IS NULL;
        DECLARE @AffectedRows INT = @@ROWCOUNT;
        INSERT INTO dbo.AccessAudit(EmployeeId, ActionName, AffectedGrantCount)
        VALUES (@EmployeeId, 'REVOKE_ALL_ACCESS', @AffectedRows);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 6. Query 1 uses NOT EXISTS for an index seek. Query 3 uses this covering index.
CREATE INDEX IX_LoginAudit_Employee_LoginTime
ON LoginAudit(EmployeeId, LoginTime DESC)
INCLUDE (SuccessFlag);
