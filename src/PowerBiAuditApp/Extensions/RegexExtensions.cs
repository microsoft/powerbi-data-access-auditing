using System.Text.RegularExpressions;

namespace PowerBiAuditApp.Extensions;

public static class RegexExtensions
{
    public static string RegexReplace(this string str, string pattern, string replacement, RegexOptions? options = null)
    {
        if (options is null)
            return Regex.Replace(str, pattern, replacement);

        return Regex.Replace(str, pattern, replacement, options.Value);
    }
}