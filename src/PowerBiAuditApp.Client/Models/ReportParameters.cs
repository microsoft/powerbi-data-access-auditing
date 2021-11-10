// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace PowerBiAuditApp.Client.Models;

public class ReportParameters
{

    // Id of Power BI report to be embedded
    public Guid ReportId { get; set; }

    // Name of the report
    public string ReportName { get; init; } = null!;

    // Embed URL for the Power BI report
    public string EmbedUrl { get; init; } = null!;

    // Embed Token for the Power BI report
    public string EmbedToken { get; init; } = null!;
    //List of pages in the report
    public PageParameter[] Pages { get; init; } = null!;
}

public class PageParameter
{
    public string DisplayName { get; init; } = null!;
    public string Name { get; init; } = null!;
}