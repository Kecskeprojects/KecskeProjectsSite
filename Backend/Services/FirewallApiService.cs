using Backend.Communication.Internal;
using Backend.Database.Service;
using Backend.Enums;
using Backend.Tools;
using NetFwTypeLib;
using System.Runtime.Versioning;

namespace Backend.Services;

[SupportedOSPlatform("windows")]
public class FirewallApiService(PermittedIpAddressService permittedIpAddressService, ILogger<FileStorageService> logger)
{
    const string FWPolicyProgID = "HNetCfg.FwPolicy2";
    const string FWRuleProgID = "HNetCfg.FWRule";

#if DEBUG
    const string ruleNameTCP = "RDP Whitelist TCP Debug";
    const string ruleNameUDP = "RDP Whitelist UDP Debug";
#else
    const string ruleNameTCP = "RDP Whitelist TCP";
    const string ruleNameUDP = "RDP Whitelist UDP";
#endif

    public async Task<bool> AddRDPRule(int accountId, string ip)
    {
        //ip format check
        if (!StringTools.ValidateIPv4(ip))
        {
            logger.LogError($"Invalid IP address format: {ip}");
            return false;
        }

        Type? fwPolicy = Type.GetTypeFromProgID(FWPolicyProgID);
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

        INetFwRule? firewallRuleTCP = GetOrCreateRule(firewallPolicy, ruleNameTCP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        INetFwRule? firewallRuleUDP = GetOrCreateRule(firewallPolicy, ruleNameUDP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);

        if (firewallRuleTCP is null || firewallRuleUDP is null)
        {
            return false;
        }

        if (!firewallRuleTCP.RemoteAddresses.Contains(ip) || !firewallRuleUDP.RemoteAddresses.Contains(ip))
        {
            DatabaseActionResult<int> dbResult = await permittedIpAddressService.AddAsync(accountId, ip);

            if(dbResult.Status != DatabaseActionResultEnum.Success)
            {
                logger.LogError($"Failed to add IP address {ip} to the database for account {accountId}.");
                return false;
            }

            firewallRuleTCP.RemoteAddresses = $"{firewallRuleTCP.RemoteAddresses.Replace("*", "")},{ip}";
            firewallRuleUDP.RemoteAddresses = $"{firewallRuleUDP.RemoteAddresses.Replace("*", "")},{ip}";

            logger.LogInformation($"Added the following IP address to the whitelist: {ip}");
        }

        return true;
    }

    public async Task<bool> RemoveExpiredRDPRules()
    {
        Type? fwPolicy = Type.GetTypeFromProgID(FWPolicyProgID);
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

        INetFwRule? firewallRuleTCP = GetOrCreateRule(firewallPolicy, ruleNameTCP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        INetFwRule? firewallRuleUDP = GetOrCreateRule(firewallPolicy, ruleNameUDP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);

        if (firewallRuleTCP is null || firewallRuleUDP is null)
        {
            return false;
        }

        DatabaseActionResult<List<string>?> expiredIPs = await permittedIpAddressService.GetExpiredIPAddressesAsync();

        if(CollectionTools.IsNullOrEmpty(expiredIPs.Data))
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

        firewallRuleTCP.RemoteAddresses = tcpAddresses.Any() ? string.Join(',', tcpAddresses) : "127.0.0.1";
        firewallRuleUDP.RemoteAddresses = udpAddresses.Any() ? string.Join(',', udpAddresses) : "127.0.0.1";

        DatabaseActionResult<int> result = await permittedIpAddressService.RemoveIPAddressesAsync(expiredIPs.Data);

        logger.LogInformation($"Removed the following IP addresses from the whitelist: {string.Join(", ", expiredIPs.Data)}");

        return result.Status == DatabaseActionResultEnum.Success;
    }

    private INetFwRule? GetOrCreateRule(INetFwPolicy2 firewallPolicy, string ruleName, NET_FW_IP_PROTOCOL_ protocol)
    {
        INetFwRule? firewallRule = firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == ruleName).FirstOrDefault();
        if (firewallRule == null)
        {
            Type? fwRule = Type.GetTypeFromProgID(FWRuleProgID);
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
            firewallRule.RemoteAddresses = "127.0.0.1";
            firewallRule.Protocol = (int) protocol;
            firewallRule.LocalPorts = "3389";
            firewallRule.ApplicationName = "%SystemRoot%\\system32\\svchost.exe";
            firewallRule.Description = "Allows RDP access from specified IP addresses. Created and modified by KecskeProjects Backend.";
            firewallPolicy.Rules.Add(firewallRule);
        }

        return firewallRule;
    }
}
