namespace Gu.Phase2.Canonicity;

/// <summary>
/// Status of a canonicity docket.
/// </summary>
public enum DocketStatus
{
    Open,
    EvidenceAccumulating,
    ClosedByTheorem,
    ClosedByClassification,
    Falsified,
}
