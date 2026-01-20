namespace UserManagementAPI.Helpers;

/// <summary>
/// Helper class for input validation
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates name (2-100 characters, not empty)
    /// </summary>
    public static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.Length >= 2 &&
               name.Length <= 100;
    }

    /// <summary>
    /// Validates age (1-150)
    /// </summary>
    public static bool IsValidAge(int age)
    {
        return age > 0 && age <= 150;
    }
}
