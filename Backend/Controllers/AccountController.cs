using Backend.Authentication;
using Backend.Communication.Incoming;
using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Database.Service;
using Backend.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController(
    AccountService accountService,
    AuthorizationCookieManager userManager,
    ILogger<AccountController> logger
    ) : ApiControllerBase(logger)
{
    private readonly AccountService accountService = accountService;

    [Authorize]
    [ErrorLoggingFilter]
    [HttpGet]
    public IActionResult GetLoggedInUser()
    {
        LoggedInAccount? account = GetLoggedInUserFromCookie();
        return account != null
            ? Ok(account)
            : ErrorResult(StatusCodes.Status401Unauthorized, "You are not logged in!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterData form)
    {
        DatabaseActionResult<string?> result = await accountService.RegisterAsync(form);

        return result.Status switch
        {
            DatabaseActionResultEnum.Success => Ok(result.Data),
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

            Logger.LogInformation("User {UserName} logged in successfully.", account.UserName);
            return Ok(account);
        }

        return ErrorResult(StatusCodes.Status401Unauthorized, "Username or Password incorrect!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        LoggedInAccount? user = GetLoggedInUserFromCookie();

        Logger.LogInformation("User {UserName} is logging out.", user?.UserName);
        await userManager.SignOut(HttpContext);

        return MessageResult("You have been logged out!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordData form)
    {
        DatabaseActionResult<int> result = await accountService.ResetPasswordAsync(form);

        return result.Status switch
        {
            DatabaseActionResultEnum.Success => Ok("Your password has been reset!"),
            DatabaseActionResultEnum.NotFound => ErrorResult(StatusCodes.Status428PreconditionRequired, "User with that name doesn't exist."),
            DatabaseActionResultEnum.DifferingHash => ErrorResult(StatusCodes.Status401Unauthorized, "Key is incorrect."),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while resetting password.")
        };
    }

    //Todo: Add endpoint generate new secret key by user ID (admin only), and a listing to get a list of users for admin purposes
}
