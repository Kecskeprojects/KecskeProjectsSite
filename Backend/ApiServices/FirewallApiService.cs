using Backend.Constants;
using Backend.Tools;
using DatabaseORM.Communication;
using DatabaseORM.Enums;
using DatabaseORM.Service;
using System.Runtime.Versioning;

namespace Backend.ApiServices;

[SupportedOSPlatform("windows")]
public class FirewallApiService(
    PermittedIpAddressService permittedIpAddressService,
    ILogger<FileStorageService> logger,
    IConfiguration configuration)
{
    //Todo: Cleanup for better readability and maintainability (e.g. split into multiple methods, add more logging, add more error handling, etc.)
    public async Task<bool> AddRDPRule(int accountId, string ipAddress)
    {
        //ip format check
        if (!StringTools.ValidateIPv4(ipAddress))
        {
            logger.LogError($"Invalid IP address format: {ipAddress}");
            return false;
        }

        int expirationMinutes = configuration.GetValue<int>(ConfigurationKeys.RDPAccessExpirationMinutesKey);
        DatabaseActionResult<int> dbResult = await permittedIpAddressService.AddAsync(expirationMinutes, accountId, ipAddress);

        if (dbResult.Status != DatabaseActionResultEnum.Success)
        {
            logger.LogError($"Failed to add IP address {ipAddress} to the database for account {accountId}.");
            return false;
        }

        return FirewallTools.ProcessFirewallRuleChange(
            logger,
            (firewallRule) => FirewallTools.AddIpAddress(logger, firewallRule, ipAddress));
    }

    public async Task<bool> RemoveExpiredRDPRules()
    {
        DatabaseActionResult<List<string>?> expiredIPs = await permittedIpAddressService.GetExpiredIPAddressesAsync();
        if (CollectionTools.IsNullOrEmpty(expiredIPs.Data))
        {
            return true;
        }

        bool modificationResult =
            FirewallTools.ProcessFirewallRuleChange(
                logger,
                (firewallRule) => FirewallTools.RemoveExpiredIpAddresses(logger, firewallRule, expiredIPs.Data));

        if (!modificationResult)
        {
            logger.LogError("Failed to remove expired IP addresses from the firewall rules.");
            return false;
        }

        DatabaseActionResult<int> result = await permittedIpAddressService.RemoveIPAddressesAsync(expiredIPs.Data);
        return result.Status == DatabaseActionResultEnum.Success;
    }
}
