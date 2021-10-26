using System;

namespace PBIParsingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string fp = args[0];
            var parser = new PBIWireParser();

            var model = parser.BuildModel(fp);

            parser.ParseTable(fp, model);
        }
    }
}
