using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PBIParsingConsole
{

    public static partial class JsonExtensions
    {
        public static JsonElement? Get(this JsonElement element, string name) =>
            element.ValueKind != JsonValueKind.Null && element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(name, out var value)
                ? value : (JsonElement?)null;

        public static JsonElement? Get(this JsonElement element, int index)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return null;
            var value = element.EnumerateArray().ElementAtOrDefault(index);
            return value.ValueKind != JsonValueKind.Undefined ? value : (JsonElement?)null;
        }
    }

    public class PBIWireRow
    {
        public IList<string> Cells { get { return new List<string>(Cells).AsReadOnly(); } }
        private string[] cells;
        public PBIWireRow(int length)
        {
            cells = new string[length];
        }
        public void SetCell(int index, string value)
        {
            cells[index] = value;
        }

        public string GetCell(int index)
        {
            return cells[index];
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var s in cells)
            {
                sb.Append($"{StringTools.StringToCSVCell(s)},");
            }

            return sb.ToString().TrimEnd(',');

        }


        

    }

    public class PBIWireTable
    {
        public IList<string> Columns { get; private set; }
        public IList<PBIWireRow> Rows { get; private set; }
        private PBIWireModel model;

        public PBIWireTable(PBIWireModel model)
        {
            // setup column names
            this.model = model;
            this.Rows = new List<PBIWireRow>();
            DeriveColumns();
        }

        private void DeriveColumns()
        {
            var c = new string[model.DataDescriptionDict.Count];
            foreach(var kv in model.DataDescriptionDict)
            {
                c[kv.Value.Index] = kv.Value.Name;
            }

            Columns = new List<string>(c).AsReadOnly();
        }

        public bool isBitSet(long num, int pos)
        {
            return (num & (1 << pos)) != 0;
        }

        public void AddRow(JsonElement c, long r)
        {
            var row = new PBIWireRow(Columns.Count);


            var q = new Queue<object>();

            foreach(var cell in c.EnumerateArray())
            {
                var val = "";

                if (cell.ValueKind == JsonValueKind.String)
                {
                    // add the value as is
                    //row.SetCell(index, cell.GetString());
                    q.Enqueue(cell.GetString());
                }
                else if (cell.ValueKind == JsonValueKind.Number)
                {
                    // lookup the value in the dictionary
                    //long lookupDictionaryIndex = cell.GetInt64();

                    q.Enqueue(cell.GetInt32());
                }
                else
                {
                    throw new Exception($"Unexpected ValueKind {cell.ValueKind}");
                }
            }


            long rowBitMask = long.MaxValue ^ r;

            for(int index = 0; index < Columns.Count; index++)
            {
                if (!isBitSet( rowBitMask, index))
                {
                    // this is a duplicate
                    //row.SetCell(index, "dup");
                    var dupValue = Rows.Last().GetCell(index);
                    row.SetCell(index, dupValue);
                } 
                else
                {
                    var e = q.Dequeue();
                    if (e is string)
                    {
                        row.SetCell(index, (string)e);
                    } 
                    else
                    {
                        // need to lookup
                        var lookupVal = model.ValueDicts[$"D{index}"][(int)e];
                        //row.SetCell(index, $"D{index}[{(int)e}]");
                        row.SetCell(index, lookupVal);
                    }
                }
            }

            Console.WriteLine($"{row}");
            this.Rows.Add(row);
        }


    }


    public class PBIWireParser
    {

        public void ParseTable(string path, PBIWireModel model)
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            var json = File.ReadAllText(path);

            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                var root = document.RootElement;

                var results = root.Get("Response")?.Get("results")?.EnumerateArray().First();

                var dm0 = results?.Get("result")?.Get("data")?.Get("dsr")?.Get("DS")?.EnumerateArray().First().Get("PH")?.EnumerateArray().First().Get("DM0");


                var table = new PBIWireTable(model);

                int count = 0; 
                foreach(var row in dm0.Value.EnumerateArray())
                {
                    //Console.WriteLine($"Processed row {count++}");
                    var s = row.Get("S");
                    var c = row.Get("C");
                    var r = row.Get("R");

                    long bitMask = 0;

                    if (r.HasValue)
                    {
                        bitMask = r.Value.GetInt64();
                        //Console.WriteLine($"Found r = {r}");
                    }

                    if (c.HasValue)
                    {
                        //Console.WriteLine($"Found c with length {c.Value.GetArrayLength()}");
                        table.AddRow(c.Value, bitMask);
                    }

                    

                }

            }
        }


        public PBIWireModel BuildModel(string path)
        {
            var model = new PBIWireModel();
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            var json = File.ReadAllText(path);

            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                var root = document.RootElement;

                var node = root.Get("Response")?.Get("results");

                var columnCsv = new StringBuilder();

                
                

                


                foreach ( var r in node.Value.EnumerateArray())
                {
                    var select = r.Get("result")?.Get("data")?.Get("descriptor")?.Get("Select");

                    foreach( var d in select.Value.EnumerateArray())
                    {
                        

                        var v = d.Get("Value").Value.GetString();

                        var n = d.Get("GroupKeys")?.EnumerateArray().First().Get("Source")?.Get("Property")?.ToString();

                        
                        // Set the Column
                        var dd = new DataDescription { GroupCalc = v, Name = n };

                        //Console.WriteLine($"Found [{dd.Index}] {v},{n}");
                        //Console.WriteLine(StringTools.StringToCSVCell(n));

                        columnCsv.Append($"{StringTools.StringToCSVCell(n)},");

                        model.DataDescriptionDict.Add(v, dd);

                    }

                    Console.WriteLine(columnCsv.ToString().TrimEnd(','));

                    var valueDicts = r.Get("result")?.Get("data")?.Get("dsr")?.Get("DS")?.EnumerateArray().First().Get("ValueDicts");

                    // Assuming naming convention is D0, D1, D2..
                    for(int i = 0; i < model.DataDescriptionDict.Count();i++)
                    {
                        var dict = valueDicts?.Get($"D{i}");
                        var l = ToList(dict.Value);

                        //Console.WriteLine($"Found D{i} = {l}");

                        model.ValueDicts.Add($"D{i}", l);

                    }

                    //Console.WriteLine($"Found {dsr}");


                }

                //Console.WriteLine($"Found {node}");
            }
                return model;
        }


        public IList<string> ToList(JsonElement node)
        {
            var l = new List<string>();
            foreach (var e in node.EnumerateArray())
            {
                l.Add(e.GetString());
            }
            return l;
        }
    }
}
