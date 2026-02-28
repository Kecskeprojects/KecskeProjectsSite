using Backend.Communication.Internal;
using Backend.Constants;
using Backend.Database.Service;
using Backend.Enums;
using Backend.Tools;
using NetFwTypeLib;
using System.Runtime.Versioning;

namespace Backend.Services;

[SupportedOSPlatform("windows")]
public class FirewallApiService(PermittedIpAddressService permittedIpAddressService, ILogger<FileStorageService> logger)
{
    public async Task<bool> AddRDPRule(int accountId, string ip)
    {
        //ip format check
        if (!StringTools.ValidateIPv4(ip))
        {
            logger.LogError($"Invalid IP address format: {ip}");
            return false;
        }

        return await ProcessFirewallRuleChange(async (firewallRuleTCP, firewallRuleUDP) =>
        {
            if (firewallRuleTCP.RemoteAddresses.Contains(ip) && firewallRuleUDP.RemoteAddresses.Contains(ip))
            {
                return true;
            }

            DatabaseActionResult<int> dbResult = await permittedIpAddressService.AddAsync(accountId, ip);

            if (dbResult.Status != DatabaseActionResultEnum.Success)
            {
                logger.LogError($"Failed to add IP address {ip} to the database for account {accountId}.");
                return false;
            }

            firewallRuleTCP.RemoteAddresses = $"{firewallRuleTCP.RemoteAddresses.Replace("*", "")},{ip}";
            firewallRuleUDP.RemoteAddresses = $"{firewallRuleUDP.RemoteAddresses.Replace("*", "")},{ip}";

            logger.LogInformation($"Added the following IP address to the whitelist: {ip}");
            return true;
        });
    }

    public async Task<bool> RemoveExpiredRDPRules()
    {
        return await ProcessFirewallRuleChange(async (firewallRuleTCP, firewallRuleUDP) =>
        {
            DatabaseActionResult<List<string>?> expiredIPs = await permittedIpAddressService.GetExpiredIPAddressesAsync();

            if (CollectionTools.IsNullOrEmpty(expiredIPs.Data))
            {
                return true;
            }

            IEnumerable<string> tcpAddresses = firewallRuleTCP.RemoteAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            IEnumerable<string> udpAddresses = firewallRuleUDP.RemoteAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string ip in expiredIPs.Data!)
            {
                tcpAddresses = tcpAddresses.Where(x => !x.Contains(ip));
                udpAddresses = udpAddresses.Where(x => !x.Contains(ip));
            }

            firewallRuleTCP.RemoteAddresses = tcpAddresses.Any() ? string.Join(',', tcpAddresses) : FirewallConstants.DefaultRemoteAddress;
            firewallRuleUDP.RemoteAddresses = udpAddresses.Any() ? string.Join(',', udpAddresses) : FirewallConstants.DefaultRemoteAddress;

            DatabaseActionResult<int> result = await permittedIpAddressService.RemoveIPAddressesAsync(expiredIPs.Data);

            logger.LogInformation($"Removed the following IP addresses from the whitelist: {string.Join(", ", expiredIPs.Data)}");

            return result.Status == DatabaseActionResultEnum.Success;
        });
    }

    private async Task<bool> ProcessFirewallRuleChange(Func<INetFwRule, INetFwRule, Task<bool>> changeLogic)
    {
        Type? fwPolicy = Type.GetTypeFromProgID(FirewallConstants.FWPolicyProgID);
        if (fwPolicy is null)
        {
            logger.LogError("Failed to get type for firewall list.");
            return false;
        }

        INetFwPolicy2? firewallPolicy = (INetFwPolicy2?) Activator.CreateInstance(fwPolicy);

        if (firewallPolicy is null)
        {
            logger.LogError("Failed to create firewall policy instance.");
            return false;
        }

        INetFwRule? firewallRuleTCP = GetOrCreateRule(firewallPolicy, FirewallConstants.ruleNameTCP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        INetFwRule? firewallRuleUDP = GetOrCreateRule(firewallPolicy, FirewallConstants.ruleNameUDP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);

        return firewallRuleTCP is not null
            && firewallRuleUDP is not null
            && await changeLogic(firewallRuleTCP, firewallRuleUDP);
    }

    private INetFwRule? GetOrCreateRule(INetFwPolicy2 firewallPolicy, string ruleName, NET_FW_IP_PROTOCOL_ protocol)
    {
        INetFwRule? firewallRule = firewallPolicy.Rules.OfType<INetFwRule>().FirstOrDefault(x => x.Name == ruleName);
        if (firewallRule == null)
        {
            Type? fwRule = Type.GetTypeFromProgID(FirewallConstants.FWRuleProgID);
            if (fwRule is null)
            {
                logger.LogError("Failed to get type for firewall rule creation.");
                return null;
            }
            firewallRule = (INetFwRule?) Activator.CreateInstance(fwRule);
            if (firewallRule is null)
            {
                logger.LogError("Failed to create firewall rule instance.");
                return null;
            }
            firewallRule.Name = ruleName;
            firewallRule.Enabled = true;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            firewallRule.RemoteAddresses = FirewallConstants.DefaultRemoteAddress;
            firewallRule.Protocol = (int) protocol;
            firewallRule.LocalPorts = FirewallConstants.RDPPort;
            firewallRule.ApplicationName = FirewallConstants.ApplicationName;
            firewallRule.Description = FirewallConstants.RuleDescription;
            firewallPolicy.Rules.Add(firewallRule);
        }

        return firewallRule;
    }
}
