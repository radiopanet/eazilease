using Microsoft.AspNetCore.Http;

namespace EaziLease.Extensions;
public static class UserExtensions
{
    public static bool IsSuperAdminElevated(this HttpContext context)
    {
        return context.Session.GetString("IsSuperAdmin") == "true";
    }
}