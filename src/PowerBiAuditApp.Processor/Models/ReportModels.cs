using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    [JsonProperty("CacheKey", Required = Required.Always)]
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
    public BindingPrimary Primary { get; set; }

    [JsonProperty("DataReduction", Required = Required.Always)]
    public DataReduction DataReduction { get; set; }

    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }
}

public class DataReduction
{
    [JsonProperty("DataVolume", Required = Required.Always)]
    public long DataVolume { get; set; }

    [JsonProperty("Primary", Required = Required.Always)]
    public DataReductionPrimary Primary { get; set; }
}

public class DataReductionPrimary
{
    [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Top Top { get; set; }

    [JsonProperty("Window", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Window Window { get; set; }
}

public class Top
{
}

public class Window
{
    [JsonProperty("Count", Required = Required.Always)]
    public long Count { get; set; }
}
public class BindingPrimary
{
    [JsonProperty("Groupings", Required = Required.Always)]
    public PrimaryGrouping[] Groupings { get; set; }
}

public class PrimaryGrouping
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

public class OrderByExpression
{
    [JsonProperty("Column", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Column Column { get; set; }

    [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Aggregation Aggregation { get; set; }
}

public class Column
{
    [JsonProperty("Expression", Required = Required.Always)]
    public ColumnExpression Expression { get; set; }

    [JsonProperty("Property", Required = Required.Always)]
    public string Property { get; set; }
}

public class ColumnExpression
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

    [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Aggregation Aggregation { get; set; }
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

    [JsonProperty("Expressions", Required = Required.Always)]
    public Expressions Expressions { get; set; }

    [JsonProperty("Limits", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Limits Limits { get; set; }

    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }
}

public class Expressions
{
    [JsonProperty("Primary", Required = Required.Always)]
    public ExpressionsPrimary Primary { get; set; }
}

public class ExpressionsPrimary
{
    [JsonProperty("Groupings", Required = Required.Always)]
    public FluffyGrouping[] Groupings { get; set; }
}

public class FluffyGrouping
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

    [JsonProperty("Select", Required = Required.Always)]
    public int Select { get; set; }
}

public class KeySource
{
    [JsonProperty("Entity", Required = Required.Always)]
    public string Entity { get; set; }

    [JsonProperty("Property", Required = Required.Always)]
    public string Property { get; set; }
}

public class Limits
{
    [JsonProperty("Primary", Required = Required.Always)]
    public LimitsPrimary Primary { get; set; }
}

public class LimitsPrimary
{
    [JsonProperty("Id", Required = Required.Always)]
    public string Id { get; set; }

    [JsonProperty("Top", Required = Required.Always)]
    public FluffyTop Top { get; set; }
}

public class FluffyTop
{
    [JsonProperty("Count", Required = Required.Always)]
    public long Count { get; set; }
}

public class DescriptorSelect
{
    [JsonProperty("Kind", Required = Required.Always)]
    public DescriptorKind Kind { get; set; }

    [JsonProperty("Depth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? Depth { get; set; }

    [JsonProperty("Value", Required = Required.Always)]
    public string Value { get; set; }

    [JsonProperty("GroupKeys", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public GroupKey[] GroupKeys { get; set; }

    [JsonProperty("Name", Required = Required.Always)]
    public string Name { get; set; }
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

    [JsonProperty("IsSameAsSelect", Required = Required.Always)]
    public bool IsSameAsSelect { get; set; }
}

public class Dsr
{
    [JsonProperty("Version", Required = Required.Always)]
    public long Version { get; set; }

    [JsonProperty("MinorVersion", Required = Required.Always)]
    public long MinorVersion { get; set; }

    [JsonProperty("DS", Required = Required.Always)]
    public DataSet[] DataSets { get; set; }
}

public class DataSet
{
    [JsonProperty("N", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("PH", Required = Required.Always)]
    public Dictionary<string, DataRow[]>[] Ph { get; set; }

    [JsonProperty("IC", Required = Required.Always)]
    public bool Ic { get; set; }

    [JsonProperty("HAD")]
    public bool Had { get; set; }

    [JsonProperty("RT")]
    public string[][] Rt { get; set; }

    [JsonProperty("ValueDicts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string[]> ValueDictionary { get; set; }
}

public class DataRow
{
    [JsonProperty("S", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public ColumnHeader[] ColumnHeaders { get; set; }

    [JsonProperty("C", Required = Required.Always)]
    public RowValue[] RowValues { get; set; }

    [JsonProperty("R", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? CopyBitmask { get; set; }

    [JsonProperty("Ø", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public long? NullBitmask { get; set; }
}

public class ColumnHeader
{
    [JsonProperty("N", Required = Required.Always)]
    public string NameIndex { get; set; }

    [JsonProperty("T", Required = Required.Always)]
    public ColumnType ColumnType { get; set; }

    [JsonProperty("DN", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public string DataIndex { get; set; }
}

public enum ColumnType
{
    String = 1,
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
    public int? Integer;
    public string String;

    public static implicit operator RowValue(int integer) => new() { Integer = integer };
    public static implicit operator RowValue(string @string) => new() { String = @string };
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new() {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            }
    };
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
            case JsonToken.String:
            case JsonToken.Date:
                var stringValue = serializer.Deserialize<string>(reader);
                return new RowValue { String = stringValue };
        }
        throw new Exception("Cannot un-marshal type C");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        var value = (RowValue)untypedValue;
        if (value.Integer != null)
        {
            serializer.Serialize(writer, value.Integer.Value);
            return;
        }
        if (value.String != null)
        {
            serializer.Serialize(writer, value.String);
            return;
        }
        throw new Exception("Cannot marshal type C");
    }
}

