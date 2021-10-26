namespace PowerBiAuditApp.Services;

public interface IPowerBiTokenProvider
{
    /// <summary>
    /// Generates and returns Access token
    /// </summary>
    /// <returns>AAD token</returns>
    string? GetAccessToken();
}