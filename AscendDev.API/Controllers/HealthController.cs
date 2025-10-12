using AscendDev.Core.DTOs;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly IEmailService _emailService;

    public HealthController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
    [Route("[controller]")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetHealth()
    {
        return Ok("Healthy");
    }

    [HttpPost]
    [Route("[controller]/test-email")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestEmail([FromQuery] string? email = null)
    {
        var testEmail = email ?? "kubahypki7@gmail.com";

        await _emailService.SendEmailAsync(
            testEmail,
            "AscendDev Email Test",
            "<h1>Email Test Successful!</h1><p>Your SMTP configuration is working correctly.</p>",
            true
        );

        return Ok($"Test email sent to {testEmail}. Check your logs for delivery status.");
    }
}