using Microsoft.AspNetCore.Mvc;

namespace UserManagementAPI.Controllers;

/// <summary>
/// Controller for testing exception handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test endpoint to trigger a general exception
    /// </summary>
    [HttpGet("exception")]
    public IActionResult TriggerException()
    {
        throw new InvalidOperationException("This is a test exception for global error handling!");
    }

    /// <summary>
    /// Test endpoint to trigger a not found exception
    /// </summary>
    [HttpGet("notfound")]
    public IActionResult TriggerNotFound()
    {
        throw new KeyNotFoundException("Test resource not found!");
    }

    /// <summary>
    /// Test endpoint to trigger a bad request exception
    /// </summary>
    [HttpGet("badrequest")]
    public IActionResult TriggerBadRequest()
    {
        throw new ArgumentException("Invalid argument provided!");
    }
}
