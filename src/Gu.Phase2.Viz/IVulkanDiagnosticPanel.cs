using Gu.VulkanViewer;

namespace Gu.Phase2.Viz;

/// <summary>
/// Interface for Phase II diagnostic visualization panels.
/// Each panel produces one or more <see cref="ViewPayload"/> objects
/// from Phase II analysis data for rendering by the Vulkan viewer.
/// </summary>
public interface IVulkanDiagnosticPanel
{
    /// <summary>Unique identifier for this panel type.</summary>
    string PanelId { get; }

    /// <summary>Human-readable panel title.</summary>
    string Title { get; }
}
