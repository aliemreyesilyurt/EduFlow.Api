using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

namespace EduFlow.Application.Features.CourseFeature;

internal static class CourseAccess
{
    public static bool CanManage(Course course, ITenantContext tenantContext) =>
        tenantContext.IsSysAdmin
        || tenantContext.IsInRole(Roles.TenantAdmin)
        || course.InstructorId == tenantContext.UserId;

    public static bool CanView(Course course, ITenantContext tenantContext) =>
        course.Status == CourseStatus.Published || CanManage(course, tenantContext);
}
