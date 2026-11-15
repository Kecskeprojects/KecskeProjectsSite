using Backend.Tools;
using DatabaseORM.Communication.Resource;
using DatabaseORM.Model;
using DatabaseORM.Service;
using DatabaseORM.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Backend.Authentication;

public class AuthorizationCookieManager(AccountService accountService)
{
    private readonly AccountService accountService = accountService;

    public async Task<LoggedInAccount?> SignIn(HttpContext httpContext, string username, string password)
    {
        Account? account = await accountService.FindByUserNameAsync(username);

        if (account is null
            || !account.IsRegistrationApproved
            || !PasswordHasherTools.VerifyPassword(account, password, account.Password))
        {
            return null;
        }

        ClaimsPrincipal principal = ClaimsPrincipalTools.CreateAccountClaimsPricipal(account);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return ClaimsPrincipalTools.GetLoggedInAccount(principal);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
    }
}
