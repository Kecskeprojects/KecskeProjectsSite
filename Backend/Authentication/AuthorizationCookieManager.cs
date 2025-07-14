using Backend.Communication.Internal;
using Backend.Database.Models;
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

    public async Task SignIn(HttpContext httpContext, string? email, string? password)
    {
        DatabaseActionResult<Account?> account =
            await accountService.FirstOrDefaultAsync(
                a => a.Email == email,
                a => a.Roles);

        if( account.Data is null || password is null)
        {
            throw new DataException("Email or Password incorrect");
        }

        PasswordHasher<Account> hasher = new();
        string hashedPass = Encoding.UTF8.GetString(account.Data.Password);
        if (hasher.VerifyHashedPassword(account.Data, hashedPass, password) == PasswordVerificationResult.Failed)
        {
            throw new DataException("Email or Password incorrect");
        }

        ClaimsIdentity identity = new(GetUserClaims(account.Data), CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
    }

    private static List<Claim> GetUserClaims(Account account)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString(), ClaimValueTypes.String),
            new Claim(ClaimTypes.Name, account.UserName, ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, account.Email, ClaimValueTypes.String),
        ];

        foreach (Role role in account.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String));
        }

        return claims;
    }
}
