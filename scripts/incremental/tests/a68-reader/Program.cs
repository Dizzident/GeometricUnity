using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Synthetic parser/metadata tests ONLY. Assembly.Load does not invoke the
// referenced executable's Program.Main. No study constructor, evaluator,
// tensor replay, coefficient calculation or output emitter is invoked.
internal static class ReaderMetadataTests
{
    private const string Schema = "phase627-typed-mixed-dag-v1";
    private static readonly string EmptyTensorHash = Hash(Encoding.UTF8.GetBytes("[]"));
    private static readonly string Minimal = "{\"schemaVersion\":\"" + Schema + "\",\"leaves\":[],\"nodes\":[],\"marks\":[]}\n";
    private static readonly string ZeroNode = "{\"id\":0,\"op\":\"zero\",\"degree\":-1,\"inputs\":[],\"parameters\":{},\"records\":0,\"bytes\":2,\"sha256\":\"" + EmptyTensorHash + "\"}";
    private static readonly string ZeroGraph = Graph(ZeroNode);
    private static Type limitsType = null!;
    private static MethodInfo readGraph = null!;
    private static int passed;

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Graph(string nodes, string marks = "", string leaves = "") =>
        "{\"schemaVersion\":\"" + Schema + "\",\"leaves\":[" + leaves + "],\"nodes\":[" + nodes + "],\"marks\":[" + marks + "]}\n";
    private static object Limits(long graphBytes = 100000, long tensorBytes = 100000, int rationalCharacters = 256) =>
        Activator.CreateInstance(limitsType, 1000, 1000, 1000, rationalCharacters, tensorBytes, graphBytes, 100000L, 100000L, 100000L)
        ?? throw new Exception("Could not construct parser-only resource limits");
    private static object Parse(byte[] bytes, object? limits = null, string? expectedHash = null)
    {
        try
        {
            return readGraph.Invoke(null, new object[] { (ReadOnlyMemory<byte>)bytes, limits ?? Limits(), expectedHash ?? Hash(bytes) })
                ?? throw new Exception("ReadGraph returned null");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
    private static object Parse(string text, object? limits = null, string? expectedHash = null) => Parse(Encoding.UTF8.GetBytes(text), limits, expectedHash);
    private static void Reject(Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex.GetType().Name == "EvidenceFailure" || ex is JsonException or FormatException or OverflowException or InvalidOperationException)
        { return; }
        throw new Exception("Malformed synthetic input was accepted (or unexpected exception type escaped)");
    }
    private static void Need(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Test(string name, Action body) { body(); passed++; Console.WriteLine("PASS metadata-only: " + name); }

    private static int Main()
    {
        try
        {
            var assembly = Assembly.Load("Phase627FullMixedMetricNativeFieldVariationAudit");
            var traceType = assembly.GetType("MixedTrace", throwOnError: true)!;
            limitsType = traceType.GetNestedType("Limits", BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new Exception("Required MixedTrace.Limits type missing");
            readGraph = traceType.GetMethod("ReadGraph", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(ReadOnlyMemory<byte>), limitsType, typeof(string) }, null)
                ?? throw new Exception("Required three-argument hash-pinned ReadGraph API missing; no fallback allowed");

            Test("canonical empty graph", () =>
            {
                var graph = Parse(Minimal);
                Need((string?)graph.GetType().GetProperty("SchemaVersion")!.GetValue(graph) == Schema, "Decoded schema mismatch");
                Need(((Array)graph.GetType().GetProperty("Nodes")!.GetValue(graph)!).Length == 0, "Unexpected node count");
            });
            Test("canonical zero node and typed mark", () =>
            {
                string mark = "{\"name\":\"synthetic-zero\",\"degree\":2,\"node\":0,\"expanded\":true,\"sha256\":\"" + EmptyTensorHash + "\"}";
                var graph = Parse(Graph(ZeroNode, mark));
                Need(((Array)graph.GetType().GetProperty("Marks")!.GetValue(graph)!).Length == 1, "Missing synthetic mark");
            });
            Test("expected exact-byte hash is mandatory and checked", () =>
            {
                Reject(() => Parse(Minimal, expectedHash: new string('0', 64)));
                Reject(() => Parse(Minimal, expectedHash: Hash(Encoding.UTF8.GetBytes(Minimal)).ToUpperInvariant()));
                Reject(() => Parse(Minimal, expectedHash: "bad-hash"));
            });
            Test("duplicate root and nested parameter keys", () =>
            {
                Reject(() => Parse(Minimal.Replace("\"leaves\":[]", "\"leaves\":[],\"leaves\":[]", StringComparison.Ordinal)));
                Reject(() => Parse(Graph(ZeroNode.Replace("\"parameters\":{}", "\"parameters\":{\"a\":1,\"a\":2}", StringComparison.Ordinal))));
            });
            Test("negative zero, exponent, leading zero and unsafe integer tokens", () =>
            {
                foreach (string number in new[] { "-0", "0e0", "0.0", "00", "9007199254740992", "-9007199254740992" })
                    Reject(() => Parse(ZeroGraph.Replace("\"id\":0", "\"id\":" + number, StringComparison.Ordinal)));
            });
            Test("printable ASCII and canonical string escapes", () =>
            {
                foreach (string token in new[] { "\\u007aero", "zéro", "zero\\t", "zero\\u007f", "\\ud800" })
                    Reject(() => Parse(ZeroGraph.Replace("\"op\":\"zero\"", "\"op\":\"" + token + "\"", StringComparison.Ordinal)));
                Reject(() => Parse(Minimal.Replace("\"marks\"", "\"\\u006darks\"", StringComparison.Ordinal)));
            });
            Test("compact JSON and exactly one terminal LF", () =>
            {
                foreach (string text in new[] { Minimal.TrimEnd('\n'), Minimal + "\n", Minimal.Replace("\n", "\r\n", StringComparison.Ordinal), " " + Minimal, Minimal.Replace("\"leaves\":", "\"leaves\": ", StringComparison.Ordinal), "\ufeff" + Minimal })
                    Reject(() => Parse(text));
            });
            Test("malformed UTF-8", () =>
            {
                var prefix = Encoding.UTF8.GetBytes("{\"schemaVersion\":\"");
                var suffix = Encoding.UTF8.GetBytes("\",\"leaves\":[],\"nodes\":[],\"marks\":[]}\n");
                foreach (byte[] invalid in new[] { new byte[] { 0xc0, 0xaf }, new byte[] { 0x80 }, new byte[] { 0xed, 0xa0, 0x80 } })
                    Reject(() => Parse(prefix.Concat(invalid).Concat(suffix).ToArray()));
            });
            Test("depth cap", () =>
            {
                string deep = string.Concat(Enumerable.Repeat("{\"x\":", 17)) + "0" + new string('}', 17);
                Reject(() => Parse(ZeroGraph.Replace("\"parameters\":{}", "\"parameters\":" + deep, StringComparison.Ordinal)));
            });
            Test("preparse byte ceiling and cross-runtime resource domain", () =>
            {
                int length = Encoding.UTF8.GetByteCount(Minimal);
                _ = Parse(Minimal, Limits(graphBytes: length));
                Reject(() => Parse(Minimal, Limits(graphBytes: length - 1)));
                Reject(() => Parse(Minimal, Limits(graphBytes: 9007199254740992)));
                Reject(() => Parse(Minimal, Limits(tensorBytes: 9007199254740992)));
                Reject(() => Parse(Minimal, Limits(rationalCharacters: 16385)));
            });
            Test("closed root, node and operation parameter shapes", () =>
            {
                Reject(() => Parse(Minimal.Replace(",\"marks\":[]", "", StringComparison.Ordinal)));
                Reject(() => Parse(Minimal.Replace("\"marks\":[]", "\"marks\":[],\"unknown\":0", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"op\":\"zero\"", "\"op\":\"unregistered\"", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"parameters\":{}", "\"parameters\":{\"unexpected\":0}", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"records\":0", "\"records\":0,\"unexpected\":0", StringComparison.Ordinal)));
            });
            Test("strict topology and inferred degree", () =>
            {
                Reject(() => Parse(ZeroGraph.Replace("\"id\":0", "\"id\":1", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"inputs\":[]", "\"inputs\":[0]", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"degree\":-1", "\"degree\":1", StringComparison.Ordinal)));
                Reject(() => Parse(ZeroGraph.Replace("\"records\":0", "\"records\":1", StringComparison.Ordinal)));
            });
            Test("exact declared leaf census", () =>
            {
                string leaf = "{\"id\":\"missing\",\"degree\":1,\"source\":\"synthetic/missing\",\"sha256\":\"" + EmptyTensorHash + "\"}";
                Reject(() => Parse(Graph(ZeroNode, leaves: leaf)));
            });
            Test("mark node, hash, uniqueness and boolean typing", () =>
            {
                string mark = "{\"name\":\"mark\",\"degree\":2,\"node\":0,\"expanded\":true,\"sha256\":\"" + EmptyTensorHash + "\"}";
                Reject(() => Parse(Graph(ZeroNode, mark.Replace("\"node\":0", "\"node\":1", StringComparison.Ordinal))));
                Reject(() => Parse(Graph(ZeroNode, mark.Replace(EmptyTensorHash, new string('0', 64), StringComparison.Ordinal))));
                Reject(() => Parse(Graph(ZeroNode, mark + "," + mark)));
                Reject(() => Parse(Graph(ZeroNode, mark.Replace("true", "1", StringComparison.Ordinal))));
            });
            Test("noncanonical rational parameters", () =>
            {
                foreach (string ratio in new[] { "2/4", "1/1", "-0", "01", "0/2" })
                {
                    string scale = "{\"id\":1,\"op\":\"scale\",\"degree\":-1,\"inputs\":[0],\"parameters\":{\"real\":\"" + ratio + "\",\"imaginary\":\"0\"},\"records\":0,\"bytes\":2,\"sha256\":\"" + EmptyTensorHash + "\"}";
                    Reject(() => Parse(Graph(ZeroNode + "," + scale)));
                }
            });
            Test("actual sink serializer camel-cases inferred geometry identity fields", () =>
            {
                var options = (JsonSerializerOptions)traceType.GetField("JsonOptions", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
                string wire = JsonSerializer.Serialize(new { Point = 1, MetricBasis = 2, JetIndex = 3, Multiindex = new[] { 0, 1, 0, 0 }, Order = 1 }, options);
                Need(wire == "{\"point\":1,\"metricBasis\":2,\"jetIndex\":3,\"multiindex\":[0,1,0,0],\"order\":1}", "actual C# identity names must match JS reader");
            });
            Need(passed == 16, "Complete synthetic test census was not executed");
            Console.WriteLine("PASS: 16/16 synthetic C# wire/metadata tests; no science execution or retained coefficient replay.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL synthetic C# wire/metadata test: " + exception);
            return 1;
        }
    }
}
