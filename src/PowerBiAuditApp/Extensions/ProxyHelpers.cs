using System.Text.RegularExpressions;

namespace PowerBiAuditApp.Extensions;

public static class ProxyHelpers
{
    public static string ReplaceUrls(this string stringContent)
    {
        return stringContent
            // ReSharper disable StringLiteralTypo
            .RegexReplace(@"https\:\/\/([^/]*)\.powerbi\.com", "/power-bi/$1", RegexOptions.IgnoreCase)
            .RegexReplace(@"https\:\/\/([^/]*)\.analysis\.windows\.net", "/analysis-windows/$1", RegexOptions.IgnoreCase)
            .RegexReplace(@"https\:\/\/([^/]*)\.powerapps\.com", "/power-apps/$1", RegexOptions.IgnoreCase);
        // ReSharper restore StringLiteralTypo
    }

    public static string ReformUrls(this string stringContent)
    {

        return stringContent
            // ReSharper disable StringLiteralTypo
            .RegexReplace(@"""\/power-bi\/([^/""]*)", @"""https://$1.powerbi.com", RegexOptions.IgnoreCase)
            .RegexReplace(@"""\/analysis-windows\/([^/""]*)", @"""https://$1.analysis.windows.net", RegexOptions.IgnoreCase)
            .RegexReplace(@"""\/power-apps\/([^/""]*)", @"""https://$1.powerapps.com", RegexOptions.IgnoreCase);
        // ReSharper restore StringLiteralTypo
    }
}