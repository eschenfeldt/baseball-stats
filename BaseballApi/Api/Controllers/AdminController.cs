using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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

    [HttpGet("isAuthorized")]
    [AllowAnonymous]
    public bool IsAuthorized(SignInManager<IdentityUser> signInManager)
    {
        return signInManager.IsSignedIn(this.User);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IResult> Login(SignInManager<IdentityUser> signInManager, LoginRequest login)
    {
        bool isPersistent = true;
        var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: isPersistent, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
            }
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
            {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
            }
        }

        if (!result.Succeeded)
        {
            return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        // The signInManager already produced the needed response in the form of a cookie or bearer token.
        return TypedResults.Empty;
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
