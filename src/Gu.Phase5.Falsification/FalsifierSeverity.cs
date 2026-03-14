namespace Gu.Phase5.Falsification;

/// <summary>
/// String constants for falsifier severity levels (M50).
///
/// Demotion rules (architect-confirmed):
///   Fatal         → cap affected candidate at C0
///   High          → demote by 2 claim class levels
///   Medium        → demote by 1 claim class level
///   Low           → warning only (no demotion)
///   Informational → logged but no action
/// </summary>
public static class FalsifierSeverity
{
    /// <summary>Caps affected candidates at C0.</summary>
    public const string Fatal = "fatal";

    /// <summary>Demotes by 2 claim class levels.</summary>
    public const string High = "high";

    /// <summary>Demotes by 1 claim class level.</summary>
    public const string Medium = "medium";

    /// <summary>Warning only; no demotion applied.</summary>
    public const string Low = "low";

    /// <summary>Logged for record-keeping; no action.</summary>
    public const string Informational = "informational";
}
