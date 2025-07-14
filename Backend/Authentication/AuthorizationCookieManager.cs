using Backend.Communication.Internal;
using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Backend.Database.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;
using System.Text;

namespace Backend.Authentication;

public class AuthorizationCookieManager(AccountService accountService)
{
    private readonly AccountService accountService = accountService;

    public async Task<LoggedInAccount?> SignIn(HttpContext httpContext, string? username, string? password)
    {
        username = username?.Trim();
        DatabaseActionResult<Account?> account =
            await accountService.FirstOrDefaultAsync(
                a => a.UserName.Equals(username),
                a => a.Roles);

        if (account.Data is null || string.IsNullOrWhiteSpace(password))
        {
            throw new DataException("Username or Password incorrect");
        }

        PasswordHasher<Account> hasher = new();
        string hashedPass = Encoding.UTF8.GetString(account.Data.Password);
        if (hasher.VerifyHashedPassword(account.Data, hashedPass, password) == PasswordVerificationResult.Failed)
        {
            throw new DataException("Username or Password incorrect");
        }

        ClaimsPrincipal principal = CreateAccountClaimsPricipal(account.Data);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return GetLoggedInUser(principal);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
    }

    private static ClaimsPrincipal CreateAccountClaimsPricipal(Account account)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString(), ClaimValueTypes.String),
            new Claim(ClaimTypes.Name, account.UserName, ClaimValueTypes.String),
        ];

        foreach (Role role in account.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String));
        }

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        return principal;
    }

    public LoggedInAccount? GetLoggedInUser(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return null;
        }

        int accountId = int.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;
        return new LoggedInAccount
            {
                AccountId = accountId,
                UserName = identity.FindFirst(ClaimTypes.Name)?.Value,
                Roles = [ ..identity.FindAll(ClaimTypes.Role).Select(x => x?.Value)]
            };
    }
}
