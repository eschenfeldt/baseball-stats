using Microsoft.AspNetCore.Mvc;

namespace BaseballApi;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private AppIdentityDbContext Context { get; }

    public AccountController(AppIdentityDbContext context)
    {
        this.Context = context;
    }

    [HttpPost("CreateRoles")]
    public async Task CreateRoles()
    {
        foreach (Role role in Enum.GetValues(typeof(Role)))
        {
            // bool roleExists = await
        }
    }
}
