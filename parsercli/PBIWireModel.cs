using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIParsingConsole
{
    public class DataDescription
    {
        public int Index { get { return int.Parse(GroupCalc.Substring(1)); } }
        public string Name { get; set; }
        public string GroupCalc { get; set; }
    }

    public class PBIWireModel
    {
        public IDictionary<string, DataDescription> DataDescriptionDict = new Dictionary<string, DataDescription>();
        public IDictionary<string, IList<string>> ValueDicts = new Dictionary<string, IList<string>>();
    }
}
