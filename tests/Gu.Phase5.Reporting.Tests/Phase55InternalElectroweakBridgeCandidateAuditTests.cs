using System.Text.Json;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase55InternalElectroweakBridgeCandidateAuditTests
{
    [Fact]
    public void BridgeCandidateAuditRejectsCouplingProxyControl()
    {
        using var doc = LoadJson("internal_bridge_candidate_audit.json");
        var root = doc.RootElement;

        Assert.Equal("internal-bridge-candidate-rejected", root.GetProperty("terminalStatus").GetString());

        var control = root.GetProperty("candidateBridgeControls").EnumerateArray().Single();
        Assert.Equal("phase55-coupling-proxy-as-weak-coupling-control", control.GetProperty("bridgeCandidateId").GetString());
        Assert.Equal("rejected", control.GetProperty("status").GetString());
        Assert.False(control.GetProperty("computedFromTargetMasses").GetBoolean());

        Assert.Contains(
            control.GetProperty("rejectionReasons").EnumerateArray(),
            reason => reason.GetString()!.Contains("not normalized Standard Model weak couplings", StringComparison.Ordinal));
    }

    [Fact]
    public void RejectedControlRecordsProxyMassesAndScaleDisagreement()
    {
        using var doc = LoadJson("internal_bridge_candidate_audit.json");
        var control = doc.RootElement.GetProperty("candidateBridgeControls").EnumerateArray().Single();

        Assert.Equal(7.139609437859668, control.GetProperty("wProxyMassGeV").GetDouble(), precision: 12);
        Assert.Equal(8.20504516541525, control.GetProperty("zProxyMassGeV").GetDouble(), precision: 12);
        Assert.Equal(0.8701487065486907, control.GetProperty("proxyMassRatio").GetDouble(), precision: 12);
        Assert.True(control.GetProperty("relativeScaleSpread").GetDouble() < -0.01);
    }

    [Fact]
    public void AbsoluteProjectionRemainsBlockedWithoutValidatedBridge()
    {
        using var doc = LoadJson("internal_bridge_candidate_audit.json");
        var root = doc.RootElement;

        var required = root.GetProperty("requiredBridgeObservable");
        Assert.Equal("missing", required.GetProperty("status").GetString());
        Assert.Equal("validated", required.GetProperty("minimumPromotionStatus").GetString());

        var status = root.GetProperty("absoluteProjectionStatus");
        Assert.Equal("blocked-no-validated-internal-bridge", status.GetProperty("physical-w-boson-mass-gev").GetString());
        Assert.Equal("blocked-no-validated-internal-bridge", status.GetProperty("physical-z-boson-mass-gev").GetString());
    }

    [Fact]
    public void WeakCouplingContractRejectsProfileMagnitudesAsBridgeInputs()
    {
        using var doc = LoadJson("weak_coupling_normalization_contract.json");
        var root = doc.RootElement;

        var rejectedKinds = root.GetProperty("rejectedInputKinds").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("coupling-profile-mean-magnitude", rejectedKinds);
        Assert.Contains("target-fitted-W-or-Z-scale", rejectedKinds);

        var hardRejects = root.GetProperty("hardRejectConditions").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("bridge uses a coupling proxy statistic without a normalization convention", hardRejects);
    }

    private static JsonDocument LoadJson(string fileName)
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "studies",
            "phase55_internal_electroweak_bridge_candidate_audit_001",
            fileName);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GeometricUnity.slnx")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
