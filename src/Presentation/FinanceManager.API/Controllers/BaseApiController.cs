using FinanceManager.Application.Common.Exceptions;
using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected string CurrentUserId =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException();

    protected IActionResult HandleResult<T>(Application.Common.Models.Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);
        return BadRequest(new { error = result.Error });
    }

    protected IActionResult HandleResult(Application.Common.Models.Result result)
    {
        if (result.IsSuccess)
            return Ok();
        return BadRequest(new { error = result.Error });
    }
}
