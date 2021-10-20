using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using PowerBiAuditApp.Models;
using System.Runtime.InteropServices;

namespace PowerBiAuditApp.Services;

public class PowerBiReportService : IPowerBiReportService
{
    private readonly IPowerBiTokenProvider _tokenProvider;
    private const string UrlPowerBiServiceApiRoot = "https://api.powerbi.com";

    public PowerBiReportService(IPowerBiTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Get Power BI client
    /// </summary>
    /// <returns>Power BI client object</returns>
    private PowerBIClient GetPowerBiClient()
    {
        var tokenCredentials = new TokenCredentials(_tokenProvider.GetAccessToken(), "Bearer");
        return new PowerBIClient(new Uri(UrlPowerBiServiceApiRoot), tokenCredentials);
    }

    /// <summary>
    /// Get embed params for a report
    /// </summary>
    /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
    public ReportParameters GetReportParameters(ReportDetails report, [Optional] Guid additionalDatasetId, [Optional] string effectiveUserName, [Optional] string effectiveUserRole)
    {
        var pbiClient = GetPowerBiClient();

        // Get report info
        var pbiReport = pbiClient.Reports.GetReportInGroup(report.WorkspaceId, report.ReportId);

        //  Check if dataset is present for the corresponding report
        //  If isRDLReport is true then it is a RDL Report 
        var isRdlReport = string.IsNullOrEmpty(pbiReport.DatasetId);

        EmbedToken embedToken;

        // Generate embed token for RDL report if dataset is not present
        if (isRdlReport)
        {
            // Get Embed token for RDL Report
            embedToken = GetEmbedTokenForRdlReport(report.WorkspaceId, report.ReportId);
        }
        else
        {
            // Create list of datasets
            var datasetIds = new List<Guid> {
                // Add dataset associated to the report
                Guid.Parse(pbiReport.DatasetId)
            };

            // Append additional dataset to the list to achieve dynamic binding later
            if (additionalDatasetId != Guid.Empty)
            {
                datasetIds.Add(additionalDatasetId);
            }

            // Get Embed token multiple resources
            embedToken = GetEmbedToken(report.ReportId, datasetIds, report.WorkspaceId, effectiveUserName);
        }


        // Capture embed params
        var reportParameters = new ReportParameters
        {
            ReportId = pbiReport.Id,
            ReportName = pbiReport.Name,
            EmbedUrl = pbiReport.EmbedUrl,
            EmbedToken = embedToken
        };

        return reportParameters;
    }

    ///// <summary>
    ///// Get embed params for multiple reports for a single workspace
    ///// </summary>
    ///// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for multiple reports</returns>
    ///// <remarks>This function is not supported for RDL Report</remarks>
    //public List<ReportParameters> GetReportParameters(Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds)
    //{
    //    // Note: This method is an example and is not consumed in this sample app

    //    var pbiClient = GetPowerBiClient();

    //    // Create mapping for reports and Embed URLs
    //    var reportParameters = new List<ReportParameters>();

    //    // Create list of datasets
    //    var datasetIds = new List<Guid>();

    //    // Get Embed token multiple resources
    //    var embedToken = GetEmbedToken(reportIds, datasetIds, workspaceId);

    //    // Get datasets and Embed URLs for all the reports
    //    foreach (var reportId in reportIds)
    //    {
    //        // Get report info
    //        var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

    //        datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

    //        // Add report data for embedding
    //        reportParameters.Add(new ReportParameters
    //        {
    //            ReportId = pbiReport.Id,
    //            ReportName = pbiReport.Name,
    //            EmbedUrl = pbiReport.EmbedUrl,
    //            EmbedToken = embedToken
    //        });
    //    }

    //    // Append to existing list of datasets to achieve dynamic binding later
    //    datasetIds.AddRange(additionalDatasetIds);

    //    return reportParameters;
    //}


    /// <summary>
    /// Get Embed token for single report, multiple datasets, and an optional target workspace
    /// </summary>
    /// <returns>Embed token</returns>
    /// <remarks>This function is not supported for RDL Report</remarks>
    private EmbedToken GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId, [Optional] string effectiveUserName)
    {
        var pbiClient = GetPowerBiClient();

        var roles = new List<string>();
        roles.Add("testrole");

        var identity = new EffectiveIdentity
        {
            Username = effectiveUserName,
            Roles = roles,
            Datasets = datasetIds.Select(d => d.ToString()).ToArray()
        };
        var ids = new List<EffectiveIdentity>();

        if (!string.IsNullOrEmpty(identity.Username))
        {
            ids.Add(identity);
        }



        // Create a request for getting Embed token 
        // This method works only with new Power BI V2 workspace experience
        var tokenRequest = new GenerateTokenRequestV2(

            reports: new List<GenerateTokenRequestV2Report> { new(reportId) },

            datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

            targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace> { new(targetWorkspaceId) } : null,

            identities: ids.Count > 0 ? ids : null
        );

        // Generate Embed token
        var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

        return embedToken;
    }

    ///// <summary>
    ///// Get Embed token for multiple reports, datasets, and an optional target workspace
    ///// </summary>
    ///// <returns>Embed token</returns>
    ///// <remarks>This function is not supported for RDL Report</remarks>
    //private EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
    //{
    //    // Note: This method is an example and is not consumed in this sample app

    //    var pbiClient = GetPowerBiClient();

    //    // Convert report Ids to required types
    //    var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

    //    // Convert dataset Ids to required types
    //    var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

    //    var targetWorkspaces = targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace> { new(targetWorkspaceId) } : null;

    //    // Create a request for getting Embed token 
    //    // This method works only with new Power BI V2 workspace experience
    //    var tokenRequest = new GenerateTokenRequestV2(datasets, reports, targetWorkspaces);

    //    // Generate Embed token
    //    var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

    //    return embedToken;
    //}

    ///// <summary>
    ///// Get Embed token for multiple reports, datasets, and optional target work spaces
    ///// </summary>
    ///// <returns>Embed token</returns>
    ///// <remarks>This function is not supported for RDL Report</remarks>
    //private EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] IList<Guid> targetWorkspaceIds)
    //{
    //    // Note: This method is an example and is not consumed in this sample app

    //    var pbiClient = GetPowerBiClient();

    //    // Convert report Ids to required types
    //    var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

    //    // Convert dataset Ids to required types
    //    var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

    //    // Convert target workspace Ids to required types
    //    var targetWorkspaces = targetWorkspaceIds.Select(targetWorkspaceId => new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId)).ToList();

    //    // Create a request for getting Embed token 
    //    // This method works only with new Power BI V2 workspace experience
    //    var tokenRequest = new GenerateTokenRequestV2(datasets, reports, targetWorkspaces.Any() ? targetWorkspaces : null);

    //    // Generate Embed token
    //    var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

    //    return embedToken;
    //}

    /// <summary>
    /// Get Embed token for RDL Report
    /// </summary>
    /// <returns>Embed token</returns>
    private EmbedToken GetEmbedTokenForRdlReport(Guid targetWorkspaceId, Guid reportId, string accessLevel = "view")
    {
        var pbiClient = GetPowerBiClient();

        // Generate token request for RDL Report
        var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel);

        // Generate Embed token
        var embedToken = pbiClient.Reports.GenerateTokenInGroup(targetWorkspaceId, reportId, generateTokenRequestParameters);

        return embedToken;
    }




    //public void RenderReport()
    //{
    //    var client = new HttpClient();
    //    var msg = new HttpRequestMessage();
    //    msg.Method = HttpMethod.Get;
    //    msg.RequestUri = new Uri("https://app.powerbi.com/view?r=eyJrIjoiYjdkZGFjMGEtOGMzZC00ZjAxLTg3ZGItOTVhMzc5NTVmMGQ2IiwidCI6IjdkYzExMmRlLTZhNTItNDA2OS1hN2Q1LWRjNzYzODMzNGMxYyIsImMiOjl9");
    //    var result = client.GetAsync(new Uri("https://app.powerbi.com/view?r=eyJrIjoiYjdkZGFjMGEtOGMzZC00ZjAxLTg3ZGItOTVhMzc5NTVmMGQ2IiwidCI6IjdkYzExMmRlLTZhNTItNDA2OS1hN2Q1LWRjNzYzODMzNGMxYyIsImMiOjl9")).Result;
    //    var resultString = result.Content.ReadAsStringAsync().Result;

    //    //Use the default configuration for AngleSharp
    //    var config = Configuration.Default;
    //    //Create a new context for evaluating webpages with the given config
    //    var context = BrowsingContext.New(config);
    //    //Create a virtual request to specify the document to load (here from our fixed string)
    //    var document = context.OpenAsync(req => req.Content(resultString)).Result;
    //    foreach (var s in document.QuerySelectorAll("script"))
    //    {
    //        if (s.HasAttribute("src"))
    //        {
    //            var src = s.GetAttribute("src");
    //            if (!src.StartsWith("https://"))
    //            {
    //                s.SetAttribute("src", "https://app.powerbi.com/" + src);
    //            }
    //        }
    //    }
    //}
}