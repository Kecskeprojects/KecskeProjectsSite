using Backend.Authentication;
using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.CustomAttributes;
using Backend.Database.Service;
using Backend.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController(
    AccountService accountService,
    UserManager userManager
    ) : ControllerBase
{
    private readonly UserManager userManager = userManager;

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterData form)
    {
        if (!ModelState.IsValid)
        {
            return Problem();
        }

        DatabaseActionResult<int> result = await accountService.RegisterAsync(form);

        if (result.Status != DatabaseActionResultEnum.Success)
        {
            return Problem("Registration unsuccessful!");
        }
        await userManager.SignIn(HttpContext, form.Email, form.Password);
        return Ok();
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginData form)
    {
        if (!ModelState.IsValid)
        {
            return Problem();
        }

        await userManager.SignIn(HttpContext, form.Email, form.Password);

        DatabaseActionResult<int> _ = await accountService.UpdateLastLoginAsync(form.Email!);

        return Ok();
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await userManager.SignOut(HttpContext);

        return Ok();
    }
}
