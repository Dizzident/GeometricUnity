using System.Text.Json.Serialization;

namespace Gu.Phase2.Recovery;

/// <summary>
/// The recovery DAG: a first-class graph representing the observed-sector recovery process
/// (Section 12.1). The mandatory pipeline order is:
///   Native -> Observation -> Extraction -> Interpretation
///
/// This is not ad hoc postprocessing — it is the mandatory typed path for all recovery.
/// No comparison campaign may consume outputs that have not passed through this graph.
/// </summary>
public sealed class RecoveryGraph
{
    /// <summary>All nodes in the recovery DAG.</summary>
    [JsonPropertyName("nodes")]
    public required IReadOnlyList<RecoveryNode> Nodes { get; init; }

    /// <summary>All edges in the recovery DAG.</summary>
    [JsonPropertyName("edges")]
    public required IReadOnlyList<RecoveryEdge> Edges { get; init; }

    /// <summary>
    /// Allowed edge transitions: Native->Observation, Observation->Extraction,
    /// Extraction->Interpretation. No level skipping permitted.
    /// </summary>
    private static readonly IReadOnlyDictionary<RecoveryNodeKind, RecoveryNodeKind> AllowedTransitions =
        new Dictionary<RecoveryNodeKind, RecoveryNodeKind>
        {
            [RecoveryNodeKind.Native] = RecoveryNodeKind.Observation,
            [RecoveryNodeKind.Observation] = RecoveryNodeKind.Extraction,
            [RecoveryNodeKind.Extraction] = RecoveryNodeKind.Interpretation,
        };

    /// <summary>
    /// Validate DAG structure. Returns true if valid.
    ///
    /// Rules:
    /// 1. No duplicate node IDs
    /// 2. All edges reference existing nodes
    /// 3. No edges skip levels (Native can't go directly to Interpretation)
    /// 4. Every terminal node (no outgoing edges) must be Interpretation kind
    /// 5. Every Interpretation node must have a complete upstream path (Native -> Obs -> Ext -> Interp)
    /// 6. No cycles (it's a DAG)
    /// 7. Every node must have a non-empty BranchProvenanceId
    /// </summary>
    public bool Validate(out IReadOnlyList<string> errors)
    {
        var errorList = new List<string>();

        var nodeMap = new Dictionary<string, RecoveryNode>();
        foreach (var node in Nodes)
        {
            if (!nodeMap.TryAdd(node.NodeId, node))
            {
                errorList.Add($"Duplicate node ID: '{node.NodeId}'.");
            }
        }

        // Rule 7: Every node must have a non-empty BranchProvenanceId.
        foreach (var node in Nodes)
        {
            if (string.IsNullOrWhiteSpace(node.BranchProvenanceId))
            {
                errorList.Add($"Node '{node.NodeId}' has empty BranchProvenanceId.");
            }
        }

        // Build adjacency lists.
        var outgoing = new Dictionary<string, List<string>>();
        var incoming = new Dictionary<string, List<string>>();
        foreach (var node in Nodes)
        {
            outgoing.TryAdd(node.NodeId, []);
            incoming.TryAdd(node.NodeId, []);
        }

        foreach (var edge in Edges)
        {
            if (!nodeMap.TryGetValue(edge.FromNodeId, out var fromNode))
            {
                errorList.Add($"Edge references unknown source node '{edge.FromNodeId}'.");
                continue;
            }
            if (!nodeMap.TryGetValue(edge.ToNodeId, out var toNode))
            {
                errorList.Add($"Edge references unknown target node '{edge.ToNodeId}'.");
                continue;
            }

            // Rule 3: No level skipping.
            if (!AllowedTransitions.TryGetValue(fromNode.Kind, out var expectedTarget) ||
                expectedTarget != toNode.Kind)
            {
                errorList.Add(
                    $"Invalid edge: {fromNode.Kind} ('{edge.FromNodeId}') -> " +
                    $"{toNode.Kind} ('{edge.ToNodeId}'). " +
                    $"Allowed: {fromNode.Kind} -> " +
                    $"{(AllowedTransitions.TryGetValue(fromNode.Kind, out var allowed) ? allowed : "none")}.");
            }

            outgoing[edge.FromNodeId].Add(edge.ToNodeId);
            incoming[edge.ToNodeId].Add(edge.FromNodeId);
        }

        // Rule 4: Every terminal node (no outgoing edges) must be Interpretation kind.
        foreach (var node in Nodes)
        {
            if (outgoing.TryGetValue(node.NodeId, out var outs) &&
                outs.Count == 0 &&
                node.Kind != RecoveryNodeKind.Interpretation)
            {
                errorList.Add(
                    $"Terminal node '{node.NodeId}' is {node.Kind}, must be Interpretation.");
            }
        }

        // Rule 5: Every Interpretation node must have a complete upstream path.
        foreach (var node in Nodes.Where(n => n.Kind == RecoveryNodeKind.Interpretation))
        {
            var upstreamKinds = CollectUpstreamKinds(node.NodeId, nodeMap, incoming);
            if (!upstreamKinds.Contains(RecoveryNodeKind.Native) ||
                !upstreamKinds.Contains(RecoveryNodeKind.Observation) ||
                !upstreamKinds.Contains(RecoveryNodeKind.Extraction))
            {
                errorList.Add(
                    $"Interpretation node '{node.NodeId}' missing upstream nodes. " +
                    $"Found kinds: [{string.Join(", ", upstreamKinds)}]. " +
                    $"Required: Native, Observation, Extraction.");
            }
        }

        // Rule 6: No cycles.
        if (HasCycle(nodeMap, outgoing))
        {
            errorList.Add("Recovery graph contains a cycle — must be a DAG.");
        }

        errors = errorList;
        return errorList.Count == 0;
    }

    private static HashSet<RecoveryNodeKind> CollectUpstreamKinds(
        string nodeId,
        Dictionary<string, RecoveryNode> nodeMap,
        Dictionary<string, List<string>> incoming)
    {
        var kinds = new HashSet<RecoveryNodeKind>();
        var visited = new HashSet<string>();
        var queue = new Queue<string>();

        if (incoming.TryGetValue(nodeId, out var parents))
        {
            foreach (var parent in parents)
                queue.Enqueue(parent);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current)) continue;

            if (nodeMap.TryGetValue(current, out var node))
            {
                kinds.Add(node.Kind);
                if (incoming.TryGetValue(current, out var currentParents))
                {
                    foreach (var parent in currentParents)
                        queue.Enqueue(parent);
                }
            }
        }

        return kinds;
    }

    private static bool HasCycle(
        Dictionary<string, RecoveryNode> nodeMap,
        Dictionary<string, List<string>> outgoing)
    {
        // Kahn's algorithm: topological sort via in-degree counting.
        var inDegree = new Dictionary<string, int>();
        foreach (var id in nodeMap.Keys)
            inDegree[id] = 0;

        foreach (var (_, targets) in outgoing)
        {
            foreach (var target in targets)
            {
                if (inDegree.ContainsKey(target))
                    inDegree[target]++;
            }
        }

        var queue = new Queue<string>();
        foreach (var (id, degree) in inDegree)
        {
            if (degree == 0) queue.Enqueue(id);
        }

        int processed = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            processed++;

            if (!outgoing.TryGetValue(current, out var targets)) continue;
            foreach (var target in targets)
            {
                if (!inDegree.ContainsKey(target)) continue;
                inDegree[target]--;
                if (inDegree[target] == 0) queue.Enqueue(target);
            }
        }

        return processed < nodeMap.Count;
    }
}
