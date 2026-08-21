using OnlineStore.Constants;
using OnlineStore.Models;

namespace OnlineStore.Extensions;

public static class SessionExtensions
{
    public static void SignIn(this ISession session, Users user)
    {
        session.SetInt32(SessionKeys.UserId, user.Id);
        session.SetString(SessionKeys.UserName, user.Name);
        session.SetString(SessionKeys.UserRole, user.Role);
    }

    public static int? GetCurrentUserId(this ISession session) =>
        session.GetInt32(SessionKeys.UserId);

    public static string? GetCurrentUserRole(this ISession session) =>
        session.GetString(SessionKeys.UserRole);

    public static bool IsInRole(this ISession session, string role) =>
        string.Equals(session.GetCurrentUserRole(), role, StringComparison.Ordinal);
}
