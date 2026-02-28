namespace Backend.Constants;

public static class FirewallConstants
{
    public const string FWPolicyProgID = "HNetCfg.FwPolicy2";
    public const string FWRuleProgID = "HNetCfg.FWRule";

#if DEBUG
    public const string ruleNameTCP = "[DEBUG] RDP Whitelist TCP";
    public const string ruleNameUDP = "[DEBUG] RDP Whitelist UDP";
#else
    public const string ruleNameTCP = "RDP Whitelist TCP";
    public const string ruleNameUDP = "RDP Whitelist UDP";
#endif

    public const string DefaultRemoteAddress = "127.0.0.1";
    public const string RDPPort = "3389";
    public const string ApplicationName = "%SystemRoot%\\system32\\svchost.exe";
    public const string RuleDescription = "Allows RDP access from specified IP addresses. Created and modified by KecskeProjects Backend.";
}
