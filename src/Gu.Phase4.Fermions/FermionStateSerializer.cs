using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase4.Fermions;

/// <summary>
/// JSON serialization helpers for fermionic state types.
/// All types use System.Text.Json with camelCase-ish (JsonPropertyName) conventions.
/// </summary>
public static class FermionStateSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Serialize a FermionFieldLayout to JSON.</summary>
    public static string ToJson(FermionFieldLayout layout, bool indented = true) =>
        JsonSerializer.Serialize(layout, indented ? Options : CompactOptions);

    /// <summary>Deserialize a FermionFieldLayout from JSON.</summary>
    public static FermionFieldLayout LayoutFromJson(string json) =>
        JsonSerializer.Deserialize<FermionFieldLayout>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize FermionFieldLayout.");

    /// <summary>Serialize a DiscreteFermionState to JSON.</summary>
    public static string ToJson(DiscreteFermionState state, bool indented = true) =>
        JsonSerializer.Serialize(state, indented ? Options : CompactOptions);

    /// <summary>Deserialize a DiscreteFermionState from JSON.</summary>
    public static DiscreteFermionState FermionStateFromJson(string json) =>
        JsonSerializer.Deserialize<DiscreteFermionState>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize DiscreteFermionState.");

    /// <summary>Serialize a DiscreteDualFermionState to JSON.</summary>
    public static string ToJson(DiscreteDualFermionState state, bool indented = true) =>
        JsonSerializer.Serialize(state, indented ? Options : CompactOptions);

    /// <summary>Deserialize a DiscreteDualFermionState from JSON.</summary>
    public static DiscreteDualFermionState DualStateFromJson(string json) =>
        JsonSerializer.Deserialize<DiscreteDualFermionState>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize DiscreteDualFermionState.");

    /// <summary>Serialize a FermionModeRecord to JSON.</summary>
    public static string ToJson(FermionModeRecord mode, bool indented = true) =>
        JsonSerializer.Serialize(mode, indented ? Options : CompactOptions);

    /// <summary>Deserialize a FermionModeRecord from JSON.</summary>
    public static FermionModeRecord ModeRecordFromJson(string json) =>
        JsonSerializer.Deserialize<FermionModeRecord>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize FermionModeRecord.");

    /// <summary>Serialize a FermionModeFamily to JSON.</summary>
    public static string ToJson(FermionModeFamily family, bool indented = true) =>
        JsonSerializer.Serialize(family, indented ? Options : CompactOptions);

    /// <summary>Deserialize a FermionModeFamily from JSON.</summary>
    public static FermionModeFamily ModeFamilyFromJson(string json) =>
        JsonSerializer.Deserialize<FermionModeFamily>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize FermionModeFamily.");

    /// <summary>Serialize a FermionBackgroundRecord to JSON.</summary>
    public static string ToJson(FermionBackgroundRecord bg, bool indented = true) =>
        JsonSerializer.Serialize(bg, indented ? Options : CompactOptions);

    /// <summary>Deserialize a FermionBackgroundRecord from JSON.</summary>
    public static FermionBackgroundRecord BackgroundRecordFromJson(string json) =>
        JsonSerializer.Deserialize<FermionBackgroundRecord>(json, Options)
        ?? throw new InvalidOperationException("Failed to deserialize FermionBackgroundRecord.");
}
