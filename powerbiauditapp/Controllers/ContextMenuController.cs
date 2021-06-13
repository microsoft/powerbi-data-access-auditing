using AppOwnsData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AppOwnsData.Controllers
{

    public class ContextMenuController : Controller 
    {
        private readonly IOptions<PowerBI> powerBI;


        public ContextMenuController(IOptions<PowerBI> powerBI)
        {
            this.powerBI = powerBI;
        }

        [HttpPost]
        public ActionResult Index()
        {

            JObject retval = JObject.Parse(@"{ url :""""}");
            retval["url"] = "";

            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEndAsync().Result;
                var jbody = JObject.Parse(body);
                
                //GetTheReport
                var r = powerBI.Value.Reports.Find(x => x.ReportId == jbody["report"]["id"].ToString() && x.DisplayLevel == 1);

                var dps = jbody["dataPoints"];

                string id = "";
                PowerBI.Report r2;
                if (r.DrillThroughReports.Count > 0)
                {
                    //GetTheDrillThroughReport
                    r2 = powerBI.Value.Reports.Find(x => x.UniqueId == r.DrillThroughReports[0]);
                    
                    foreach (var dp in dps)
                    {
                        foreach (var i in dp["identity"])
                        {
                            var colname = i["target"]["column"].ToString();
                            if (colname == r2.RequiredParameters[0])
                            {
                                id = i["equals"].ToString();
                            }
                        }
                    }

                    if (id == "")
                    {
                        retval["url"] = "https://" + this.Request.Host + $"/?&r={r2.ReportId}&WorkspaceId={r2.WorkspaceId}";
                    }
                    else
                    {
                        retval["url"] = "https://" + this.Request.Host + $"/?&r={r2.ReportId}&WorkspaceId={r2.WorkspaceId}&Id={id}";
                    }
                }
            }


            return new ObjectResult(retval.ToString());
        }
    }
}
