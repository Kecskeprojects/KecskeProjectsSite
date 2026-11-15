using Backend.Tools;
using DatabaseORM.Constants;
using DatabaseORM.Service;
using System.Runtime.Versioning;

namespace Backend.ApiServices;

[SupportedOSPlatform("windows")]
public class FirewallApiService(
    PermittedIpAddressService permittedIpAddressService,
    ILogger<FirewallApiService> logger,
    IConfiguration configuration)
{
    public async Task<bool> AddRDPRule(int accountId, string ipAddress)
    {
        //ip format check
        if (!StringTools.ValidateIPv4(ipAddress))
        {
            logger.LogError($"Invalid IP address format: {ipAddress}");
            return false;
        }

        int expirationMinutes = configuration.GetValue<int>(ConfigurationKeys.RDPAccessExpirationMinutesKey);
        await permittedIpAddressService.AddAsync(expirationMinutes, accountId, ipAddress);

        return FirewallTools.ProcessFirewallRuleChange(
            logger,
            configuration,
            (firewallRule) => FirewallTools.AddIpAddress(logger, firewallRule, ipAddress));
    }

    public async Task<bool> RemoveExpiredRDPRules()
    {
        List<string> expiredIPs = await permittedIpAddressService.GetExpiredIPAddressesAsync();

        IEnumerable<string> currentlyConnected = FirewallTools.GetConnectedRdpAddresses();
        expiredIPs = expiredIPs.Where(ip => !currentlyConnected.Contains(ip)).ToList();

        if (CollectionTools.IsNullOrEmpty(expiredIPs))
        {
            return true;
        }

        bool modificationResult =
            FirewallTools.ProcessFirewallRuleChange(
                logger,
                configuration,
                (firewallRule) => FirewallTools.RemoveExpiredIpAddresses(logger, firewallRule, expiredIPs));

        if (!modificationResult)
        {
            logger.LogError("Failed to remove expired IP addresses from the firewall rules.");
            return false;
        }

        await permittedIpAddressService.RemoveIPAddressesAsync(expiredIPs);
        return true;
    }
}
