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
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
public class AccountController(
    AccountService accountService,
    AuthorizationCookieManager AccountManager,
    ILogger<AccountController> logger
    ) : ApiControllerBase(logger)
{
    private readonly AccountService accountService = accountService;

    [Authorize]
    [HttpGet]
    public IActionResult GetLoggedInAccount()
    {
        LoggedInAccount? account = GetLoggedInAccountFromCookie();
        return account != null
            ? ContentResult(account)
            : ErrorResult(StatusCodes.Status401Unauthorized, "You are not logged in!");
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterData form)
    {
        DatabaseActionResult<string?> result = await accountService.RegisterAsync(form);

        return result.Status switch
        {
            DatabaseActionResultEnum.Success => ContentResult(result.Data),
            DatabaseActionResultEnum.AlreadyExists => ErrorResult(StatusCodes.Status409Conflict, "Username already exists."),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while registering.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginData form)
    {
        LoggedInAccount? account = await AccountManager.SignIn(HttpContext, form.UserName, form.Password);
        if (account != null)
        {
            _ = await accountService.UpdateLastLoginAsync(account.AccountId);

            Logger.LogInformation($"Account {account.UserName} logged in successfully.");
            return ContentResult(account);
        }

        return ErrorResult(StatusCodes.Status401Unauthorized, "Username or Password incorrect!");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        LoggedInAccount? user = GetLoggedInAccountFromCookie();

        Logger.LogInformation($"Acount {user?.UserName} is logging out.");
        await AccountManager.SignOut(HttpContext);

        return MessageResult("You have been logged out!");
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordData form)
    {
        DatabaseActionResult<int> result = await accountService.ResetPasswordAsync(form);

        return result.Status switch
        {
            DatabaseActionResultEnum.Success => MessageResult("Your password has been reset!"),
            DatabaseActionResultEnum.NotFound => ErrorResult(StatusCodes.Status428PreconditionRequired, "Account with that name doesn't exist."),
            DatabaseActionResultEnum.DifferingHash => ErrorResult(StatusCodes.Status401Unauthorized, "Key is incorrect."),
            _ => ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while resetting password.")
        };
    }

    //Todo: Add endpoint generate new secret key by user ID (admin only), and a listing to get a list of users for admin purposes
}
