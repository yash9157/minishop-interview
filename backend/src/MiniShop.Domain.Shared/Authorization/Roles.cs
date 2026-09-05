namespace MiniShop.Domain.Shared;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string SecurityAdmin = "SecurityAdmin";
    public const string Provisioner = "Provisioner";
    public const string Maker = "Maker";
    public const string Checker = "Checker";

    public static readonly string[] PortalRoles =
        [Admin, Employee, Manager, SecurityAdmin, Provisioner];
}
