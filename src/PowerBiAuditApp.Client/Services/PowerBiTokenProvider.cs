// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

using System.Security;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PowerBiAuditApp.Client.Models;

namespace PowerBiAuditApp.Client.Services;

public class PowerBiTokenProvider : IPowerBiTokenProvider
{
    private readonly IOptions<ServicePrincipal> _servicePrincipal;

    public PowerBiTokenProvider(IOptions<ServicePrincipal> servicePrincipal)
    {
        _servicePrincipal = servicePrincipal;
    }

    /// <summary>
    /// Generates and returns Access token
    /// </summary>
    /// <returns>AAD token</returns>
    public string? GetAccessToken()
    {
        AuthenticationResult? authenticationResult = null;
        switch (_servicePrincipal.Value?.AuthenticationMode)
        {
            case ServicePrincipal.ServicePrincipalAuthenticationMode.MasterUser:
                {
                    // Create a public client to authorize the app with the AAD app
                    var clientApp = PublicClientApplicationBuilder.Create(_servicePrincipal.Value.ClientId).WithAuthority(_servicePrincipal.Value.AuthorityUri).Build();
                    var userAccounts = clientApp.GetAccountsAsync().Result;
                    try
                    {
                        // Retrieve Access token from cache if available
                        authenticationResult = clientApp.AcquireTokenSilent(_servicePrincipal.Value.Scope, userAccounts.FirstOrDefault()).ExecuteAsync().Result;
                    }
                    catch (MsalUiRequiredException)
                    {
                        var password = new SecureString();
                        foreach (var key in _servicePrincipal.Value.PbiPassword ?? string.Empty)
                        {
                            password.AppendChar(key);
                        }
                        authenticationResult = clientApp.AcquireTokenByUsernamePassword(_servicePrincipal.Value.Scope, _servicePrincipal.Value.PbiUsername, password).ExecuteAsync().Result;
                    }

                    break;
                }
            // Service Principal auth is the recommended by Microsoft to achieve App Owns Data Power BI embedding
            case ServicePrincipal.ServicePrincipalAuthenticationMode.ServicePrincipal:
                {
                    // For app only authentication, we need the specific tenant id in the authority url
                    var tenantSpecificUrl = _servicePrincipal.Value.AuthorityUri?.Replace("organizations", _servicePrincipal.Value.TenantId);

                    // Create a confidential client to authorize the app with the AAD app
                    var clientApp = ConfidentialClientApplicationBuilder
                        .Create(_servicePrincipal.Value.ClientId)
                        .WithClientSecret(_servicePrincipal.Value.ClientSecret)
                        .WithAuthority(tenantSpecificUrl)
                        .Build();
                    // Make a client call if Access token is not available in cache
                    authenticationResult = clientApp.AcquireTokenForClient(_servicePrincipal.Value.Scope).ExecuteAsync().Result;
                    break;
                }
        }

        return authenticationResult?.AccessToken;
    }
}