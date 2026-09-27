// Distinct from the earlier numerical background seal. Final metadata remains
// producer-only until independent validation binds the accepted background and
// all350 same-point children. No file is rewritten or retried after failure.
internal sealed record MixedPointCompletionCheckpoint(string Context,MixedPointBackgroundCheckpoint Background,MixedEmittedArtifact Metadata);
internal sealed class MixedPointEvidenceSeal(string context,string graphPath,string backgroundPath,string metadataPath)
{
 bool busy,failed,returned;
 void Need(bool ok,string why){if(!ok){failed=true;throw new InvalidOperationException("A68 point completion seal: "+why);}}
 void Pin(MixedEmittedArtifact value,string expected)
 {Need(value is not null&&value.Path==expected&&value.Bytes>1&&value.Sha256 is {Length:64}&&value.Sha256.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f'),"exact emitted artifact identity");}
 public MixedPointCompletionCheckpoint Complete(MixedPointBackgroundCheckpoint background,
  Func<MixedPointBackgroundCheckpoint,MixedEmittedArtifact> writeMetadata,Action<MixedPointCompletionCheckpoint> validate)
 {
  Need(!failed&&!busy&&!returned,"failed, reentrant or completed seal");busy=true;
  try
  {
   Need(context is "point0" or "point1","exact baseline point");
   Need(new[]{graphPath,backgroundPath,metadataPath}.All(p=>!string.IsNullOrWhiteSpace(p))&&
    new[]{graphPath,backgroundPath,metadataPath}.Distinct(StringComparer.Ordinal).Count()==3,"three distinct declared artifact paths");
   Need(background is not null&&background.Context==context,"same-point background checkpoint");
   Pin(background!.Graph,graphPath);Pin(background.Metadata,backgroundPath);
   ArgumentNullException.ThrowIfNull(writeMetadata);ArgumentNullException.ThrowIfNull(validate);
   var metadata=writeMetadata(background);Need(!failed,"metadata writer reentry");Pin(metadata,metadataPath);
   var checkpoint=new MixedPointCompletionCheckpoint(context,background,metadata);
   validate(checkpoint);Need(!failed,"independent validator reentry");returned=true;return checkpoint;
  }
  catch{failed=true;throw;}finally{busy=false;}
 }
 public static object Envelope(string context,MixedEmittedArtifact backgroundCheckpoint,object metadata)=>new{
  schema="phase627-point-computational-completion-v1",context,status="producer-complete",independentValidationComplete=false,backgroundCheckpoint,metadata};
 public bool ValidationReturned=>returned&&!failed;
 public bool Failed=>failed;
}
