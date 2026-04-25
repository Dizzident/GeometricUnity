namespace Gu.Phase5.QuantitativeValidation;

public static class PhysicalModeEvidenceValidator
{
    public static IReadOnlyList<string> ValidateForPrediction(
        IdentifiedPhysicalModeRecord mode,
        IReadOnlyList<ModeIdentificationEvidenceRecord>? evidenceRecords)
    {
        ArgumentNullException.ThrowIfNull(mode);

        var errors = new List<string>();
        if (!string.Equals(mode.Status, "validated", StringComparison.OrdinalIgnoreCase))
            errors.Add($"mode '{mode.ModeId}' status is '{mode.Status}', not 'validated'.");
        if (mode.Uncertainty < 0)
            errors.Add($"mode '{mode.ModeId}' total uncertainty is unestimated.");

        var evidence = evidenceRecords?.Where(e =>
            string.Equals(e.ModeId, mode.ModeId, StringComparison.Ordinal)).ToList() ?? [];
        if (evidence.Count == 0)
        {
            errors.Add($"mode '{mode.ModeId}' has no mode-identification evidence record.");
            return errors;
        }

        var validated = evidence.FirstOrDefault(e =>
            string.Equals(e.Status, "validated", StringComparison.OrdinalIgnoreCase));
        if (validated is null)
        {
            var status = evidence.First().Status;
            errors.Add($"mode '{mode.ModeId}' has only {status} mode-identification evidence.");
            return errors;
        }

        if (!string.Equals(validated.ParticleId, mode.ParticleId, StringComparison.Ordinal))
            errors.Add($"mode '{mode.ModeId}' evidence particleId '{validated.ParticleId}' does not match mode particleId '{mode.ParticleId}'.");
        if (!string.Equals(validated.ModeKind, mode.ModeKind, StringComparison.Ordinal))
            errors.Add($"mode '{mode.ModeId}' evidence modeKind '{validated.ModeKind}' does not match mode modeKind '{mode.ModeKind}'.");
        if (!string.Equals(validated.EnvironmentId, mode.EnvironmentId, StringComparison.Ordinal))
            errors.Add($"mode '{mode.ModeId}' evidence environmentId '{validated.EnvironmentId}' does not match mode environmentId '{mode.EnvironmentId}'.");
        if (!string.Equals(validated.BranchId, mode.BranchId, StringComparison.Ordinal))
            errors.Add($"mode '{mode.ModeId}' evidence branchId '{validated.BranchId}' does not match mode branchId '{mode.BranchId}'.");
        if (!string.Equals(validated.RefinementLevel, mode.RefinementLevel, StringComparison.Ordinal))
            errors.Add($"mode '{mode.ModeId}' evidence refinementLevel does not match mode refinementLevel.");
        if (validated.SourceObservableIds.Count == 0)
            errors.Add($"mode '{mode.ModeId}' validated evidence must declare sourceObservableIds.");

        return errors;
    }

    public static void ThrowIfInvalidForPrediction(
        IdentifiedPhysicalModeRecord mode,
        IReadOnlyList<ModeIdentificationEvidenceRecord>? evidenceRecords)
    {
        var errors = ValidateForPrediction(mode, evidenceRecords);
        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors));
    }
}
