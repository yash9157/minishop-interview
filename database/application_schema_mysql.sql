CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetRoles` (
        `Id` char(36) NOT NULL,
        `IsRequestable` tinyint(1) NOT NULL DEFAULT FALSE,
        `Name` varchar(256) NULL,
        `NormalizedName` varchar(256) NULL,
        `ConcurrencyStamp` longtext NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetUsers` (
        `Id` char(36) NOT NULL,
        `FullName` varchar(120) NOT NULL,
        `ManagerId` char(36) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `DeletedAtUtc` datetime(6) NULL,
        `UserName` varchar(256) NULL,
        `NormalizedUserName` varchar(256) NULL,
        `Email` varchar(256) NULL,
        `NormalizedEmail` varchar(256) NULL,
        `EmailConfirmed` tinyint(1) NOT NULL,
        `PasswordHash` longtext NULL,
        `SecurityStamp` longtext NULL,
        `ConcurrencyStamp` longtext NULL,
        `PhoneNumber` longtext NULL,
        `PhoneNumberConfirmed` tinyint(1) NOT NULL,
        `TwoFactorEnabled` tinyint(1) NOT NULL,
        `LockoutEnd` datetime NULL,
        `LockoutEnabled` tinyint(1) NOT NULL,
        `AccessFailedCount` int NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AspNetUsers_AspNetUsers_ManagerId` FOREIGN KEY (`ManagerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `Permissions` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `Code` varchar(80) NOT NULL,
        `Name` varchar(120) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `TargetSystems` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `Name` varchar(120) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetRoleClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `RoleId` char(36) NOT NULL,
        `ClaimType` longtext NULL,
        `ClaimValue` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetUserClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` char(36) NOT NULL,
        `ClaimType` longtext NULL,
        `ClaimValue` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetUserLogins` (
        `LoginProvider` varchar(255) NOT NULL,
        `ProviderKey` varchar(255) NOT NULL,
        `ProviderDisplayName` longtext NULL,
        `UserId` char(36) NOT NULL,
        PRIMARY KEY (`LoginProvider`, `ProviderKey`),
        CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetUserRoles` (
        `UserId` char(36) NOT NULL,
        `RoleId` char(36) NOT NULL,
        PRIMARY KEY (`UserId`, `RoleId`),
        CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AspNetUserTokens` (
        `UserId` char(36) NOT NULL,
        `LoginProvider` varchar(255) NOT NULL,
        `Name` varchar(255) NOT NULL,
        `Value` longtext NULL,
        PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
        CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AuditLogs` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `UserId` char(36) NULL,
        `Action` varchar(60) NOT NULL,
        `Entity` varchar(80) NOT NULL,
        `EntityId` varchar(80) NOT NULL,
        `TimestampUtc` datetime(6) NOT NULL,
        `OldValue` longtext NULL,
        `NewValue` longtext NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AuditLogs_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `RolePermissions` (
        `RoleId` char(36) NOT NULL,
        `PermissionId` bigint NOT NULL,
        PRIMARY KEY (`RoleId`, `PermissionId`),
        CONSTRAINT `FK_RolePermissions_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `AccessRequests` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `RequesterId` char(36) NOT NULL,
        `TargetSystemId` bigint NOT NULL,
        `RequestedRoleId` char(36) NOT NULL,
        `BusinessJustification` varchar(1000) NOT NULL,
        `Status` varchar(20) NOT NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        `SubmittedAtUtc` datetime(6) NULL,
        `ProvisionedById` char(36) NULL,
        `ProvisionedAtUtc` datetime(6) NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccessRequests_AspNetRoles_RequestedRoleId` FOREIGN KEY (`RequestedRoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_AccessRequests_AspNetUsers_ProvisionedById` FOREIGN KEY (`ProvisionedById`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_AccessRequests_AspNetUsers_RequesterId` FOREIGN KEY (`RequesterId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_AccessRequests_TargetSystems_TargetSystemId` FOREIGN KEY (`TargetSystemId`) REFERENCES `TargetSystems` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE TABLE `ApprovalHistory` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `AccessRequestId` bigint NOT NULL,
        `Level` int NOT NULL,
        `ApproverId` char(36) NOT NULL,
        `Decision` varchar(20) NOT NULL,
        `Remarks` varchar(500) NULL,
        `DecisionAtUtc` datetime(6) NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ApprovalHistory_AccessRequests_AccessRequestId` FOREIGN KEY (`AccessRequestId`) REFERENCES `AccessRequests` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ApprovalHistory_AspNetUsers_ApproverId` FOREIGN KEY (`ApproverId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AccessRequests_ProvisionedById` ON `AccessRequests` (`ProvisionedById`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AccessRequests_RequestedRoleId` ON `AccessRequests` (`RequestedRoleId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AccessRequests_RequesterId_CreatedAtUtc` ON `AccessRequests` (`RequesterId`, `CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AccessRequests_Status_CreatedAtUtc` ON `AccessRequests` (`Status`, `CreatedAtUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AccessRequests_TargetSystemId` ON `AccessRequests` (`TargetSystemId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE UNIQUE INDEX `IX_ApprovalHistory_AccessRequestId_Level` ON `ApprovalHistory` (`AccessRequestId`, `Level`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_ApprovalHistory_ApproverId_Decision` ON `ApprovalHistory` (`ApproverId`, `Decision`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetUsers_IsDeleted_IsActive` ON `AspNetUsers` (`IsDeleted`, `IsActive`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AspNetUsers_ManagerId` ON `AspNetUsers` (`ManagerId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AuditLogs_TimestampUtc` ON `AuditLogs` (`TimestampUtc`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_AuditLogs_UserId` ON `AuditLogs` (`UserId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE UNIQUE INDEX `IX_Permissions_Code` ON `Permissions` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE INDEX `IX_RolePermissions_PermissionId` ON `RolePermissions` (`PermissionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    CREATE UNIQUE INDEX `IX_TargetSystems_Name` ON `TargetSystems` (`Name`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905163314_InitialAccessManagement')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260905163314_InitialAccessManagement', '8.0.30');
END;

COMMIT;

START TRANSACTION;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905164023_AddIdempotencyRecords')
BEGIN
    CREATE TABLE `IdempotencyRecords` (
        `Id` bigint NOT NULL AUTO_INCREMENT,
        `Key` varchar(100) NOT NULL,
        `Operation` varchar(80) NOT NULL,
        `ResourceId` varchar(80) NOT NULL,
        `CreatedAtUtc` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905164023_AddIdempotencyRecords')
BEGIN
    CREATE UNIQUE INDEX `IX_IdempotencyRecords_Operation_Key` ON `IdempotencyRecords` (`Operation`, `Key`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905164023_AddIdempotencyRecords')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260905164023_AddIdempotencyRecords', '8.0.30');
END;

COMMIT;

START TRANSACTION;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905182728_HardenAccessManagement')
BEGIN
    ALTER TABLE `IdempotencyRecords` ADD `RequestHash` varchar(64) NOT NULL DEFAULT '';
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905182728_HardenAccessManagement')
BEGIN
    ALTER TABLE `IdempotencyRecords` ADD `ResponseJson` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905182728_HardenAccessManagement')
BEGIN
    ALTER TABLE `IdempotencyRecords` ADD `StatusCode` int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905182728_HardenAccessManagement')
BEGIN
    ALTER TABLE `AccessRequests` ADD `Version` bigint NOT NULL DEFAULT 0;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905182728_HardenAccessManagement')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260905182728_HardenAccessManagement', '8.0.30');
END;

COMMIT;

START TRANSACTION;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `Permissions` MODIFY `Name` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `IdempotencyRecords` MODIFY `ResourceId` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `IdempotencyRecords` MODIFY `RequestHash` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AuditLogs` MODIFY `EntityId` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AuditLogs` MODIFY `Entity` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AuditLogs` MODIFY `Action` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AspNetUsers` MODIFY `FullName` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `ApprovalHistory` MODIFY `Remarks` longtext NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `ApprovalHistory` MODIFY `Decision` int NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AccessRequests` MODIFY `Status` int NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    ALTER TABLE `AccessRequests` MODIFY `BusinessJustification` longtext NOT NULL;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260905193706_SimplifyEntityConfigurations')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260905193706_SimplifyEntityConfigurations', '8.0.30');
END;

COMMIT;

