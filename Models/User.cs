namespace UserManagementAPI.Models;

/// <summary>
/// User entity model
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a new user
/// </summary>
public record CreateUserRequest(string Name, int Age, string Email);

/// <summary>
/// Request model for updating an existing user
/// </summary>
public record UpdateUserRequest(string Name, int Age, string Email);

/// <summary>
/// Response model for paginated user list
/// </summary>
public class PagedUsersResponse
{
    public List<User> Users { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPages { get; set; }
}
