namespace EduFlow.Application.Constants;

public static class Roles
{
    public const string SysAdmin = "SysAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Instructor = "Instructor";
    public const string Student = "Student";

    public static readonly string[] All = [SysAdmin, TenantAdmin, Instructor, Student];
}
