using System.Collections.ObjectModel;
using System.Text.Json;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal sealed record MixedSinkMark(string Name,int Degree,bool Expanded,string? RelativePath);
internal sealed record MixedSinkResources(MixedTrace.Limits Trace,long FileBytes,long ContextBytes,long MetadataBytes,long FailureGraphBytes);
internal sealed record MixedPointCheckpointPlan(string RelativePath,long Bytes);
// Immutable references to exact emitted bytes, not mutable Graph/metadata
// arrays. The independent reader must open/read/hash the same pinned bytes.
internal sealed record MixedEmittedArtifact(string Path,long Bytes,string Sha256);
internal sealed record MixedPointBackgroundCheckpoint(string Context,MixedEmittedArtifact Graph,MixedEmittedArtifact Metadata);
internal sealed record MixedCaptureInspection(string Id,long Copies);
// Immutable run-wide declaration. Matching this wire proves agreement only,
// never source-plan authenticity or sufficient production memory.
internal sealed class MixedCaptureDeclaration
{
 public MixedTrace.CaptureLimits Limits{get;}
 public IReadOnlyList<MixedCaptureInspection> InspectionPolicy{get;}
 public MixedCaptureDeclaration(MixedTrace.CaptureLimits limits,MixedCaptureInspection[] inspectionPolicy)
 {
  ArgumentNullException.ThrowIfNull(limits);ArgumentNullException.ThrowIfNull(inspectionPolicy);
  static void Need(bool ok){if(!ok)throw new InvalidOperationException("A68 capture declaration: explicit safe limits and ordered705 policy required");}
  foreach(var c in new[]{limits.Normal,limits.Inspection,limits.Failure})
   foreach(long n in new[]{c.Graphs,c.NodeObjects,c.Arrays,c.Slots,c.ElementCopies})Need(n>=0&&n<=9007199254740991);
  var ids=MixedAuditPlan.RunContextIds();Need(inspectionPolicy.Length==ids.Length);
  var copy=new MixedCaptureInspection[ids.Length];
  for(int i=0;i<ids.Length;i++)
  {var row=inspectionPolicy[i];Need(row is not null&&row.Id==ids[i]&&row.Copies>=0&&row.Copies<=9007199254740991);copy[i]=row!;}
  Limits=limits;InspectionPolicy=Array.AsReadOnly(copy);
 }
 public bool Matches(MixedCaptureDeclaration other)=>other is not null&&Limits==other.Limits&&InspectionPolicy.SequenceEqual(other.InspectionPolicy);
 public long InspectionCopiesFor(string id)
 {foreach(var row in InspectionPolicy)if(row.Id==id)return row.Copies;throw new InvalidOperationException("A68 capture declaration: unknown context");}
}

// Every collection is copied before use. A caller must supply this prospective
// plan from reviewed source/provenance recipes, never from a recorded graph.
internal sealed class MixedSinkContextPlan
{
 public string Id{get;}
 public string GraphPath{get;}
 public string MetadataPath{get;}
 public MixedSinkResources Resources{get;}
 public MixedPointCheckpointPlan? PointCheckpoint{get;}
 public IReadOnlyList<MixedTrace.LeafSpec> Leaves{get;}
 public IReadOnlyDictionary<string,string> LeafRoles{get;}
 public IReadOnlyList<MixedSinkMark> Marks{get;}
 readonly string callbackId;readonly MixedPlannedCallback[] callbackEntries;
 public MixedContextPlan Callbacks=>new(callbackId,callbackEntries.ToArray());
 public MixedSinkContextPlan(string id,string graphPath,string metadataPath,MixedSinkResources resources,
  IEnumerable<MixedTrace.LeafSpec> leaves,IReadOnlyDictionary<string,string> leafRoles,
  IEnumerable<MixedSinkMark> marks,MixedContextPlan callbacks,MixedPointCheckpointPlan? pointCheckpoint=null)
 {
  Id=id;GraphPath=graphPath;MetadataPath=metadataPath;Resources=resources;PointCheckpoint=pointCheckpoint;
  Leaves=Array.AsReadOnly(leaves.ToArray());LeafRoles=new ReadOnlyDictionary<string,string>(new Dictionary<string,string>(leafRoles,StringComparer.Ordinal));
  Marks=Array.AsReadOnly(marks.ToArray());callbackId=callbacks.Id;callbackEntries=callbacks.Callbacks.ToArray();
 }
}

internal interface IMixedAuditSinkPrerequisites
{
 // Require the accepted preflight handshake to bind THIS actual sink plan's
 // declaration, not merely an externally supplied profile hash.
 void ValidateCaptureDeclaration(MixedCaptureDeclaration declaration);
 // All methods are mandatory. Shape-only/no-op implementations are not an
 // authorization to execute: MAIN must approve the independently frozen
 // provenance/geometry/expression implementation as part of the full pack.
 void ValidateContextPlan(MixedSinkContextPlan plan);
 // Structure has NO placeholder coefficient hashes. Validate every frozen
 // menu/path/retention/resource/source declaration before science begins.
 void ValidateContextTemplate(MixedSinkContextTemplate template);
 // Mandatory terminal acknowledgement of the full independently checked705
 // set. Per-context acceptance alone is not full preflight completion.
 void CompleteTemplateValidation();
 // Mandatory before EACH metadata factory call, including the full prepass.
 void CheckTemplateConstructionResources(string context);
 // Resolve hashes ONLY from independently authenticated source reconstruction
 // and accepted point replay exports, never from observed producer tensors.
 IReadOnlyList<MixedTrace.LeafSpec> BindContextLeaves(MixedSinkContextTemplate template);
 void ValidateDiagnosticMenu(string diagnostic,DiagnosticMenu actualMenu);
 void ValidateLeaf(string context,string role,MixedTrace.LeafSpec declared,FT actual);
 void ValidateBackground(string context,string name,MixedBackground actual);
 void ValidateGeometry(string context,string name,MixedBackground baseline,MixedMetricGerm actual);
 void ValidateParameter(string context,string operation,JsonElement parameter);
 // Must reconstruct the intended high-level recipes / input topology; a
 // valid graph hash, allowed constants and generic DAG replay do not suffice.
 // Read/hash these exact emitted graph AND metadata bytes. Require complete
 // independent geometry, scalar/check/error replay and retained-file checks.
 // Producer-complete metadata is explicitly NOT independent acceptance.
 void ValidateContextEvidence(MixedContextComputationalCheckpoint checkpoint);
 // Mandatory BEFORE the first child germ. Independently read pinned graph and
 // background-sealed metadata, compare ALL source geometry/checks/domains,
 // reconstruct the full point recipe and accept its27-export numerical
 // receipt. Returning must mean that source adapter accepted that receipt,
 // not that file hashes matched. Point traversal is still incomplete.
 void ValidatePointBackground(MixedPointBackgroundCheckpoint checkpoint);
 // Distinct terminal check AFTER all350 child germs: read/hash final metadata,
 // match the previously independently accepted background and unchanged metadata,
 // and require all same-point child numerical replays in the independent ledger.
 // The producer's completion flag or background replay alone is insufficient.
 void ValidatePointCompletion(MixedPointCompletionCheckpoint checkpoint);
 // Covers runtime operator/matrix/dual/height/memory checks not provided by
 // the recorder's own guards. Called at every sink boundary and prewrite.
 void CheckResources(string context);
}

internal sealed class MixedAuditSinkPlan
{
 public string OutputRoot{get;}
 public string FailurePath{get;}
 public long AggregateBytes{get;}
 public long FailureBytes{get;}
 public long FailureGraphBytes{get;}
 public MixedCaptureDeclaration Capture{get;}
 public MixedTrace.CaptureLimits CaptureLimits=>Capture.Limits;
 public MixedContextTemplateCatalog Templates{get;}
 public IReadOnlyList<string> ContextIds=>Templates.ContextIds;
 public MixedAuditSinkPlan(string outputRoot,string failurePath,long aggregateBytes,long failureBytes,long failureGraphBytes,
  Func<string,MixedSinkContextTemplate> templateFactory,MixedTemplateResources templateResources,MixedCaptureDeclaration capture)
 {
  OutputRoot=Path.GetFullPath(outputRoot);FailurePath=failurePath;AggregateBytes=aggregateBytes;FailureBytes=failureBytes;FailureGraphBytes=failureGraphBytes;
  Capture=capture??throw new ArgumentNullException(nameof(capture));
  Templates=new(templateFactory,templateResources);
 }
}
