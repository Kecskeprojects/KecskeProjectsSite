using Backend.Communication.Outgoing;
using Backend.Database.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Backend.Tools;

public class ClaimsPrincipalTools
{
    public static ClaimsPrincipal CreateAccountClaimsPricipal(Account account)
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

    public static LoggedInAccount? GetLoggedInAccount(ClaimsPrincipal principal)
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
            Roles = [.. identity.FindAll(ClaimTypes.Role).Select(x => x?.Value)]
        };
    }
}
