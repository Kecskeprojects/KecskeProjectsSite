using Backend.Constants;
using NetFwTypeLib;
using System.Runtime.Versioning;

namespace Backend.Tools;

[SupportedOSPlatform("windows")]
public static class FirewallTools
{
    public static bool RemoveExpiredIpAddresses(ILogger logger, INetFwRule firewallRule, List<string> expiredIps)
    {
        IEnumerable<string> ipAddresses = [.. firewallRule.RemoteAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries)];

        foreach (string ip in expiredIps)
        {
            ipAddresses = ipAddresses.Where(x => !x.Contains(ip));
        }

        firewallRule.RemoteAddresses = ipAddresses.Any() ? string.Join(',', ipAddresses) : FirewallConstants.DefaultRemoteAddress;

        logger.LogInformation($"Removed the following IP addresses from the whitelist: {string.Join(", ", expiredIps)}");
        return true;
    }

    public static bool AddIpAddress(ILogger logger, INetFwRule firewallRule, string ipAddress)
    {
        if (firewallRule.RemoteAddresses.Contains(ipAddress))
        {
            return true;
        }

        firewallRule.RemoteAddresses = $"{firewallRule.RemoteAddresses.Replace("*", "")},{ipAddress}";

        logger.LogInformation($"Added the following IP address to the whitelist: {ipAddress}");
        return true;
    }

    public static bool ProcessFirewallRuleChange(ILogger logger, Func<INetFwRule, bool> changeLogic)
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

        INetFwRule? firewallRuleTCP = GetOrCreateFirewallRule(logger, firewallPolicy, FirewallConstants.ruleNameTCP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
        INetFwRule? firewallRuleUDP = GetOrCreateFirewallRule(logger, firewallPolicy, FirewallConstants.ruleNameUDP, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);

        return firewallRuleTCP is not null && changeLogic(firewallRuleTCP)
            && firewallRuleUDP is not null && changeLogic(firewallRuleUDP);
    }

    private static INetFwRule? GetOrCreateFirewallRule(ILogger logger, INetFwPolicy2 firewallPolicy, string ruleName, NET_FW_IP_PROTOCOL_ protocol)
    {
        INetFwRule? firewallRuleInstance = firewallPolicy.Rules.OfType<INetFwRule>().FirstOrDefault(x => x.Name == ruleName);
        if (firewallRuleInstance == null)
        {
            Type? fwRuleType = Type.GetTypeFromProgID(FirewallConstants.FWRuleProgID);
            if (fwRuleType is null)
            {
                logger.LogError("Failed to get type for firewall rule creation.");
                return null;
            }
            firewallRuleInstance = CreateFirewallRule(logger, fwRuleType, ruleName, protocol);
            if (firewallRuleInstance == null)
            {
                return null;
            }

            firewallPolicy.Rules.Add(firewallRuleInstance);
        }

        return firewallRuleInstance;
    }

    private static INetFwRule? CreateFirewallRule(ILogger logger, Type fwRuleType, string ruleName, NET_FW_IP_PROTOCOL_ protocol)
    {
        INetFwRule? firewallRuleInstance = (INetFwRule?) Activator.CreateInstance(fwRuleType);
        if (firewallRuleInstance is null)
        {
            logger.LogError("Failed to create firewall rule instance.");
            return null;
        }
        firewallRuleInstance.Name = ruleName;
        firewallRuleInstance.Enabled = true;
        firewallRuleInstance.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
        firewallRuleInstance.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
        firewallRuleInstance.RemoteAddresses = FirewallConstants.DefaultRemoteAddress;
        firewallRuleInstance.Protocol = (int) protocol;
        firewallRuleInstance.LocalPorts = FirewallConstants.RDPPort;
        firewallRuleInstance.ApplicationName = FirewallConstants.ApplicationName;
        firewallRuleInstance.Description = FirewallConstants.RuleDescription;
        return firewallRuleInstance;
    }
}
