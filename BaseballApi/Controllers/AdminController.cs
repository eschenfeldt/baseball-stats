using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseballApi;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdminController : ControllerBase
{
    [HttpGet("healthcheck")]
    public bool HealthCheck()
    {
        return true;
    }

    [HttpPost("logout")]
    public async Task<IResult> Logout(SignInManager<IdentityUser> signInManager, object empty)
    {
        if (empty != null)
        {
            await signInManager.SignOutAsync();
            return Results.Ok();
        }
        return Results.Unauthorized();
    }
}
