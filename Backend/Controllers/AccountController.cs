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
    ) : ApiControllerBase(userManager, logger)
{
    private readonly AccountService accountService = accountService;
    private readonly ILogger<AccountController> logger = logger;

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
        DatabaseActionResult<int> result = await accountService.RegisterAsync(form);

        if (result.Status <= DatabaseActionResultEnum.Success)
        {
            logger.LogError("Registration successful for {UserName}, once an admin approves your user, you can log in.", form.UserName);
        }

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

            logger.LogInformation("User {UserName} logged in successfully.", account.UserName);
            return Ok(account);
        }

        return ErrorResult(StatusCodes.Status401Unauthorized, "Username or Password incorrect!");
    }

    [ErrorLoggingFilter]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        LoggedInAccount? user = GetLoggedInUserFromCookie();

        logger.LogInformation("User {UserName} is logging out.", user?.UserName);
        await userManager.SignOut(HttpContext);

        return MessageResult("You have been logged out!");
    }
}
