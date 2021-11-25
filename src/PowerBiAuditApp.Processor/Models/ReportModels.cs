using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace PowerBiAuditApp.Processor.Models
{
    public class AuditLog
    {
        [JsonProperty("User", Required = Required.Always)]
        public string User { get; set; }

        [JsonProperty("Date", Required = Required.Always)]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("Request", Required = Required.Always)]
        public PowerBiRequestRequest Request { get; set; }

        [JsonProperty("Response", Required = Required.Always)]
        public Response Response { get; set; }
    }

    public class PowerBiRequestRequest
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

        [JsonProperty("ApplicationContext", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
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

        [JsonProperty("ExecutionMetricsKind", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long ExecutionMetricsKind { get; set; }
    }

    public class Binding
    {
        [JsonProperty("Primary", Required = Required.Always)]
        public PowerBiBinding Primary { get; set; }

        [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PowerBiBinding Secondary { get; set; }

        [JsonProperty("DataReduction", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DataReduction DataReduction { get; set; }

        [JsonProperty("Version", Required = Required.Always)]
        public long Version { get; set; }

        [JsonProperty("Aggregates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public BindingAggregate[] Aggregates { get; set; }

        [JsonProperty("IncludeEmptyGroups", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool IncludeEmptyGroups { get; set; }

        [JsonProperty("SuppressedJoinPredicates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] SuppressedJoinPredicates { get; set; }

        [JsonProperty("Projections", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] Projections { get; set; }

        [JsonProperty("Highlights", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Highlight[] Highlight { get; set; }
    }

    public class BindingAggregate
    {
        [JsonProperty("Select", Required = Required.Always)]
        public long Select { get; set; }

        [JsonProperty("Aggregations", Required = Required.Always)]
        public Aggregate[] Aggregations { get; set; }
    }

    public class Aggregate
    {
        [JsonProperty("Scope", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Scope Scope { get; set; }

        [JsonProperty("Min", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Empty Min { get; set; }

        [JsonProperty("Max", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Empty Max { get; set; }

        [JsonProperty("RespectInstanceFilters", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool RespectInstanceFilters { get; set; }
    }

    public class Scope
    {
        [JsonProperty("Primary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] Primary { get; set; }

        [JsonProperty("PrimaryDepth", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long PrimaryDepth { get; set; }
    }


    public class Empty
    {
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

        [JsonProperty("Primary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limits Primary { get; set; }

        [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limits Secondary { get; set; }


        [JsonProperty("Scoped", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DataReductionScoped[] Scoped { get; set; }

        [JsonProperty("Intersection", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DataReductionIntersection Intersection { get; set; }
    }

    public class DataReductionIntersection
    {
        [JsonProperty("BinnedLineSample", Required = Required.Always)]
        public BinnedLineSample BinnedLineSample { get; set; }
    }

    public class DataReductionScoped
    {
        [JsonProperty("Scope", Required = Required.Always)]
        public Scope Scope { get; set; }

        [JsonProperty("Algorithm", Required = Required.Always)]
        public Algorithm Algorithm { get; set; }
    }
    public class Algorithm
    {
        [JsonProperty("Window", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Window { get; set; }

        [JsonProperty("Sample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Sample { get; set; }
    }

    public class Limits
    {
        [JsonProperty("Id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Top { get; set; }

        [JsonProperty("Bottom", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Bottom { get; set; }


        [JsonProperty("Scope", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Scope Scope { get; set; }

        [JsonProperty("Window", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Window { get; set; }


        [JsonProperty("OverlappingPointsSample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public OverlappingPointsSample OverlappingPointsSample { get; set; }

        [JsonProperty("Sample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Sample { get; set; }


        [JsonProperty("BinnedLineSample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public BinnedLineSample BinnedLineSample { get; set; }

        [JsonProperty("TopNPerLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public TopNPerLevel TopNPerLevel { get; set; }
    }

    public class TopNPerLevel
    {
        [JsonProperty("Count", Required = Required.Always)]
        public long Count { get; set; }

        [JsonProperty("WindowExpansion", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public WindowExpansion WindowExpansion { get; set; }
    }

    public class WindowExpansion
    {
        [JsonProperty("From", Required = Required.Always)]
        public From[] From { get; set; }

        [JsonProperty("Levels", Required = Required.Always)]
        public Level[] Levels { get; set; }

        [JsonProperty("WindowInstances", Required = Required.Always)]
        public WindowInstances WindowInstances { get; set; }
    }

    public class WindowInstances
    {
        [JsonProperty("Values", Required = Required.Always)]
        public LiteralExpression[] Values { get; set; }

        [JsonProperty("WindowExpansionInstanceWindowValue", Required = Required.Always)]
        public object[] WindowExpansionInstanceWindowValue { get; set; }

        [JsonProperty("Children", Required = Required.Always)]
        public WindowInstances[] Children { get; set; }
    }

    public class Limit
    {
        [JsonProperty("Count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Count { get; set; }

        [JsonProperty("Calc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Calc { get; set; }


        [JsonProperty("RestartTokens", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[][] RestartTokens { get; set; }
    }

    public class BinnedLineSample
    {
        [JsonProperty("MaxTargetPointCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long MaxTargetPointCount { get; set; }

        [JsonProperty("MinPointsPerSeriesCount", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long MinPointsPerSeriesCount { get; set; }

        [JsonProperty("IntersectionDbCountCalc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string IntersectionDbCountCalc { get; set; }

        [JsonProperty("PrimaryScalarKey", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long PrimaryScalarKey { get; set; }

        [JsonProperty("SecondaryScalarKey", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long SecondaryScalarKey { get; set; }

        [JsonProperty("PrimaryDbCountCalc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryDbCountCalc { get; set; }

        [JsonProperty("SecondaryDbCountCalc", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SecondaryDbCountCalc { get; set; }
    }

    public class OverlappingPointsSample
    {
        [JsonProperty("X", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PointsSampleValue X { get; set; }

        [JsonProperty("Y", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PointsSampleValue Y { get; set; }

        [JsonProperty("Count", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Count { get; set; }
    }

    public class PointsSampleValue
    {
        [JsonProperty("Index", Required = Required.Always)]
        public long Index { get; set; }
    }

    public class PowerBiBinding
    {
        [JsonProperty("Groupings", Required = Required.Always)]
        public PowerBiBindingGrouping[] Groupings { get; set; }

        [JsonProperty("Synchronization", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PowerBiSynchronization[] Synchronization { get; set; }

        [JsonProperty("Expansion", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Expansion Expansion { get; set; }
    }

    public class Expansion
    {
        [JsonProperty("From", Required = Required.Always)]
        public From[] From { get; set; }

        [JsonProperty("Levels", Required = Required.Always)]
        public Level[] Levels { get; set; }

        [JsonProperty("Instances", Required = Required.Always)]
        public Instances Instances { get; set; }
    }

    public class Level
    {
        [JsonProperty("Expressions", Required = Required.Always)]
        public ColumnExpression[] Expressions { get; set; }

        [JsonProperty("Default", Required = Required.Always)]
        public long Default { get; set; }
    }
    public class Instances
    {
        [JsonProperty("Values", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public LiteralExpression[] Values { get; set; }

        [JsonProperty("Children", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Instances[] Children { get; set; }
    }

    public class PowerBiSynchronization
    {
        [JsonProperty("Groupings", Required = Required.Always)]
        public long[] Groupings { get; set; }
    }

    public class PowerBiBindingGrouping
    {
        [JsonProperty("Projections", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] Projections { get; set; }

        [JsonProperty("Subtotal", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public int Subtotal { get; set; }

        [JsonProperty("SuppressedProjections", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] SuppressedProjections { get; set; }

        [JsonProperty("ShowItemsWithNoData", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long[] ShowItemsWithNoData { get; set; }

        [JsonProperty("Aggregates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public BindingAggregate[] Aggregates { get; set; }
    }

    public class SemanticQueryDataShapeCommandQuery
    {
        [JsonProperty("Version", Required = Required.Always)]
        public long Version { get; set; }

        [JsonProperty("From", Required = Required.Always)]
        public From[] From { get; set; }

        [JsonProperty("Select", Required = Required.Always)]
        public ColumnExpression[] Select { get; set; }

        [JsonProperty("Where", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Where[] Where { get; set; }

        [JsonProperty("OrderBy", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public OrderBy[] OrderBy { get; set; }

        [JsonProperty("Transform", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Transform[] Transform { get; set; }

        [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Top { get; set; }
    }
    public class Transform
    {
        [JsonProperty("Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("Algorithm", Required = Required.Always)]
        public string Algorithm { get; set; }

        [JsonProperty("Input", Required = Required.Always)]
        public TransformData Input { get; set; }

        [JsonProperty("Output", Required = Required.Always)]
        public TransformData Output { get; set; }
    }

    public class TransformData
    {
        [JsonProperty("Parameters", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public LiteralExpression[] Parameters { get; set; }

        [JsonProperty("Table", Required = Required.Always)]
        public PowerBiTable Table { get; set; }
    }

    public class PowerBiTable
    {
        [JsonProperty("Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("Columns", Required = Required.Always)]
        public PowerBiTableColumn[] Columns { get; set; }
    }

    public class PowerBiTableColumn
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public PowerBiTableExpression Expression { get; set; }

        [JsonProperty("Role", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Role { get; set; }
    }

    public class PowerBiTableExpression
    {
        [JsonProperty("Column", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Column Column { get; set; }

        [JsonProperty("Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Aggregation Aggregation { get; set; }

        [JsonProperty("Measure", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Column Measure { get; set; }

        [JsonProperty("TransformOutputRoleRef", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public TransformOutputRoleRef TransformOutputRoleRef { get; set; }
    }

    public class TransformOutputRoleRef
    {
        [JsonProperty("Role", Required = Required.Always)]
        public string Role { get; set; }
    }


    public class From
    {
        [JsonProperty("Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("Entity", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Entity { get; set; }


        [JsonProperty("Expression", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SubueryExpression Expression { get; set; }

        [JsonProperty("Type", Required = Required.Always)]
        public long Type { get; set; }
    }

    public class SubueryExpression
    {
        [JsonProperty("Subquery", Required = Required.Always)]
        public Subquery Subquery { get; set; }
    }

    public class Subquery
    {
        [JsonProperty("Query", Required = Required.Always)]
        public SemanticQueryDataShapeCommandQuery Query { get; set; }
    }

    public class OrderBy
    {
        [JsonProperty("Direction", Required = Required.Always)]
        public long Direction { get; set; }

        [JsonProperty("Expression", Required = Required.Always)]
        public ColumnExpression Expression { get; set; }
    }
    public class Where
    {
        [JsonProperty("Condition", Required = Required.Always)]
        public Condition Condition { get; set; }

        [JsonProperty("Target", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ColumnExpression[] Target { get; set; }
    }
    public class Condition
    {
        [JsonProperty("And", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison And { get; set; }

        [JsonProperty("Not", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Not Not { get; set; }

        [JsonProperty("Comparison", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Comparison { get; set; }

        [JsonProperty("Or", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Or { get; set; }

        [JsonProperty("In", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public In In { get; set; }

        [JsonProperty("Exists", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Column Exists { get; set; }
    }

    public class Not
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public NotExpression Expression { get; set; }
    }

    public class NotExpression
    {
        [JsonProperty("In", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public In In { get; set; }

        [JsonProperty("Comparison", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Comparison { get; set; }
    }

    public class In
    {
        [JsonProperty("Expressions", Required = Required.Always)]
        public ColumnExpression[] Expressions { get; set; }

        [JsonProperty("Values", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public LiteralExpression[][] Values { get; set; }

        [JsonProperty("Table", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SourceRefExpression Table { get; set; }
    }

    public class Comparison
    {
        [JsonProperty("ComparisonKind", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long ComparisonKind { get; set; }

        [JsonProperty("Left", Required = Required.Always)]
        public ColumnExpression Left { get; set; }

        [JsonProperty("Right", Required = Required.Always)]
        public LiteralExpression Right { get; set; }
    }
    public class ColumnExpression
    {
        [JsonProperty("Name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Measure", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Column Measure { get; set; }

        [JsonProperty("Column", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Column Column { get; set; }

        [JsonProperty("Aggregation", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Aggregation Aggregation { get; set; }

        [JsonProperty("Comparison", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Comparison { get; set; }

        [JsonProperty("Or", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Or { get; set; }

        [JsonProperty("SourceRef", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SourceRef SourceRef { get; set; }


        [JsonProperty("HierarchyLevel", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public HierarchyLevel HierarchyLevel { get; set; }
    }
    public class HierarchyLevel
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public HierarchyLevelExpression Expression { get; set; }

        [JsonProperty("Level", Required = Required.Always)]
        public string Level { get; set; }
    }

    public class HierarchyLevelExpression
    {
        [JsonProperty("Hierarchy", Required = Required.Always)]
        public Hierarchy Hierarchy { get; set; }
    }
    public class Hierarchy
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public HierarchyExpression Expression { get; set; }

        [JsonProperty("Hierarchy", Required = Required.Always)]
        public string HierarchyHierarchy { get; set; }
    }
    public class HierarchyExpression
    {
        [JsonProperty("PropertyVariationSource", Required = Required.Always)]
        public PropertyVariationSource PropertyVariationSource { get; set; }
    }
    public class PropertyVariationSource
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public ColumnExpression Expression { get; set; }

        [JsonProperty("Name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("Property", Required = Required.Always)]
        public string Property { get; set; }
    }


    public class LiteralExpression
    {
        [JsonProperty("Name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Literal", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ValueHolder Literal { get; set; }

        [JsonProperty("DateSpan", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateSpan DateSpan { get; set; }

        [JsonProperty("Comparison", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Comparison Comparison { get; set; }

        [JsonProperty("AnyValue", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public AnyValue AnyValue { get; set; }
    }
    public class AnyValue
    {
        [JsonProperty("DefaultValueOverridesAncestors", Required = Required.Always)]
        public bool DefaultValueOverridesAncestors { get; set; }
    }

    public class DateSpan
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public LiteralExpression Expression { get; set; }

        [JsonProperty("TimeUnit", Required = Required.Always)]
        public long TimeUnit { get; set; }
    }

    public class ValueHolder
    {
        [JsonProperty("Value", Required = Required.Always)]
        public string Value { get; set; }
    }


    public class Column
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public SourceRefExpression Expression { get; set; }

        [JsonProperty("Property", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Property { get; set; }
    }

    public class SourceRefExpression
    {
        [JsonProperty("SourceRef", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SourceRef SourceRef { get; set; }

        [JsonProperty("TransformTableRef", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SourceRef TransformTableRef { get; set; }
    }

    public class SourceRef
    {
        [JsonProperty("Source", Required = Required.Always)]
        public string Source { get; set; }
    }

    public class QueryMeasure
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public SourceRefExpression Expression { get; set; }
        [JsonProperty("Property", Required = Required.Always)]
        public string Property { get; set; }
    }

    public class Aggregation
    {
        [JsonProperty("Expression", Required = Required.Always)]
        public ColumnExpression Expression { get; set; }

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
        [JsonProperty("descriptor", Required = Required.AllowNull)]
        public Descriptor Descriptor { get; set; }

        [JsonProperty("dsr", Required = Required.Always)]
        public Dsr Dsr { get; set; }

        [JsonProperty("metrics", Required = Required.AllowNull)]
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


        [JsonProperty("Synchronization", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public GroupingSynchronization[] Synchronization { get; set; }
    }
    public class GroupingSynchronization
    {
        [JsonProperty("DataShape", Required = Required.Always)]
        public string DataShape { get; set; }

        [JsonProperty("Groupings", Required = Required.Always)]
        public long[] Groupings { get; set; }
    }

    public class ExpressionGroupingDetail
    {
        [JsonProperty("Keys", Required = Required.Always)]
        public Key[] Keys { get; set; }

        [JsonProperty("Member", Required = Required.Always)]
        public string Member { get; set; }

        [JsonProperty("SynchronizationIndex", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SynchronizationIndex { get; set; }

        [JsonProperty("SubtotalMember", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string SubtotalMember { get; set; }

        [JsonProperty("RestartIdentities", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[] RestartIdentities { get; set; }


        [JsonProperty("SynchronizedGroup", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ExpressionGroupingDetail SynchronizedGroup { get; set; }


        [JsonProperty("Aggregates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ExpressionGroupingAggregates Aggregates { get; set; }
    }

    public class ExpressionGroupingAggregates
    {
        [JsonProperty("AggregatesMember", Required = Required.Always)]
        public string AggregatesMember { get; set; }

        [JsonProperty("Ids", Required = Required.Always)]
        public string[] Ids { get; set; }
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

        [JsonProperty("Top", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limit Top { get; set; }

        [JsonProperty("BinnedLineSample", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public BinnedLineSample BinnedLineSample { get; set; }
    }

    public class LimitsGroup
    {
        [JsonProperty("Primary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limits Primary { get; set; }

        [JsonProperty("Secondary", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limits Secondary { get; set; }


        [JsonProperty("Intersection", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Intersection Intersection { get; set; }

        [JsonProperty("Scoped", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Limits[] Scoped { get; set; }
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
        public ValueHolder Highlight { get; set; }

        [JsonProperty("Min", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[] Min { get; set; }

        [JsonProperty("Max", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[] Max { get; set; }

        [JsonProperty("Subtotal", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[] Subtotal { get; set; }

        [JsonProperty("Aggregates", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SelectAggregate[] Aggregates { get; set; }

        [JsonProperty("Synchronized", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Synchronized Synchronized { get; set; }
    }

    public class Synchronized
    {
        [JsonProperty("Depth", Required = Required.Always)]
        public long Depth { get; set; }

        [JsonProperty("Value", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("GroupKeys", Required = Required.Always)]
        public GroupKey[] GroupKeys { get; set; }
    }

    public class SelectAggregate
    {
        [JsonProperty("Ids", Required = Required.Always)]
        public string[] Ids { get; set; }

        [JsonProperty("Aggregate", Required = Required.Always)]
        public Aggregate Aggregate { get; set; }
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
        [JsonProperty("Version", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long Version { get; set; }

        [JsonProperty("MinorVersion", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long MinorVersion { get; set; }

        [JsonProperty("DS", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PowerBiDataSet[] DataOrRow { get; set; }

        [JsonProperty("DataShapes", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DataShape[] DataShapes { get; set; }
    }

    public class DataShape
    {
        [JsonProperty("odata.error", Required = Required.Always)]
        public OdataError OdataError { get; set; }
    }

    public class OdataError
    {
        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty("source", Required = Required.Always)]
        public string Source { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public PowerBiDisplayMessage Message { get; set; }

        [JsonProperty("azure:values", Required = Required.Always)]
        public AzureValue[] AzureValues { get; set; }
    }

    public class AzureValue
    {
        [JsonProperty("timestamp", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("additionalMessages", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PowerBiMessage[] AdditionalMessages { get; set; }
    }
    public class PowerBiDisplayMessage
    {
        [JsonProperty("lang", Required = Required.Always)]
        public string Lang { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public string Value { get; set; }
    }

    public class PowerBiDataSet
    {
        [JsonProperty("N", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("S", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public ColumnHeader[] ColumnHeaders { get; set; }

        [JsonProperty("C", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public RowValue[] RowValues { get; set; } = Array.Empty<RowValue>(); //Limits rows see result.data.descriptor.Limits, gives counts etc i.e data on data

        [JsonProperty("PH", Required = Required.Always)]
        public Dictionary<string, PowerBiDataRow[]>[] PrimaryRows { get; set; }

        [JsonProperty("SH", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, PowerBiDataRow[]>[] SecondaryRows { get; set; } = Array.Empty<Dictionary<string, PowerBiDataRow[]>>();

        [JsonProperty("IC", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool Ic { get; set; }

        [JsonProperty("HAD", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool Had { get; set; }

        [JsonProperty("M0", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string M0 { get; set; }

        [JsonProperty("M1", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string M1 { get; set; }

        [JsonProperty("Msg", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public List<PowerBiMessage> Msg { get; set; }

        [JsonProperty("RT", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string[][] Rt { get; set; }

        [JsonProperty("A0", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long A0 { get; set; }

        [JsonProperty("ValueDicts", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string[]> ValueDictionary { get; set; }

        [JsonProperty("DS", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public PowerBiDataSet[] DataOrRow { get; set; }

        [JsonProperty("DW", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Dw[] Dw { get; set; }

        [JsonProperty("DLEx", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public DlEx[] DlEx { get; set; }
    }

    public class DlEx
    {
        [JsonProperty("N", Required = Required.Always)]
        public string N { get; set; }
    }

    public class Dw
    {
        [JsonProperty("N", Required = Required.Always)]
        public string N { get; set; }

        [JsonProperty("IC", Required = Required.Always)]
        public bool Ic { get; set; }

        [JsonProperty("RT", Required = Required.Always)]
        public string[][] Rt { get; set; }
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

    public class PowerBiDataRow
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

        [JsonProperty("M", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, PowerBiDataRow[]>[] M { get; set; }

        [JsonProperty("RF", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public long? Rf { get; set; }

        [JsonIgnore]
        public Dictionary<string, RowValue> ValueLookup { get; } = new();

        [OnDeserialized]
        // ReSharper disable once UnusedMember.Local // JSON special prop
        // ReSharper disable once UnusedParameter.Local // JSON special prop
        private void OnDeserialized(StreamingContext context)
        {
            foreach (var (key, value) in _additionalData)
            {
                ValueLookup[key] = value.Type switch {
                    JTokenType.Integer => new RowValue { Integer = value.ToObject<long>() },
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

        public Dictionary<string, string> ValueLookup { get; } = new();

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

        [JsonIgnore]
        public string MatrixKey { get; set; }

        [JsonIgnore]
        public int? MatrixRowIndex { get; set; }

        [JsonIgnore]
        public int? MatrixColumnIndex { get; set; }


        public ColumnHeader Clone() => (ColumnHeader)MemberwiseClone();
    }

    public enum ColumnType
    {
        Invalid = 0,
        String = 1,
        Double = 3,
        Int = 4,
        Long = 7
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
        public DateTime? Date { get; set; }
        public bool IsNull { get; set; }

        public long? Integer;
        public string String;
    }

    internal class RowValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(RowValue) || t == typeof(RowValue?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Date:
                    var dateValue = serializer.Deserialize<DateTime>(reader);
                    return new RowValue { Date = dateValue };
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new RowValue { Integer = integerValue };
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new RowValue { Double = doubleValue };
                case JsonToken.Null:
                    return new RowValue { IsNull = true };
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
            if (value.IsNull)
            {
                serializer.Serialize(writer, null);
                return;
            }
            if (value.Date is not null)
            {
                serializer.Serialize(writer, value.Date);
                return;
            }
            throw new Exception("Cannot marshal type RowValue");
        }
    }
}