// A producer-complete computational checkpoint, NOT independent scientific
// acceptance. Preserve BOTH emitted artifacts before invoking the mandatory
// synchronous reader/replay prerequisite. Failure never rewrites or retries.
internal sealed record MixedContextComputationalCheckpoint(string Context,MixedEmittedArtifact Graph,MixedEmittedArtifact Metadata);
internal sealed class MixedContextEvidenceSeal(string context,string graphPath,string metadataPath)
{
 bool busy,failed,returned;
 void Need(bool ok,string why){if(!ok){failed=true;throw new InvalidOperationException("A68 context seal: "+why);}}
 void Pin(MixedEmittedArtifact value,string expectedPath)
 {
  Need(value is not null&&value.Path==expectedPath&&value.Bytes>1&&value.Sha256 is {Length:64}&&value.Sha256.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f'),"exact emitted artifact identity");
 }
 public MixedContextComputationalCheckpoint Complete(Func<MixedEmittedArtifact> writeGraph,
  Func<MixedEmittedArtifact,MixedEmittedArtifact> writeMetadata,Action<MixedContextComputationalCheckpoint> validate)
 {
  Need(!failed&&!busy&&!returned,"failed, reentrant or completed seal");busy=true;
  try
  {
   Need(!string.IsNullOrWhiteSpace(context)&&!string.IsNullOrWhiteSpace(graphPath)&&!string.IsNullOrWhiteSpace(metadataPath)&&graphPath!=metadataPath,"distinct predeclared context artifact paths");
   ArgumentNullException.ThrowIfNull(writeGraph);ArgumentNullException.ThrowIfNull(writeMetadata);ArgumentNullException.ThrowIfNull(validate);
   var graph=writeGraph();Need(!failed,"graph writer reentry");Pin(graph,graphPath);
   var metadata=writeMetadata(graph);Need(!failed,"metadata writer reentry");Pin(metadata,metadataPath);
   var checkpoint=new MixedContextComputationalCheckpoint(context,graph,metadata);
   validate(checkpoint);Need(!failed,"independent validator reentry");returned=true;return checkpoint;
  }
  catch{failed=true;throw;}finally{busy=false;}
 }
 // Same envelope is used for prospective sizing and actual emission. Graph
 // pin and explicit false validation flag are immutable recorded claims; a
 // successful prerequisite does NOT rewrite this file to claim validation.
 public static object Envelope(string context,MixedEmittedArtifact graph,object metadata)=>new{
  schema="phase627-context-computational-evidence-v1",context,status="producer-complete",independentValidationComplete=false,graph,metadata};
 public bool ValidationReturned=>returned&&!failed;
 public bool Failed=>failed;
}
