namespace OnlineStore.Constants;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static bool IsValid(string? role) => role is Admin or Customer;
}
