using Gu.Core;

namespace Gu.Validation;

/// <summary>
/// A single validation rule that can be executed to produce a ValidationRecord.
/// </summary>
public interface IValidationRule
{
    /// <summary>Unique rule identifier (e.g., "gauge-residual-norm").</summary>
    string RuleId { get; }

    /// <summary>Category (e.g., "parity", "convergence", "gauge", "structural").</summary>
    string Category { get; }

    /// <summary>
    /// Execute this validation rule and return a record.
    /// </summary>
    ValidationRecord Execute();
}
