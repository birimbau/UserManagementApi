using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>
/// Interface for user service operations
/// </summary>
public interface IUserService
{
    PagedUsersResponse GetAllUsers(int page, int pageSize);
    User? GetUserById(int id);
    User CreateUser(CreateUserRequest request);
    User? UpdateUser(int id, UpdateUserRequest request);
    bool DeleteUser(int id);
    bool EmailExists(string email, int? excludeUserId = null);
}

/// <summary>
/// Service for managing user operations with in-memory storage
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users;
    private readonly object _lock = new();

    public UserService()
    {
        _users = new List<User>
        {
            new User { Id = 1, Name = "John Doe", Age = 30, Email = "john.doe@example.com" },
            new User { Id = 2, Name = "Jane Smith", Age = 25, Email = "jane.smith@example.com" },
            new User { Id = 3, Name = "Bob Johnson", Age = 35, Email = "bob.johnson@example.com" }
        };
    }

    public PagedUsersResponse GetAllUsers(int page, int pageSize)
    {
        lock (_lock)
        {
            var totalUsers = _users.Count;
            var pagedUsers = _users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedUsersResponse
            {
                Users = pagedUsers,
                Page = page,
                PageSize = pageSize,
                TotalUsers = totalUsers,
                TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
            };
        }
    }

    public User? GetUserById(int id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    public User CreateUser(CreateUserRequest request)
    {
        lock (_lock)
        {
            var newUser = new User
            {
                Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                Name = request.Name.Trim(),
                Age = request.Age,
                Email = request.Email.Trim().ToLowerInvariant()
            };

            _users.Add(newUser);
            return newUser;
        }
    }

    public User? UpdateUser(int id, UpdateUserRequest request)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return null;

            user.Name = request.Name.Trim();
            user.Age = request.Age;
            user.Email = request.Email.Trim().ToLowerInvariant();

            return user;
        }
    }

    public bool DeleteUser(int id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return false;

            _users.Remove(user);
            return true;
        }
    }

    public bool EmailExists(string email, int? excludeUserId = null)
    {
        lock (_lock)
        {
            return _users.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
        }
    }
}
