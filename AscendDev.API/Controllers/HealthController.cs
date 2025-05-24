using AscendDev.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AscendDev.Functions.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    [Route("[controller]")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorApiResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetHealth()
    {
        return Ok("Healthy");
    }
}