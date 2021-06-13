// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace AppOwnsData.Models
{
    public class PBIReport
    {
        // Can be set to 'MasterUser' or 'ServicePrincipal'
        public string AuthenticationMode { get; set; }
    }
}
