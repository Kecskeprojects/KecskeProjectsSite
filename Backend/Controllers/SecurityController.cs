using Backend.Communication.Outgoing;
using Backend.Controllers.Base;
using Backend.CustomAttributes;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;

namespace Backend.Controllers;

[ApiController]
[ErrorLoggingFilter]
[Route("api/[controller]/[action]")]
[SupportedOSPlatform("windows")]
public class SecurityController(
    ILogger<AccountController> logger,
    FirewallApiService firewallApiService
    ) : ApiControllerBase(logger)
{
    private readonly FirewallApiService firewallApiService = firewallApiService;

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AddAddressToRule([FromQuery] string? address)
    {
        address ??= HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(address))
        {
            return ErrorResult(StatusCodes.Status400BadRequest, "IP address is required.");
        }

        LoggedInAccount? loggedInAccount = GetLoggedInAccountFromCookie();

        bool success = await firewallApiService.AddRDPRule(loggedInAccount!.AccountId, address);

        return success
            ? MessageResult("Firewall rule updated successfully.")
            : ErrorResult(StatusCodes.Status500InternalServerError, "Failed to update firewall rule.");
    }
}
