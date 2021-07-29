// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace AppOwnsData.Models
{
    public class PowerBI
    {

        public List<Report> Reports { get; set; }
        

        public class Report {

            public int UniqueId { get; set; }

            public int DisplayLevel { get; set; }
            
            public string DisplayName { get; set; }
            // Workspace Id for which Embed token needs to be generated
            public string WorkspaceId { get; set; }

            public string Description { get; set; }

            // Report Id for which Embed token needs to be generated
            public string ReportId { get; set; }

            public List<int> DrillThroughReports { get; set; }

            public List<string> RequiredParameters { get; set; }

            public string PaginationTable { get; set; }

            public string PaginationColumn { get; set; }
        }
    }
}
