using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace PowerBiAuditApp.Processor.Models;

public class AuditLog
{
    [JsonProperty("User", Required = Required.Always)]
    public string User { get; set; }

    [JsonProperty("Date", Required = Required.Always)]
    public DateTimeOffset Date { get; set; }

    [JsonProperty("Request", Required = Required.Always)]
    public Request Request { get; set; }

    [JsonProperty("Response", Required = Required.Always)]
    public Response Response { get; set; }
}

public class Request
{
    [JsonProperty("version", Required = Required.Always)]
    public string Version { get; set; }

    [JsonProperty("queries", Required = Required.Always)]
    public Query[] Queries { get; set; }

    [JsonProperty("cancelQueries", Required = Required.Always)]
    public object[] CancelQueries { get; set; }

    [JsonProperty("modelId", Required = Required.Always)]
    public long ModelId { get; set; }

    [JsonProperty("userPreferredLocale", Required = Required.Always)]
    public string UserPreferredLocale { get; set; }
}

public class Query
{
    [JsonProperty("Query", Required = Required.Always)]
    public QueryQuery QueryQuery { get; set; }

    [JsonProperty("CacheKey", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string CacheKey { get; set; }

    [JsonProperty("QueryId", Required = Required.Always)]
    public string QueryId { get; set; }

    [JsonProperty("ApplicationContext", Required = Required.Always)]
    public ApplicationContext ApplicationContext { get; set; }
}

public class ApplicationContext
{
    [JsonProperty("DatasetId", Required = Required.Always)]
    public Guid DatasetId { get; set; }

    [JsonProperty("Sources", Required = Required.Always)]
    public SourceElement[] Sources { get; set; }
}

public class SourceElement
{
    [JsonProperty("ReportId", Required = Required.Always)]
    public Guid ReportId { get; set; }

    [JsonProperty("VisualId", Required = Required.Always)]
    public string VisualId { get; set; }
}

public class QueryQuery
{
    [JsonProperty("Commands", Required = Required.Always)]
    public Command[] Commands { get; set; }
}

public class Command
{
    [JsonProperty("SemanticQueryDataShapeCommand", Required = Required.Always)]
    public SemanticQueryDataShapeCommand SemanticQueryDataShapeCommand { get; set; }
}

public class SemanticQueryDataShapeCommand
{
    [JsonProperty("Query", Required = Required.Always)]
    public SemanticQueryDataShapeCommandQuery Query { get; set; }

    [JsonProperty("Binding", Required = Required.Always)]
    public Binding Binding { get; set; }

    [JsonProperty("ExecutionMetricsKind", Required = Required.Always)]
    public long ExecutionMetricsKind { get; set; }
}

public class Binding
{
    [JsonProperty("Primary", Required = Required.Always)]
    public PowerBiBinding Primary { get; set; }

    [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public PowerBiBinding Secondary { get; set; }

    [JsonProperty("DataReduction", Required = Required.Always)]
    public DataReduction DataReduction { get; set; }

    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }

    [JsonProperty("IncludeEmptyGroups", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool IncludeEmptyGroups { get; set; }


    [JsonProperty("Highlights", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Highlight[] Highlight { get; set; }
}

public class Highlight
{
    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }

    [JsonProperty("From", Required = Required.Always)]
    public From[] From { get; set; }

    [JsonProperty("Where", Required = Required.Always)]
    public Where[] Where { get; set; }
}


public class DataReduction
{
    [JsonProperty("DataVolume", Required = Required.Always)]
    public long DataVolume { get; set; }

    [JsonProperty("Primary", Required = Required.Always)]
    public DataReductionGrouping Primary { get; set; }

    [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public DataReductionGrouping Secondary { get; set; }
}

public class DataReductionGrouping
{
    [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Top Top { get; set; }

    [JsonProperty("Window", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Window Window { get; set; }


    [JsonProperty("OverlappingPointsSample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public OverlappingPointsSample OverlappingPointsSample { get; set; }
}

public class OverlappingPointsSample
{
    [JsonProperty("X", Required = Required.Always)]
    public PointsSampleValue X { get; set; }

    [JsonProperty("Y", Required = Required.Always)]
    public PointsSampleValue Y { get; set; }
}

public class PointsSampleValue
{
    [JsonProperty("Index", Required = Required.Always)]
    public long Index { get; set; }
}


public class Window
{
    [JsonProperty("Count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? Count { get; set; }
}
public class PowerBiBinding
{
    [JsonProperty("Groupings", Required = Required.Always)]
    public PowerBiBindingGrouping[] Groupings { get; set; }
}

public class PowerBiBindingGrouping
{
    [JsonProperty("Projections", Required = Required.Always)]
    public long[] Projections { get; set; }

    [JsonProperty("Subtotal", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public int Subtotal { get; set; }
}

public class SemanticQueryDataShapeCommandQuery
{
    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }

    [JsonProperty("From", Required = Required.Always)]
    public From[] From { get; set; }

    [JsonProperty("Select", Required = Required.Always)]
    public QuerySelect[] Select { get; set; }

    [JsonProperty("Where", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Where[] Where { get; set; }

    [JsonProperty("OrderBy", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public OrderBy[] OrderBy { get; set; }
}

public class From
{
    [JsonProperty("Name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("Entity", Required = Required.Always)]
    public string Entity { get; set; }

    [JsonProperty("Type", Required = Required.Always)]
    public long Type { get; set; }
}

public class OrderBy
{
    [JsonProperty("Direction", Required = Required.Always)]
    public long Direction { get; set; }

    [JsonProperty("Expression", Required = Required.Always)]
    public OrderByExpression Expression { get; set; }
}
public class Where
{
    [JsonProperty("Condition", Required = Required.Always)]
    public Condition Condition { get; set; }
}
public class Condition
{
    [JsonProperty("Comparison", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Comparison Comparison { get; set; }

    [JsonProperty("In", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public In In { get; set; }
}

public class In
{
    [JsonProperty("Expressions", Required = Required.Always)]
    public ColumnExpression[] Expressions { get; set; }

    [JsonProperty("Values", Required = Required.Always)]
    public LiteralExpression[][] Values { get; set; }
}

public class Comparison
{
    [JsonProperty("ComparisonKind", Required = Required.Always)]
    public long ComparisonKind { get; set; }

    [JsonProperty("Left", Required = Required.Always)]
    public ColumnExpression Left { get; set; }

    [JsonProperty("Right", Required = Required.Always)]
    public LiteralExpression Right { get; set; }
}
public class ColumnExpression
{
    [JsonProperty("Column", Required = Required.Always)]
    public Column Column { get; set; }
}

public class LiteralExpression
{
    [JsonProperty("Literal", Required = Required.Always)]
    public ComparisonLiteral Literal { get; set; }
}

public class ComparisonLiteral
{
    [JsonProperty("Value", Required = Required.Always)]
    public string Value { get; set; }
}


public class OrderByExpression
{

    [JsonProperty("Measure", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public OrderByMeasure Measure { get; set; }

    [JsonProperty("Column", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Column Column { get; set; }

    [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Aggregation Aggregation { get; set; }
}

public class OrderByMeasure
{
    [JsonProperty("Expression", Required = Required.Always)]
    public SourceRefExpression Expression { get; set; }

    [JsonProperty("Property", Required = Required.Always)]
    public string Property { get; set; }
}

public class Column
{
    [JsonProperty("Expression", Required = Required.Always)]
    public SourceRefExpression Expression { get; set; }

    [JsonProperty("Property", Required = Required.Always)]
    public string Property { get; set; }
}

public class SourceRefExpression
{
    [JsonProperty("SourceRef", Required = Required.Always)]
    public SourceRef SourceRef { get; set; }
}

public class SourceRef
{
    [JsonProperty("Source", Required = Required.Always)]
    public string Source { get; set; }
}

public class QuerySelect
{
    [JsonProperty("Column", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Column Column { get; set; }

    [JsonProperty("Name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("Measure", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public QueryMeasure Measure { get; set; }

    [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Aggregation Aggregation { get; set; }
}

public class QueryMeasure
{
    public SourceRefExpression Expression { get; set; }
    public string Property { get; set; }
}

public class Aggregation
{
    [JsonProperty("Expression", Required = Required.Always)]
    public OrderByExpression Expression { get; set; }

    [JsonProperty("Function", Required = Required.Always)]
    public long Function { get; set; }
}

public class Response
{
    [JsonProperty("jobIds", Required = Required.Always)]
    public Guid[] JobIds { get; set; }

    [JsonProperty("results", Required = Required.Always)]
    public ResultElement[] Results { get; set; }
}

public class ResultElement
{
    [JsonProperty("jobId", Required = Required.Always)]
    public Guid JobId { get; set; }

    [JsonProperty("result", Required = Required.Always)]
    public ResultResult Result { get; set; }
}

public class ResultResult
{
    [JsonProperty("data", Required = Required.Always)]
    public Data Data { get; set; }
}

public class Data
{
    [JsonProperty("descriptor", Required = Required.Always)]
    public Descriptor Descriptor { get; set; }

    [JsonProperty("dsr", Required = Required.Always)]
    public Dsr Dsr { get; set; }

    [JsonProperty("metrics", Required = Required.Always)]
    public Metrics Metrics { get; set; }
}

public class Descriptor
{
    [JsonProperty("Select", Required = Required.Always)]
    public DescriptorSelect[] Select { get; set; }

    [JsonProperty("Expressions", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Expressions Expressions { get; set; }

    [JsonProperty("Limits", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public LimitsGroup Limits { get; set; }

    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }
}

public class Expressions
{
    [JsonProperty("Primary", Required = Required.Always)]
    public ExpressionGrouping Primary { get; set; }

    [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ExpressionGrouping Secondary { get; set; }
}

public class ExpressionGrouping
{
    [JsonProperty("Groupings", Required = Required.Always)]
    public ExpressionGroupingDetail[] Groupings { get; set; }
}

public class ExpressionGroupingDetail
{
    [JsonProperty("Keys", Required = Required.Always)]
    public Key[] Keys { get; set; }

    [JsonProperty("Member", Required = Required.Always)]
    public string Member { get; set; }
}

public class Key
{
    [JsonProperty("Source", Required = Required.Always)]
    public KeySource Source { get; set; }

    [JsonProperty("Select", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public int Select { get; set; }

    [JsonProperty("Calc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string Calc { get; set; }
}

public class KeySource
{
    [JsonProperty("Entity", Required = Required.Always)]
    public string Entity { get; set; }

    [JsonProperty("Property", Required = Required.Always)]
    public string Property { get; set; }
}
public class Intersection
{
    [JsonProperty("Id", Required = Required.Always)]
    public string Id { get; set; }

    [JsonProperty("Top", Required = Required.Always)]
    public Top Top { get; set; }
}

public class LimitsGroup
{
    [JsonProperty("Primary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Limits Primary { get; set; }

    [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Limits Secondary { get; set; }


    [JsonProperty("Intersection", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Intersection Intersection { get; set; }
}

public class Limits
{
    [JsonProperty("Id", Required = Required.Always)]
    public string Id { get; set; }

    [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Top Top { get; set; }

    [JsonProperty("OverlappingPointsSample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public OverlappingPointsSampleLimits OverlappingPointsSample { get; set; }
}
public class OverlappingPointsSampleLimits
{
    [JsonProperty("Count", Required = Required.Always)]
    public long Count { get; set; }
}

public class Top
{
    [JsonProperty("Count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long Count { get; set; }

    [JsonProperty("Calc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string Calc { get; set; }
}

public class DescriptorSelect
{
    [JsonProperty("Kind", Required = Required.Always)]
    public DescriptorKind Kind { get; set; }

    [JsonProperty("Depth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? Depth { get; set; }

    [JsonProperty("SecondaryDepth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? SecondaryDepth { get; set; }

    [JsonProperty("Value", Required = Required.Always)]
    public string Value { get; set; }

    [JsonProperty("GroupKeys", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public GroupKey[] GroupKeys { get; set; }

    [JsonProperty("Name", Required = Required.Always)]
    public string Name { get; set; }


    [JsonProperty("Format", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string Format { get; set; }

    [JsonProperty("Highlight", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ComparisonLiteral Highlight { get; set; }
}

public enum DescriptorKind
{
    Select = 1,
    Grouping = 2
}

public class GroupKey
{
    [JsonProperty("Source", Required = Required.Always)]
    public KeySource Source { get; set; }

    [JsonProperty("Calc", Required = Required.Always)]
    public string Calc { get; set; }

    [JsonProperty("IsSameAsSelect", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSameAsSelect { get; set; }
}

public class Dsr
{
    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }

    [JsonProperty("MinorVersion", Required = Required.Always)]
    public long MinorVersion { get; set; }

    [JsonProperty("DS", Required = Required.Always)]
    public DataSet[] DataOrRow { get; set; }
}

public class DataSet
{
    [JsonProperty("N", Required = Required.Always)]
    public string Name { get; set; }


    [JsonProperty("S", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public List<ColumnHeader> S { get; set; }


    [JsonProperty("C", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public List<int> C { get; set; }

    [JsonProperty("PH", Required = Required.Always)]
    public Dictionary<string, DataRow[]>[] PrimaryRows { get; set; }

    [JsonProperty("SH", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, DataRow[]>[] SecondaryRows { get; set; } = Array.Empty<Dictionary<string, DataRow[]>>();

    [JsonProperty("IC", Required = Required.Always)]
    public bool Ic { get; set; }

    [JsonProperty("HAD", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public bool Had { get; set; }

    [JsonProperty("Msg", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public List<PowerBiMessage> Msg { get; set; }

    [JsonProperty("RT", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string[][] Rt { get; set; }

    [JsonProperty("ValueDicts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string[]> ValueDictionary { get; set; }
}

public class PowerBiMessage
{
    [JsonProperty("Code", Required = Required.Always)]
    public string Code { get; set; }

    [JsonProperty("Severity", Required = Required.Always)]
    public string Severity { get; set; }

    [JsonProperty("Message", Required = Required.Always)]
    public string Message { get; set; }
}

public class DataRow
{
    [JsonProperty("S", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ColumnHeader[] ColumnHeaders { get; set; }

    [JsonProperty("C", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public RowValue[] RowValues { get; set; } = Array.Empty<RowValue>();

    [JsonProperty("R", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? RepeatBitmask { get; set; }

    [JsonProperty("Ø", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? NullBitmask { get; set; }

    [JsonProperty("X", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public SubDataRow[] SubDataRows { get; set; }

    [JsonIgnore]
    public Dictionary<string, RowValue> ValueLookup { get; set; } = new();

    [OnDeserialized]
    // ReSharper disable once UnusedMember.Local // JSON special prop
    // ReSharper disable once UnusedParameter.Local // JSON special prop
    private void OnDeserialized(StreamingContext context)
    {
        foreach (var (key, value) in _additionalData)
        {
            ValueLookup[key] = value.Type switch {
                JTokenType.Integer => new RowValue { Integer = value.ToObject<int>() },
                JTokenType.Float => new RowValue { Double = value.ToObject<double>() },
                JTokenType.String => new RowValue { String = value.ToObject<string>() },
                _ => throw new Exception("Cannot un-marshal type RowValue")
            };
        }
    }

    [JsonExtensionData]
    // ReSharper disable once CollectionNeverUpdated.Local // JSON special prop
    private readonly IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();
}

public class SubDataRow
{
    [JsonProperty("S", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ColumnHeader[] ColumnHeaders { get; set; }

    [JsonProperty("I", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? Index { get; set; }

    [JsonProperty("C", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public RowValue[] RowValues { get; set; }

    [JsonProperty("R", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? RepeatBitmask { get; set; }

    [JsonProperty("Ø", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? NullBitmask { get; set; }

    public Dictionary<string, string> ValueLookup { get; set; } = new();

    [OnDeserialized]
    // ReSharper disable once UnusedMember.Local // JSON special prop
    // ReSharper disable once UnusedParameter.Local // JSON special prop
    private void OnDeserialized(StreamingContext context)
    {
        foreach (var (key, value) in _additionalData)
        {
            ValueLookup[key] = value.ToObject<string>();
        }
    }

    [JsonExtensionData]
    // ReSharper disable once CollectionNeverUpdated.Local // JSON special prop
    private readonly IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();
}

public class ColumnHeader
{
    [JsonProperty("N", Required = Required.Always)]
    public string NameIndex { get; set; }

    [JsonProperty("T", Required = Required.Always)]
    public ColumnType ColumnType { get; set; }

    [JsonProperty("DN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string DataIndex { get; set; }

    [JsonIgnore]
    public int? SubDataRowIndex { get; set; }

    [JsonIgnore]
    public int? SubDataColumnIndex { get; set; }


    public ColumnHeader Clone() => (ColumnHeader)MemberwiseClone();
}

public enum ColumnType
{
    Invalid = 0,
    String = 1,
    Double = 3,
    Int = 4
}

public class Metrics
{
    [JsonProperty("Version", Required = Required.Always)]
    public string Version { get; set; }

    [JsonProperty("Events", Required = Required.Always)]
    public Event[] Events { get; set; }
}

public class Event
{
    [JsonProperty("Id", Required = Required.Always)]
    public string Id { get; set; }

    [JsonProperty("Name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("Component", Required = Required.Always)]
    public string Component { get; set; }

    [JsonProperty("Start", Required = Required.Always)]
    public DateTimeOffset Start { get; set; }

    [JsonProperty("End", Required = Required.Always)]
    public DateTimeOffset End { get; set; }

    [JsonProperty("ParentId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string ParentId { get; set; }

    [JsonProperty("Metrics", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public MetricsClass Metrics { get; set; }
}

public class MetricsClass
{
    [JsonProperty("RowCount", Required = Required.Always)]
    public long RowCount { get; set; }
}

[JsonConverter(typeof(RowValueConverter))]
public struct RowValue
{
    public double? Double { get; set; }
    public int? Integer;
    public string String;
}

internal class RowValueConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(RowValue) || t == typeof(RowValue?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Integer:
                var integerValue = serializer.Deserialize<int>(reader);
                return new RowValue { Integer = integerValue };
            case JsonToken.Float:
                var doubleValue = serializer.Deserialize<double>(reader);
                return new RowValue { Double = doubleValue };
            case JsonToken.String:
                var stringValue = serializer.Deserialize<string>(reader);
                return new RowValue { String = stringValue };
        }
        throw new Exception("Cannot un-marshal type RowValue");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        var value = (RowValue)untypedValue;
        if (value.Integer is not null)
        {
            serializer.Serialize(writer, value.Integer.Value);
            return;
        }
        if (value.String is not null)
        {
            serializer.Serialize(writer, value.String);
            return;
        }
        if (value.Double is not null)
        {
            serializer.Serialize(writer, value.Double);
            return;
        }
        throw new Exception("Cannot marshal type RowValue");
    }
}