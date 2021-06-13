using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AppOwnsData.Controllers
{

    public class ContextMenuController : Controller 
    {
        [HttpPost]
        public ActionResult Index()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEndAsync().Result;

                // Do something
            }

            return new ObjectResult(value: new { foo = "bar" });
        }
    }
}
