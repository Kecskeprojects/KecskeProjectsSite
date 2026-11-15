using Backend.Authentication;
using Backend.Communication.Incoming;
using Backend.Constants;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using DatabaseORM.Communication.Resource;
using DatabaseORM.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
[EnableRateLimiting(RateLimitPolicies.Authentication)]
public class AccountController(
    AccountService accountService,
    AuthorizationCookieManager AccountManager,
    ILogger<AccountController> logger
    ) : ApiControllerBase<AccountService>(logger, accountService)
{
    [Authorize]
    [HttpGet]
    [DisableRateLimiting]
    public IActionResult GetLoggedInAccount()
    {
        return LoggedInAccount != null
            ? ContentResult(LoggedInAccount)
            : ErrorResult(StatusCodes.Status401Unauthorized, "You are not logged in!");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    [DisableRateLimiting]
    public async Task<IActionResult> GetUsers()
    {
        List<AccountResource> users = await service.GetUsersAsync();
        return ContentResult(users);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterData form)
    {
        string secretKey = await service.RegisterAsync(form.UserName, form.Password);
        return ContentResult(secretKey);
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginData form)
    {
        LoggedInAccount? account = await AccountManager.SignIn(HttpContext, form.UserName, form.Password);
        if (account != null)
        {
            await service.UpdateLastLoginAsync(account.AccountId);

            Logger.LogInformation($"Account {account.UserName} logged in successfully.");
            return ContentResult(account);
        }

        return ErrorResult(StatusCodes.Status401Unauthorized, "Username or Password incorrect!");
    }

    [HttpPost]
    [DisableRateLimiting]
    public async Task<IActionResult> Logout()
    {
        Logger.LogInformation($"Acount {LoggedInAccount?.UserName} is logging out.");
        await AccountManager.SignOut(HttpContext);

        return MessageResult("You have been logged out!");
    }

    [HttpPut]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordData form)
    {
        await service.ResetPasswordAsync(form.UserName, form.SecretKey, form.Password);
        return MessageResult("Your password has been reset!");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{accountId}")]
    [DisableRateLimiting]
    public async Task<IActionResult> GenerateNewSecretKey([FromRoute] int accountId)
    {
        string secretKey = await service.GenerateNewSecretKeyAsync(accountId);
        return ContentResult(secretKey);
    }
}
