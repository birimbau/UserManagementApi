using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add HTTP logging services
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
    options.CombineLogs = true;
});

// Add Problem Details for standardized error responses
builder.Services.AddProblemDetails();

// Add Authentication services
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Add built-in HTTP logging middleware
app.UseHttpLogging();

// Add standardized problem details exception handling
app.UseExceptionHandler();
app.UseStatusCodePages();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Static list to store users in memory
var users = new List<User>
{
    new User { Id = 1, Name = "John Doe", Age = 30, Email = "john.doe@example.com" },
    new User { Id = 2, Name = "Jane Smith", Age = 25, Email = "jane.smith@example.com" },
    new User { Id = 3, Name = "Bob Johnson", Age = 35, Email = "bob.johnson@example.com" }
};

// GET: Retrieve all users with pagination for performance
app.MapGet("/users", (int page = 1, int pageSize = 10) =>
{
    try
    {
        if (page <= 0 || pageSize <= 0 || pageSize > 100)
        {
            return Results.BadRequest("Page must be > 0 and pageSize must be between 1 and 100.");
        }

        var totalUsers = users.Count;
        var pagedUsers = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new
        {
            Users = pagedUsers,
            Page = page,
            PageSize = pageSize,
            TotalUsers = totalUsers,
            TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
        };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        // Log the exception in a real application
        return Results.Problem(
            detail: "An error occurred while retrieving users.",
            statusCode: 500
        );
    }
})
.WithName("GetAllUsers");

// GET: Retrieve a specific user by ID
app.MapGet("/users/{id}", (int id) =>
{
    try
    {
        if (id <= 0)
        {
            return Results.BadRequest("User ID must be greater than 0.");
        }

        var user = users.FirstOrDefault(u => u.Id == id);
        return user is not null ? Results.Ok(user) : Results.NotFound($"User with ID {id} not found.");
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: "An error occurred while retrieving the user.",
            statusCode: 500
        );
    }
})
.WithName("GetUserById");

// POST: Add a new user
app.MapPost("/users", (CreateUserRequest request) =>
{
    try
    {
        // Comprehensive validation
        var validationErrors = new List<string>();

        if (!ValidationHelper.IsValidName(request.Name))
        {
            validationErrors.Add("Name must be between 2 and 100 characters.");
        }

        if (!ValidationHelper.IsValidAge(request.Age))
        {
            validationErrors.Add("Age must be between 1 and 150.");
        }

        if (!ValidationHelper.IsValidEmail(request.Email))
        {
            validationErrors.Add("Please provide a valid email address.");
        }

        // Check for duplicate email
        if (users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            validationErrors.Add("A user with this email already exists.");
        }

        if (validationErrors.Any())
        {
            return Results.BadRequest(new { Errors = validationErrors });
        }

        var newUser = new User
        {
            Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1,
            Name = request.Name.Trim(),
            Age = request.Age,
            Email = request.Email.Trim().ToLowerInvariant()
        };

        users.Add(newUser);
        return Results.Created($"/users/{newUser.Id}", newUser);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: "An error occurred while creating the user.",
            statusCode: 500
        );
    }
})
.WithName("CreateUser");

// PUT: Update an existing user's details
app.MapPut("/users/{id}", (int id, UpdateUserRequest request) =>
{
    try
    {
        if (id <= 0)
        {
            return Results.BadRequest("User ID must be greater than 0.");
        }

        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return Results.NotFound($"User with ID {id} not found.");
        }

        // Comprehensive validation
        var validationErrors = new List<string>();

        if (!ValidationHelper.IsValidName(request.Name))
        {
            validationErrors.Add("Name must be between 2 and 100 characters.");
        }

        if (!ValidationHelper.IsValidAge(request.Age))
        {
            validationErrors.Add("Age must be between 1 and 150.");
        }

        if (!ValidationHelper.IsValidEmail(request.Email))
        {
            validationErrors.Add("Please provide a valid email address.");
        }

        // Check for duplicate email (excluding current user)
        if (users.Any(u => u.Id != id && u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            validationErrors.Add("A user with this email already exists.");
        }

        if (validationErrors.Any())
        {
            return Results.BadRequest(new { Errors = validationErrors });
        }

        user.Name = request.Name.Trim();
        user.Age = request.Age;
        user.Email = request.Email.Trim().ToLowerInvariant();

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: "An error occurred while updating the user.",
            statusCode: 500
        );
    }
})
.WithName("UpdateUser");

// DELETE: Remove a user by ID
app.MapDelete("/users/{id}", (int id) =>
{
    try
    {
        if (id <= 0)
        {
            return Results.BadRequest("User ID must be greater than 0.");
        }

        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return Results.NotFound($"User with ID {id} not found.");
        }

        users.Remove(user);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: "An error occurred while deleting the user.",
            statusCode: 500
        );
    }
})
.WithName("DeleteUser");

// Test endpoint to trigger exceptions (remove in production)
app.MapGet("/test/exception", () =>
{
    throw new InvalidOperationException("This is a test exception for global error handling!");
});

// Test endpoint for different exception types
app.MapGet("/test/notfound", () =>
{
    throw new KeyNotFoundException("Test resource not found!");
});

// Test endpoint for argument exception
app.MapGet("/test/badrequest", () =>
{
    throw new ArgumentException("Invalid argument provided!");
});

// Protected endpoint that requires authentication
app.MapGet("/users/protected", () =>
{
    return Results.Ok(new { message = "This is a protected endpoint!", timestamp = DateTime.UtcNow });
})
.RequireAuthorization()
.WithName("GetProtectedData");

app.Run();

// API Key Authentication Handler
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ValidApiKey = "your-secret-api-key-12345"; // In production, use configuration

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(ApiKeyHeaderName))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
        }

        var providedApiKey = Request.Headers[ApiKeyHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        if (providedApiKey != ValidApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim(ClaimTypes.NameIdentifier, "api-user")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Authentication scheme options
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

// User model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

// Request models for creating and updating users
public record CreateUserRequest(string Name, int Age, string Email);
public record UpdateUserRequest(string Name, int Age, string Email);

// Validation helper class
public static class ValidationHelper
{
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

    public static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.Length >= 2 &&
               name.Length <= 100;
    }

    public static bool IsValidAge(int age)
    {
        return age > 0 && age <= 150;
    }
}
