using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using PowerBiAuditApp.Client.Extensions;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Client.Services
{
    public class PowerBiEmbeddedReportService : IPowerBiEmbeddedReportService
    {
        private readonly IPowerBiTokenProvider _tokenProvider;
        private readonly IDataProtector _dataProtector;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UrlPowerBiServiceApiRoot = "https://api.powerbi.com";

        public PowerBiEmbeddedReportService(IPowerBiTokenProvider tokenProvider, IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor)
        {
            _tokenProvider = tokenProvider;
            _httpContextAccessor = httpContextAccessor;
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
        public async Task<ReportParameters> GetReportParameters(ReportDetail report, [Optional] Guid additionalDatasetId)
        {
            var pbiClient = GetPowerBiClient();

            // Get report info
            var pbiReport = await pbiClient.Reports.GetReportInGroupAsync(report.GroupId, report.ReportId);

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
                embedToken = GetEmbedToken(report, datasetIds);
            }

            var pages = await GetPages(pbiClient, report);


            // Capture embed params
            var reportParameters = new ReportParameters {
                ReportId = pbiReport.Id,
                ReportName = pbiReport.Name,
                EmbedUrl = pbiReport.EmbedUrl.Replace("app.powerbi.com", _httpContextAccessor.HttpContext?.Request.Host.ToString()),
                EmbedToken = embedToken,
                Pages = pages
            };

            return reportParameters;
        }

        /// <summary>
        /// Gets a reports pages
        /// </summary>
        /// <param name="pbiClient"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        private static async Task<PageParameter[]> GetPages(PowerBIClient pbiClient, ReportDetail report)
        {
            var pages = await pbiClient.Reports.GetPagesAsync(report.GroupId, report.ReportId);
            return pages.Value.Select(x => new PageParameter { DisplayName = x.DisplayName, Name = x.Name }).ToArray();
        }

        /// <summary>
        /// Get Embed token for single report, multiple datasets, and an optional target workspace
        /// </summary>
        /// <returns>Embed token</returns>
        /// <remarks>This function is not supported for RDL Report</remarks>
        private string GetEmbedToken(ReportDetail report, IList<Guid> datasetIds)
        {
            var pbiClient = GetPowerBiClient();

            List<EffectiveIdentity> ids = null;
            if (report.EffectiveIdentityRequired)
            {
                var effectiveUserName = string.IsNullOrWhiteSpace(report.EffectiveIdentityOverRide)
                    ? _httpContextAccessor.HttpContext?.User.Identity?.Name
                    : report.EffectiveIdentityOverRide;

                if (report.EffectiveIdentityRolesRequired && !report.Roles.Any())
                    throw new ArgumentException("Roles need to be setup for this report");

                ids = new List<EffectiveIdentity> {
                    new() {
                        Username = effectiveUserName,
                        Roles = report.EffectiveIdentityRolesRequired? report.Roles.ToList() : null,
                        Datasets = datasetIds.Select(d => d.ToString()).ToArray()
                    }
                };
            }



            // Create a request for getting Embed token 
            // This method works only with new Power BI V2 workspace experience
            var tokenRequest = new GenerateTokenRequestV2(

                reports: new List<GenerateTokenRequestV2Report> { new(report.ReportId) },

                datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

                targetWorkspaces: report.GroupId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace> { new(report.GroupId) } : null,

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
        private string GetEmbedTokenForRdlReport(ReportDetail report, string accessLevel = "view")
        {
            var pbiClient = GetPowerBiClient();

            // Generate token request for RDL Report
            var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel);

            // Generate Embed token
            var embedToken = pbiClient.Reports.GenerateTokenInGroup(report.GroupId, report.ReportId, generateTokenRequestParameters);

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
}