using Backend.Authentication;
using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Database.Service;
using Backend.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController(
    AccountService accountService,
    AuthorizationCookieManager userManager
    ) : ApiControllerBase
{
    private readonly AuthorizationCookieManager userManager = userManager;

    [Authorize]
    [ErrorLoggingFilter]
    [HttpGet]
    public IActionResult GetLoggedInUser()
    {
        LoggedInAccount? account = userManager.GetLoggedInUser(HttpContext.User);
        return Ok(account);
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterData form) //Todo: model validation
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

        LoggedInAccount? account = await userManager.SignIn(HttpContext, form.Email, form.Password);
        DatabaseActionResult<int> _ = await accountService.UpdateLastLoginAsync(form.Email!);

        return Ok(account);
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginData form) //Todo: model validation
    {
        if (!ModelState.IsValid)
        {
            return Problem();
        }

        LoggedInAccount? account = await userManager.SignIn(HttpContext, form.Email, form.Password);
        DatabaseActionResult<int> _ = await accountService.UpdateLastLoginAsync(form.Email!);

        return Ok(account);
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await userManager.SignOut(HttpContext);

        return Ok();
    }
}
