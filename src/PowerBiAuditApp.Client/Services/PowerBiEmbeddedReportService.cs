﻿using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using PowerBiAuditApp.Client.Extensions;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Client.Services;

public class PowerBiEmbeddedReportService : IPowerBiEmbeddedReportService
{
    private readonly IPowerBiTokenProvider _tokenProvider;
    private readonly IDataProtector _dataProtector;
    private const string UrlPowerBiServiceApiRoot = "https://api.powerbi.com";

    public PowerBiEmbeddedReportService(IPowerBiTokenProvider tokenProvider, IDataProtectionProvider dataProtectionProvider)
    {
        _tokenProvider = tokenProvider;
        _dataProtector = dataProtectionProvider.CreateProtector(Constants.PowerBiTokenPurpose);
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
    public ReportParameters GetReportParameters(ReportDetails report, [Optional] Guid additionalDatasetId, [Optional] string? effectiveUserName)
    {
        var pbiClient = GetPowerBiClient();

        // Get report info
        var pbiReport = pbiClient.Reports.GetReportInGroup(report.WorkspaceId, report.ReportId);

        //  Check if dataset is present for the corresponding report
        //  If isRDLReport is true then it is a RDL Report 
        var isRdlReport = string.IsNullOrEmpty(pbiReport.DatasetId);

        string embedToken;

        // Generate embed token for RDL report if dataset is not present
        if (isRdlReport)
        {
            // Get Embed token for RDL Report
            embedToken = GetEmbedTokenForRdlReport(report);
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
            embedToken = GetEmbedToken(report, datasetIds, effectiveUserName);
        }


        // Capture embed params
        var reportParameters = new ReportParameters {
            ReportId = pbiReport.Id,
            ReportName = pbiReport.Name,
            EmbedUrl = pbiReport.EmbedUrl,
            EmbedToken = embedToken
        };

        return reportParameters;
    }

    /// <summary>
    /// Get Embed token for single report, multiple datasets, and an optional target workspace
    /// </summary>
    /// <returns>Embed token</returns>
    /// <remarks>This function is not supported for RDL Report</remarks>
    private string GetEmbedToken(ReportDetails report, IList<Guid> datasetIds, [Optional] string? effectiveUserName)
    {
        var pbiClient = GetPowerBiClient();

        List<EffectiveIdentity>? ids = null;
        if (!string.IsNullOrEmpty(effectiveUserName) && report.Roles.Any())
        {

            ids = new()
            {
                new EffectiveIdentity {
                    Username = effectiveUserName,
                    Roles = report.Roles.ToList(),
                    Datasets = datasetIds.Select(d => d.ToString()).ToArray()
                }
            };
        }



        // Create a request for getting Embed token 
        // This method works only with new Power BI V2 workspace experience
        var tokenRequest = new GenerateTokenRequestV2(

            reports: new List<GenerateTokenRequestV2Report> { new(report.ReportId) },

            datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

            targetWorkspaces: report.WorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace> { new(report.WorkspaceId) } : null,

            identities: ids
        );

        // Generate Embed token
        var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

        return EncryptAndFormatToken(embedToken);
    }

    /// <summary>
    /// Get Embed token for RDL Report
    /// </summary>
    /// <returns>Embed token</returns>
    private string GetEmbedTokenForRdlReport(ReportDetails report, string accessLevel = "view")
    {
        var pbiClient = GetPowerBiClient();

        // Generate token request for RDL Report
        var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel);

        // Generate Embed token
        var embedToken = pbiClient.Reports.GenerateTokenInGroup(report.WorkspaceId, report.ReportId, generateTokenRequestParameters);

        return EncryptAndFormatToken(embedToken);
    }

    /// <summary>
    /// Encrypt the token so it's not usable by the end user to run a report;
    /// Update the cluster url so the users browser isn't redirected to the wrong place
    /// </summary>
    /// <param name="embedToken"></param>
    /// <returns></returns>
    private string EncryptAndFormatToken(EmbedToken embedToken)
    {
        //return embedToken.Token;
        var tokenParts = embedToken.Token.Split(".");

        var unprotectedBytes = Encoding.UTF8.GetBytes(tokenParts[0]);
        var protectedBytes = _dataProtector.Protect(unprotectedBytes);
        tokenParts[0] = Convert.ToBase64String(protectedBytes);

        if (tokenParts.Length > 1)
        {

            var additionalData = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1])).ReplaceUrls();
            var additionalBytes = Encoding.UTF8.GetBytes(additionalData);
            tokenParts[1] = Convert.ToBase64String(additionalBytes);
        }

        return string.Join(".", tokenParts);
    }
}