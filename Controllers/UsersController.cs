using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Helpers;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

/// <summary>
/// Controller for managing user operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PagedUsersResponse> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0 || pageSize <= 0 || pageSize > 100)
        {
            return BadRequest("Page must be > 0 and pageSize must be between 1 and 100.");
        }

        var result = _userService.GetAllUsers(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetUserById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be greater than 0.");
        }

        var user = _userService.GetUserById(id);
        if (user is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<User> CreateUser([FromBody] CreateUserRequest request)
    {
        var validationErrors = ValidateUserRequest(request.Name, request.Age, request.Email);

        // Check for duplicate email
        if (_userService.EmailExists(request.Email))
        {
            validationErrors.Add("A user with this email already exists.");
        }

        if (validationErrors.Any())
        {
            return BadRequest(new { Errors = validationErrors });
        }

        var newUser = _userService.CreateUser(request);
        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update request</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be greater than 0.");
        }

        // Check if user exists
        var existingUser = _userService.GetUserById(id);
        if (existingUser is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        var validationErrors = ValidateUserRequest(request.Name, request.Age, request.Email);

        // Check for duplicate email (excluding current user)
        if (_userService.EmailExists(request.Email, id))
        {
            validationErrors.Add("A user with this email already exists.");
        }

        if (validationErrors.Any())
        {
            return BadRequest(new { Errors = validationErrors });
        }

        var updatedUser = _userService.UpdateUser(id, request);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Delete a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest("User ID must be greater than 0.");
        }

        var deleted = _userService.DeleteUser(id);
        if (!deleted)
        {
            return NotFound($"User with ID {id} not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Protected endpoint that requires authentication
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProtectedData()
    {
        return Ok(new { message = "This is a protected endpoint!", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Validates user input fields
    /// </summary>
    private static List<string> ValidateUserRequest(string name, int age, string email)
    {
        var errors = new List<string>();

        if (!ValidationHelper.IsValidName(name))
        {
            errors.Add("Name must be between 2 and 100 characters.");
        }

        if (!ValidationHelper.IsValidAge(age))
        {
            errors.Add("Age must be between 1 and 150.");
        }

        if (!ValidationHelper.IsValidEmail(email))
        {
            errors.Add("Please provide a valid email address.");
        }

        return errors;
    }
}
