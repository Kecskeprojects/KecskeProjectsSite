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
        return account != null
            ? Ok(account)
            : ErrorResult(StatusCodes.Status401Unauthorized, "You are not logged in!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterData form)
    {
        DatabaseActionResult<int> result = await accountService.RegisterAsync(form);

        return result.Status switch
        {
            DatabaseActionResultEnum.Success => MessageResult("Registration successful, once an admin approves your user, you can log in."),
            DatabaseActionResultEnum.AlreadyExists => ErrorResult(StatusCodes.Status409Conflict, "Username already exists."),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while registering.")
        };
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginData form)
    {
        LoggedInAccount? account = await userManager.SignIn(HttpContext, form.UserName, form.Password);
        if (account != null)
        {
            await accountService.UpdateLastLoginAsync(account.AccountId);

            return Ok(account);
        }

        return ErrorResult(StatusCodes.Status401Unauthorized, "Username or Password incorrect!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await userManager.SignOut(HttpContext);

        return MessageResult("You have been logged out!");
    }
}
