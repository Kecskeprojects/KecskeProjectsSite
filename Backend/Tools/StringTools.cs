namespace Backend.Tools;

public static class StringTools
{
    public static bool ValidateIPv4(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        string[] splitValues = ipString.Split('.');
        return splitValues.Length == 4
            && splitValues.All(r => byte.TryParse(r, out byte _));
    }
}
