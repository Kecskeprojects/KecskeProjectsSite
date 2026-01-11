using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Backend.Database.Service;
using Backend.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Backend.Authentication;

public class AuthorizationCookieManager(AccountService accountService)
{
    private readonly AccountService accountService = accountService;

    public async Task<LoggedInAccount?> SignIn(HttpContext httpContext, string username, string password)
    {
        username = username.Trim();
        DatabaseActionResult<Account?> result =
            await accountService.FirstOrDefaultAsync(
                a => a.UserName.Equals(username),
                a => a.Roles);

        if (result.Data is null
            || !EncryptionTools.VerifyPassword(result.Data, password, result.Data.Password)
            || !result.Data.IsRegistrationApproved)
        {
            return null;
        }

        ClaimsPrincipal principal = ClaimsPrincipalTools.CreateAccountClaimsPricipal(result.Data);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return ClaimsPrincipalTools.GetLoggedInAccount(principal);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
    }
}
