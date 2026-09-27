using System.Security.Cryptography;
using System.Text.Json;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Uncalled complete writer. This class cannot invent its prerequisite plan,
// leaves, constants, retention decisions, resource ceilings or FIRST approval.
// All writes are CreateNew; no file, including failed/partial evidence, is
// deleted, replaced or retried under another path after a failure.
internal sealed class MixedAuditSink:IMixedAuditEvidence,IMixedDiagnosticEvidence,IDisposable
{
 sealed class Context(MixedSinkContextPlan plan)
 {
  public MixedSinkContextPlan Plan{get;}=plan;
  public MixedCallbackCensus Census{get;}=new(plan.Callbacks);
  public MixedTrace.Session Session{get;set;}=null!;
  public Dictionary<string,object> Metadata{get;}=new(StringComparer.Ordinal);
  public HashSet<string> Marks{get;}=new(StringComparer.Ordinal);
  public HashSet<string> LeafRoles{get;}=new(StringComparer.Ordinal);
  public Dictionary<string,MixedSinkMark> MarkMenu{get;}=plan.Marks.ToDictionary(m=>m.Name,StringComparer.Ordinal);
  public long Bytes;
  public long MetadataBytes;
  public MixedPointBackgroundCheckpoint? BackgroundCheckpoint;
  public MixedContextEvidenceSeal EvidenceSeal{get;}=new(plan.Id,plan.GraphPath,plan.MetadataPath);
  public MixedPointEvidenceSeal? PointSeal{get;}=plan.PointCheckpoint is { } checkpoint?new(plan.Id,plan.GraphPath,checkpoint.RelativePath,plan.MetadataPath):null;
 }
 readonly MixedAuditSinkPlan plan;readonly IMixedAuditSinkPrerequisites prerequisites;
 readonly MixedTrace.Session.SinkOwnership traceOwner;
 readonly MixedAuditRunCensus run=new();readonly HashSet<string> opened=new(StringComparer.Ordinal),completedContexts=new(StringComparer.Ordinal),startedContexts=new(StringComparer.Ordinal);
 readonly List<MixedEmittedArtifact> completedFiles=[];Context? active,pointContext;long aggregateBytes,failureGraphBytes;bool failed,finished,disposed,busy,reentered;int pointIndex=-1;
 public MixedAuditSink(MixedAuditSinkPlan plan,IMixedAuditSinkPrerequisites prerequisites)
 {
  this.plan=plan;this.prerequisites=prerequisites??throw new ArgumentNullException(nameof(prerequisites));
  traceOwner=MixedTrace.Session.AcquireSinkOwnership(plan.CaptureLimits);
  try
  {
  traceOwner.Callback(()=>prerequisites.ValidateCaptureDeclaration(plan.Capture));
  Need(plan.AggregateBytes>0&&plan.FailureBytes>0&&plan.FailureGraphBytes>0,"positive explicitly frozen normal/failure byte reservations");Need(MixedTrace.Active is null,"exclusive trace ownership");
  MixedAuditPlan.ValidateRunContextIds(plan.ContextIds);var paths=new HashSet<string>(StringComparer.Ordinal);AddPath(plan.FailurePath);
  // Fence the complete catalog operation, including its caller-supplied
  // factory and the returned leaf collection's Count/enumerator accessors.
  traceOwner.Callback(()=>plan.Templates.Freeze(prerequisites.CheckTemplateConstructionResources,context=>
  {
   prerequisites.ValidateContextTemplate(context);Need(context.Resources.FileBytes>0&&context.Resources.ContextBytes>0&&context.Resources.MetadataBytes>0&&context.Resources.FailureGraphBytes>0,"positive context/failure budgets");
   Need(context.Callbacks.Id==context.Id,"context callback ID");AddPath(context.GraphPath);AddPath(context.MetadataPath);
   bool point=context.Id is "point0" or "point1";
   Need(point==(context.PointCheckpoint is not null),"dedicated checkpoint plan iff baseline point");
   if(context.PointCheckpoint is { } checkpoint)
   {
    Need(checkpoint.Bytes>0&&checkpoint.Bytes<=context.Resources.FileBytes&&checkpoint.Bytes<=context.Resources.ContextBytes&&checkpoint.Bytes<=plan.AggregateBytes,"explicit checkpoint file/context/aggregate admission");
    AddPath(checkpoint.RelativePath);
   }
   Need(context.Marks.Select(m=>m.Name).Distinct(StringComparer.Ordinal).Count()==context.Marks.Count,"distinct planned marks");
   foreach(var mark in context.Marks){Need(mark.Degree is >=0 and <=14,"mark degree");Need(mark.Expanded==(mark.RelativePath is not null),"expanded path iff expanded mark");if(mark.RelativePath is not null)AddPath(mark.RelativePath);}
  }));
  foreach(string outputPath in paths)for(string? parent=Path.GetDirectoryName(outputPath);parent is not null;parent=Path.GetDirectoryName(parent))Need(!paths.Contains(parent),"a planned output file cannot be another output's directory");
  traceOwner.Callback(prerequisites.CompleteTemplateValidation);
  var preflight=plan.Templates.Snapshot();Need(preflight.Frozen&&!preflight.Failed&&preflight.BoundContexts==0,"terminal template validation must preserve the healthy unbound catalog");
  void AddPath(string relative){string path=PathFor(relative);Need(paths.Add(path),"globally unique fixed output path");Need(!File.Exists(path)&&!Directory.Exists(path),"pre-existing output is never overwritten");}
  traceOwner.CheckHealthy();
  }
  catch{traceOwner.Dispose();throw;}
 }
 static void Need(bool condition,string message){if(!condition)throw new InvalidOperationException("A68 sink: "+message);}
 string PathFor(string relative)
 {
  Need(!string.IsNullOrWhiteSpace(relative)&&!Path.IsPathRooted(relative)&&!relative.Contains('\\'),"relative portable output path");
  string[] parts=relative.Split('/');Need(parts.All(p=>p.Length>0&&p is not ("." or "..")&&p.All(c=>char.IsAsciiLetterOrDigit(c)||c is '_' or '-' or '.')),"closed path component alphabet");
  string root=Path.GetFullPath(plan.OutputRoot);Need(root!=Path.GetPathRoot(root),"output root cannot be filesystem root");
  string result=Path.GetFullPath(Path.Combine(root,Path.Combine(parts)));Need(result.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.Ordinal),"path remains inside output root");
  for(string? directory=Path.GetDirectoryName(result);directory is not null;directory=Path.GetDirectoryName(directory))
   if(Directory.Exists(directory))Need((File.GetAttributes(directory)&FileAttributes.ReparsePoint)==0,"symbolic-link/reparse output ancestor forbidden");
  return result;
 }
 void Guard(Action action)
 {
  traceOwner.RequireControl();
  if(busy){reentered=true;throw new InvalidOperationException("A68 sink: reentrant callback");}
  Need(!failed&&!finished&&!disposed,"failed/closed sink");
  busy=true;
  try
  {
   traceOwner.CheckHealthy();CheckCatalog();Need(ReferenceEquals(MixedTrace.Active,active?.Session),"exclusive active trace ownership");
   if(active is not null)traceOwner.Callback(()=>prerequisites.CheckResources(active.Plan.Id));action();
   traceOwner.CheckHealthy();Need(!reentered,"swallowed reentrant callback failure");CheckCatalog();
  }
  catch(Exception exception)
  {try{Fail(exception);}catch(Exception evidenceFailure){throw new AggregateException("Scientific/writer failure and failure-evidence preservation failure",exception,evidenceFailure);}throw;}
  finally{busy=false;}
 }
 void CheckCatalog()
 {
  var state=plan.Templates.Snapshot();
  Need(state.Frozen&&!state.Failed&&state.BoundContexts==startedContexts.Count,"healthy frozen catalog with exactly the sink-started contexts");
 }
 Context Current()=>active??throw new InvalidOperationException("no active evidence context");
 void Open(string id)
 {
  Need(startedContexts.Add(id),"context cannot restart");
  var boundPlan=traceOwner.Callback(()=>plan.Templates.Materialize(id,prerequisites.CheckTemplateConstructionResources,prerequisites.BindContextLeaves));
  traceOwner.Callback(()=>prerequisites.ValidateContextPlan(boundPlan));Need(!reentered&&!failed,"context plan validator must complete without reentry");
  var context=new Context(boundPlan);active=context;
  context.Session=traceOwner.CreateSession(plan.Capture.InspectionCopiesFor(id),context.Plan.Leaves.ToArray(),context.Plan.Resources.Trace,(name,degree,value)=>WriteExpanded(context,name,degree,value),(op,p)=>traceOwner.Callback(()=>prerequisites.ValidateParameter(id,op,p)));traceOwner.Activate(context.Session);
  using var counter=new GuardedStream(Stream.Null,context.Plan.Resources.MetadataBytes,_=>{});
  if(context.Plan.PointCheckpoint is { } checkpoint)
  {
   // Sizing only, never emitted or treated as evidence: longest positive
   // Int64 and64 ASCII hash characters reserve the final reference BEFORE
   // metadata accumulation. Actual checkpoint bytes/hash are supplied later.
   var referenceEnvelope=new MixedEmittedArtifact(checkpoint.RelativePath,long.MaxValue,new string('f',64));
   JsonSerializer.Serialize(counter,MixedPointEvidenceSeal.Envelope(id,referenceEnvelope,new Dictionary<string,object>()),MixedTrace.JsonOptions);
  }
  else JsonSerializer.Serialize(counter,MixedContextEvidenceSeal.Envelope(id,
   new MixedEmittedArtifact(context.Plan.GraphPath,long.MaxValue,new string('f',64)),new Dictionary<string,object>()),MixedTrace.JsonOptions);
  counter.WriteByte(10);context.MetadataBytes=counter.Length;
 }
 void Import(string role,int degree,FT value)
 {
  var context=Current();Need(context.BackgroundCheckpoint is null,"sealed point cannot import another leaf");Need(context.LeafRoles.Add(role)&&context.Plan.LeafRoles.TryGetValue(role,out _),"declared unique leaf role");string id=context.Plan.LeafRoles[role];var spec=context.Plan.Leaves.Single(l=>l.Id==id);
  Need(spec.Degree==degree,"leaf role degree");traceOwner.Callback(()=>prerequisites.ValidateLeaf(context.Plan.Id,role,spec,value));context.Session.RegisterLeaf(id,degree,value);
 }
 void Mark(string name,int degree,FT value,bool? requiredExpanded=null)
 {
  var context=Current();Need(context.BackgroundCheckpoint is null,"sealed point cannot append tensor marks");Need(context.MarkMenu.TryGetValue(name,out var expected)&&expected.Degree==degree,"exact frozen mark name/degree");
  Need(requiredExpanded is null||requiredExpanded==expected!.Expanded,"callback expansion cannot be weakened");Need(context.Marks.Add(name),"unique mark");context.Session.Mark(name,degree,value,expected!.Expanded);
 }
 void Metadata(string name,object value)
 {
  var context=Current();Need(context.BackgroundCheckpoint is null,"sealed point metadata is immutable");Need(!context.Metadata.ContainsKey(name),"unique metadata key");
  // Admission bounds retained JSON-content volume before dictionary insertion.
  // KeyValuePair's own key/value labels and braces conservatively exceed one
  // dictionary property's syntax; +1 also charges its possible comma. The
  // already-constructed input object and serializer scratch need their separate
  // mandatory worst-case memory proof, not an RSS claim from this counter.
  using var counter=new GuardedStream(Stream.Null,context.Plan.Resources.MetadataBytes-context.MetadataBytes,_=>{});
  JsonSerializer.Serialize(counter,new KeyValuePair<string,object>(name,value),MixedTrace.JsonOptions);counter.WriteByte(10);
  context.MetadataBytes=checked(context.MetadataBytes+counter.Length);context.Metadata.Add(name,value);
 }
 string Rat(Rational value)
 {
  int ceiling=Current().Plan.Resources.Trace.RationalCharacters;
  Need(value.Denominator.Sign>0&&System.Numerics.BigInteger.Abs(value.Numerator).GetBitLength()<=4L*ceiling&&value.Denominator.GetBitLength()<=4L*ceiling,"bounded canonical metadata rational operands");
  string result=value.ToString();Need(result.Length<=ceiling,"metadata rational character ceiling");return result;
 }
 string[][] MatrixText(Matrix matrix)
 {Need(matrix.N==14,"complete fourteen-dimensional geometry matrix");return Enumerable.Range(0,14).Select(i=>Enumerable.Range(0,14).Select(j=>Rat(matrix[i,j])).ToArray()).ToArray();}
 static Dictionary<string,FT> Baseline(MixedBackground b)
 {
  var values=new Dictionary<string,FT>(StringComparer.Ordinal){{"X",b.X},{"B",b.B},{"F",b.F},{"DX",b.DX},{"Q",b.Q},{"AdjointX",b.AdjointX},{"NativeExterior",b.NativeExterior}};
  for(int p=0;p<3;p++)values.Add($"KInputs[{p}]",b.KInputs[p]);for(int p=0;p<4;p++)values.Add($"GradientPieces[{p}]",b.GradientPieces[p]);for(int z=0;z<14;z++)values.Add($"CovariantFrame[{z}]",b.CovariantFrame[z]);return values;
 }
 void ImportBaseline(MixedBackground b)
 {
  var values=Baseline(b);MixedAuditPlan.ValidateBaselineAliases(values.ToDictionary(q=>q.Key,q=>(object)q.Value,StringComparer.Ordinal));
  foreach(var role in MixedAuditPlan.BaselineRoles())
  {
   string key="baseline/"+role.Role,canonical="baseline/"+role.CanonicalId;var context=Current();
   Need(context.Plan.LeafRoles.TryGetValue(key,out var actual)&&context.Plan.LeafRoles.TryGetValue(canonical,out var expected)&&actual==expected,"explicit baseline alias-to-leaf binding");
   if(key==canonical)Import(key,role.Degree,values[role.Role]);
  }
 }
 public void BeginPoint(int point,FT certifiedPolynomial)=>Guard(()=>
 {
  Need(active is null&&pointContext is null,"point context state");run.BeginPoint(point);pointIndex=point;Open("point"+point);pointContext=active;Current().Census.Accept("BeginPoint","$");Import("input/X",1,certifiedPolynomial);
 });
 public void Background(MixedBackground background)=>Guard(()=>
 {run.Current.Accept("Background","$");Current().Census.Accept("Background","$");SaveBackground("background",background);});
 public void SealPointBackground()=>Guard(()=>
 {
  Need(active==pointContext&&pointContext is not null,"seal requires active baseline point");var context=Current();
  Need(context.BackgroundCheckpoint is null&&context.Plan.PointCheckpoint is not null,"one declared point checkpoint");
  run.SealPointBackground(()=>
  {
   context.Census.OnlyRemaining("SealPointBackground","Germ","EndPoint");
   Need(context.Marks.SetEquals(context.MarkMenu.Keys),"all background marks precede seal");
   var graph=context.Session.Finish();
   var graphArtifact=Write(context,context.Plan.GraphPath,context.Plan.Resources.Trace.GraphBytes,s=>MixedTrace.WriteGraph(s,graph,context.Plan.Resources.Trace));
   // Write immutable observed metadata before independent validation. A later
   // failure preserves these exact bytes; this status NEVER means the point
   // or its350 child contexts finished. No arrays are passed to the validator.
   var metadataArtifact=WriteJson(context,context.Plan.PointCheckpoint!.RelativePath,
    new{schema="phase627-point-background-checkpoint-v1",context=context.Plan.Id,status="background-sealed",pointTraversalComplete=false,graph=graphArtifact,metadata=context.Metadata},context.Plan.PointCheckpoint.Bytes);
   var checkpoint=new MixedPointBackgroundCheckpoint(context.Plan.Id,graphArtifact,metadataArtifact);
   traceOwner.Callback(()=>prerequisites.ValidatePointBackground(checkpoint));
   Need(!reentered&&!failed,"background validator must complete without reentry or failure");
   context.Census.Accept("SealPointBackground","$");context.BackgroundCheckpoint=checkpoint;
   // Leave the FINISHED recorder active as a tripwire: any attempted point
   // tensor operation before/after a child fails instead of going unrecorded.
  });
 });
 public void BeginGerm(MixedBackground background,int metricBasis,int jetIndex)=>Guard(()=>
 {
  Need(active==pointContext&&pointContext?.BackgroundCheckpoint is not null,"germ requires verified sealed baseline point");run.BeginGerm(metricBasis,jetIndex);Open($"point{pointIndex}/m{metricBasis}_j{jetIndex}");Current().Census.Accept("BeginGerm","$");ImportBaseline(background);
 });
 public void Geometry(MixedBackground background,MixedMetricGerm germ)=>Guard(()=>
 {run.Current.Accept("Geometry","$");Current().Census.Accept("Geometry","$");SaveGeometry("geometry",background,germ);});
 public void GeometryLeaf(string name,int degree,FT value)=>Guard(()=>
 {
  if(!Current().Plan.Id.StartsWith("diagnostic/",StringComparison.Ordinal))run.Current.Accept("GeometryLeaf",name,degree);
  Current().Census.Accept("GeometryLeaf",name,degree);Import("geometry/"+name,degree,value);
 });
 public void Tensor(string name,int degree,FT value,bool expanded)=>Guard(()=>
 {
  if(!Current().Plan.Id.StartsWith("diagnostic/",StringComparison.Ordinal))run.Current.Accept("Tensor",name,degree,expanded);
  Current().Census.Accept("Tensor",name,degree,expanded);Mark("tensor/"+name,degree,value,expanded);
 });
 public void Check(string name,bool passed)=>Guard(()=>
 {
  Metadata("check/"+name,passed);if(!Current().Plan.Id.StartsWith("diagnostic/",StringComparison.Ordinal))run.Current.Check(name,passed);Current().Census.Check(name,passed);
 });
 public void Error(MixedError error)=>Guard(()=>
 {
  run.Current.Accept("Error","$");Current().Census.Accept("Error","$");Need(error.NativeFirstJetNorms.Length==14,"fourteen first-jet error norms");
  Metadata("error",new{radius=Rat(error.Radius),fieldError=Rat(error.FieldError),motionNorm=Rat(error.MotionNorm),motionDerivativeSum=Rat(error.MotionDerivativeSum),referenceNorm=Rat(error.ReferenceNorm),referenceVariationNorm=Rat(error.ReferenceVariationNorm),raw0Majorant=Rat(error.Raw0Majorant),raw2Majorant=Rat(error.Raw2Majorant),eulerMajorant=Rat(error.EulerMajorant),raw0Error=Rat(error.Raw0Error),raw2Error=Rat(error.Raw2Error),eulerError=Rat(error.EulerError),nativeOneFormDualNorm=Rat(error.NativeOneFormDualNorm),nativeTwoFormDualNorm=Rat(error.NativeTwoFormDualNorm),nativeRaw0Error=Rat(error.NativeRaw0Error),nativeRaw2Error=Rat(error.NativeRaw2Error),nativeEulerError=Rat(error.NativeEulerError),nativeFieldNorm=Rat(error.NativeFieldNorm),nativeFirstJetNorms=error.NativeFirstJetNorms.Select(Rat).ToArray()});
 });
 public void EndGerm()=>Guard(()=>
 {
  var context=Current();context.Census.Accept("EndGerm","$");run.EndGerm();Close(context);pointContext!.Census.Accept("Germ",context.Plan.Id[(context.Plan.Id.LastIndexOf('/')+1)..]);active=pointContext;traceOwner.Activate(pointContext.Session);
 });
 public void EndPoint()=>Guard(()=>
 {
  Need(active==pointContext&&pointContext is not null,"point end state");var context=Current();context.Census.Accept("EndPoint","$");run.EndPoint();Close(context);active=null;pointContext=null;traceOwner.Detach();
 });
 public void Begin(string diagnostic,DiagnosticMenu menu)=>Guard(()=>
 {
  Need(active is null&&pointContext is null,"diagnostics cannot interrupt audit contexts");traceOwner.Callback(()=>prerequisites.ValidateDiagnosticMenu(diagnostic,menu));Open("diagnostic/"+diagnostic);Current().Census.Accept("Begin","$");
 });
 public void Background(string name,MixedBackground background)=>Guard(()=>{Current().Census.Accept("Background",name);SaveBackground(name,background);});
 public void Geometry(string name,MixedBackground background,MixedMetricGerm germ)=>Guard(()=>{Current().Census.Accept("Geometry",name);SaveGeometry(name,background,germ);});
 public void Scalars(string name,Rational[] values)=>Guard(()=>
 {Current().Census.Accept("ScalarArray",name,length:values.Length);Metadata("scalars/"+name,values.Select(Rat).ToArray());});
 public void End(string diagnostic)=>Guard(()=>
 {Need(Current().Plan.Id=="diagnostic/"+diagnostic,"diagnostic end identity");Current().Census.Accept("End","$");Close(Current());active=null;traceOwner.Detach();});
 public void SecondJet(string name,FT value)=>Guard(()=>
 {Current().Census.Accept("SecondJet",name,2);Mark("secondJet/"+name,2,value);});
 public void Original(string name,OriginalMixedResult result)=>Guard(()=>Structured("Original",name,v=>MixedStructuredFields.Original(result,v)));
 public void Ward(string name,WardActionResult result)=>Guard(()=>Structured("Ward",name,v=>MixedStructuredFields.Ward(result,v)));
 public void Acceleration(string name,WardAccelerationResult result)=>Guard(()=>Structured("Acceleration",name,v=>MixedStructuredFields.Acceleration(result,v)));
 void Structured(string category,string name,Action<MixedStructuredFields.Visitor> emit)
 {
  var child=new MixedCallbackCensus(MixedAuditPlan.Structured(category));var fields=new List<MixedPlannedCallback>();string prefix="structured/"+category+"/"+name;var scalarFields=new Dictionary<string,object>(StringComparer.Ordinal);
  void Field(MixedPlannedCallback f){child.Accept(f.Category,f.Name,f.Degree,f.Expanded,f.Length);fields.Add(f);}
  emit(new((field,degree,tensor)=>{Field(new("Tensor",field,degree));Mark(prefix+"/"+field,degree,tensor);},
   (field,values)=>{Field(new("ScalarArray",field,Length:values.Length));scalarFields.Add(field,values.Select(Rat).ToArray());},
   (field,value)=>{Field(new("Scalar",field));scalarFields.Add(field,Rat(value));},
   (field,value)=>{Field(new("Boolean",field));scalarFields.Add(field,value);}));
  child.Finish();Current().Census.AcceptStructured(category,name,fields);
  if(!Current().Plan.Id.StartsWith("diagnostic/",StringComparison.Ordinal))run.Current.AcceptStructured(category,name,fields);Metadata(prefix,scalarFields);
 }
 object Metric(MetricData metric)=>new{g=MatrixText(metric.G),d=metric.D.Select(MatrixText).ToArray(),dd=Enumerable.Range(0,14).Select(mu=>Enumerable.Range(0,14).Select(nu=>MatrixText(metric.DD[mu,nu])).ToArray()).ToArray()};
 object Connection(ConnectionData connection)=>new{gamma=connection.Gamma.Select(MatrixText).ToArray(),dGamma=Enumerable.Range(0,14).Select(mu=>Enumerable.Range(0,14).Select(nu=>MatrixText(connection.DGamma[mu,nu])).ToArray()).ToArray(),curvature=Enumerable.Range(0,14).Select(a=>Enumerable.Range(0,14).Select(b=>Enumerable.Range(0,14).Select(c=>Enumerable.Range(0,14).Select(d=>Rat(connection.R[a,b,c,d])).ToArray()).ToArray()).ToArray()).ToArray()};
 void SaveBackground(string name,MixedBackground b)
 {
  traceOwner.Callback(()=>prerequisites.ValidateBackground(Current().Plan.Id,name,b));
  Metadata(name+"/geometry",new{b.Point,frame=MatrixText(b.Frame),inverseFrame=MatrixText(b.InverseFrame),metric=Metric(b.Metric),connection=Connection(b.Connection),frameLift=b.FrameLift.Select(MatrixText).ToArray(),framePartial=b.FramePartial.Select(MatrixText).ToArray(),omega=b.Omega.Select(MatrixText).ToArray()});
  var values=Baseline(b);foreach(var role in MixedAuditPlan.BaselineRoles())Mark(name+"/"+role.Role,role.Degree,values[role.Role]);
  Mark(name+"/NativeX",1,b.NativeX);Mark(name+"/DXOracle",2,b.DXOracle);Mark(name+"/ReverseX",1,b.ReverseX);Mark(name+"/Gradient",1,b.Gradient);
  for(int mu=0;mu<14;mu++){Mark($"{name}/NativePartial[{mu}]",1,b.NativePartial[mu]);Mark($"{name}/NativePartialOracle[{mu}]",1,b.NativePartialOracle[mu]);Mark($"{name}/CovariantCoordinate[{mu}]",1,b.CovariantCoordinate[mu]);}
 }
 void SaveGeometry(string name,MixedBackground b,MixedMetricGerm g)
 {
  traceOwner.Callback(()=>prerequisites.ValidateGeometry(Current().Plan.Id,name,b,g));
  Metadata(name+"/geometry",new{g.MetricBasis,g.JetIndex,g.Multiindex,g.Order,shear=Metric(g.Shear),deltaMetric=Metric(g.DeltaMetric),blockMetric=Metric(g.BlockMetric),deltaConnection=Connection(g.DeltaConnection),palatini=Connection(g.Palatini),motion=MatrixText(g.Motion),motionPartial=g.MotionPartial.Select(MatrixText).ToArray(),motionCovariant=g.MotionCovariant.Select(MatrixText).ToArray(),deltaOmega=g.DeltaOmega.Select(MatrixText).ToArray(),deltaOmegaPartial=Enumerable.Range(0,14).Select(mu=>Enumerable.Range(0,14).Select(nu=>MatrixText(g.DeltaOmegaPartial[mu,nu])).ToArray()).ToArray(),deltaFrame=MatrixText(g.DeltaFrame)});
  Mark(name+"/DeltaB",1,g.DeltaB);Mark(name+"/DeltaFAdapted",2,g.DeltaFAdapted);Mark(name+"/DeltaFOracle",2,g.DeltaFOracle);Mark(name+"/DeltaFFixed",2,g.DeltaFFixed);Mark(name+"/DeltaBExterior",2,g.DeltaBExterior);Mark(name+"/DeltaCurvatureFromConnection",2,g.DeltaCurvatureFromConnection);
 }
 void Close(Context context)
 {
  context.Census.Finish();Need(context.Marks.SetEquals(context.MarkMenu.Keys),"complete frozen tensor mark census");
  if(context.Plan.PointCheckpoint is not null)
  {
   Need(context.BackgroundCheckpoint is not null,"point completion requires numerical background checkpoint");traceOwner.Detach();
   context.PointSeal!.Complete(context.BackgroundCheckpoint!,
    background=>WriteJson(context,context.Plan.MetadataPath,MixedPointEvidenceSeal.Envelope(context.Plan.Id,background.Metadata,context.Metadata),context.Plan.Resources.MetadataBytes),
    checkpoint=>traceOwner.Callback(()=>prerequisites.ValidatePointCompletion(checkpoint)));
   Need(context.PointSeal.ValidationReturned&&!context.PointSeal.Failed&&!reentered&&!failed,"healthy independent point terminal validation before completion");
   traceOwner.DisposeSession(context.Session);Need(completedContexts.Add(context.Plan.Id),"unique completed point");return;
  }
  var graph=context.Session.Finish();traceOwner.Detach();
  // Both immutable computational artifacts precede independent read/replay.
  // Their producer-complete/false-validation status is never rewritten, even
  // after acceptance; only the independent prerequisite closes this context.
  context.EvidenceSeal.Complete(
   ()=>Write(context,context.Plan.GraphPath,context.Plan.Resources.Trace.GraphBytes,s=>MixedTrace.WriteGraph(s,graph,context.Plan.Resources.Trace)),
   graphArtifact=>WriteJson(context,context.Plan.MetadataPath,MixedContextEvidenceSeal.Envelope(context.Plan.Id,graphArtifact,context.Metadata),context.Plan.Resources.MetadataBytes),
   checkpoint=>traceOwner.Callback(()=>prerequisites.ValidateContextEvidence(checkpoint)));
  Need(context.EvidenceSeal.ValidationReturned&&!context.EvidenceSeal.Failed&&!reentered&&!failed,"healthy combined artifact validation before context completion");
  traceOwner.DisposeSession(context.Session);Need(completedContexts.Add(context.Plan.Id),"unique completed context");
 }
 void WriteExpanded(Context context,string name,int degree,FT tensor)
 {
  var mark=context.MarkMenu[name];Need(mark.Expanded&&mark.RelativePath is not null,"expanded callback must have fixed path");
  Write(context,mark.RelativePath!,context.Plan.Resources.Trace.TensorBytes+1,stream=>{using(var writer=new Utf8JsonWriter(stream,new(){Encoder=MixedTrace.JsonOptions.Encoder}))MixedTrace.WriteTensor(writer,tensor,degree,context.Plan.Resources.Trace);stream.WriteByte(10);});
 }
 MixedEmittedArtifact WriteJson(Context context,string path,object value,long limit)=>Write(context,path,limit,stream=>{JsonSerializer.Serialize(stream,value,MixedTrace.JsonOptions);stream.WriteByte(10);});
 sealed class GuardedStream(Stream output,long limit,Action<int> charge):Stream
 {
  long length;readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);bool hashFinished;
  public override void Write(ReadOnlySpan<byte> bytes){Need(!hashFinished&&bytes.Length<=limit-length,"prewrite file ceiling or closed hash");charge(bytes.Length);output.Write(bytes);hash.AppendData(bytes);length+=bytes.Length;}
  public string FinishHash(){Need(!hashFinished,"single emitted hash finalization");hashFinished=true;return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();}
  protected override void Dispose(bool disposing){if(disposing)hash.Dispose();base.Dispose(disposing);}
  public override void Write(byte[] bytes,int offset,int count)=>Write(bytes.AsSpan(offset,count));public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;public override long Length=>length;public override long Position{get=>length;set=>throw new NotSupportedException();}
  public override void Flush()=>output.Flush();public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override long Seek(long o,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long l)=>throw new NotSupportedException();
 }
 MixedEmittedArtifact Write(Context context,string relative,long ceiling,Action<Stream> write,bool failureSnapshot=false)
 {
  if(!failureSnapshot)traceOwner.Callback(()=>prerequisites.CheckResources(context.Plan.Id));
  else Need(failed&&relative==context.Plan.GraphPath,"failure reserve only for the predeclared graph path");
  string path=PathFor(relative);Need(opened.Add(relative),"fixed path cannot be retried");Directory.CreateDirectory(Path.GetDirectoryName(path)!);_=PathFor(relative);
  long emittedBytes;string emittedHash;using(var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read))
  {
   long fileCeiling=Math.Min(ceiling,context.Plan.Resources.FileBytes);if(failureSnapshot)fileCeiling=Math.Min(fileCeiling,context.Plan.Resources.FailureGraphBytes);
   using var guarded=new GuardedStream(file,fileCeiling,count=>
   {
    if(failureSnapshot){Need(count<=plan.FailureGraphBytes-failureGraphBytes,"prewrite reserved failure-graph aggregate ceiling");failureGraphBytes=checked(failureGraphBytes+count);}
    else{Need(count<=plan.AggregateBytes-aggregateBytes&&count<=context.Plan.Resources.ContextBytes-context.Bytes,"prewrite aggregate/context ceiling");aggregateBytes=checked(aggregateBytes+count);context.Bytes=checked(context.Bytes+count);}
   });write(guarded);guarded.Flush();file.Flush(true);emittedBytes=guarded.Length;emittedHash=guarded.FinishHash();
  }
  var artifact=new MixedEmittedArtifact(relative,emittedBytes,emittedHash);completedFiles.Add(artifact);return artifact;
 }
 public void Complete()
 {
  Guard(()=>
  {Need(active is null&&pointContext is null,"no unfinished context");run.Finish();MixedAuditPlan.ValidateRunContextIds(completedContexts);Need(completedContexts.SetEquals(plan.ContextIds),"all705 planned contexts complete");});
  // Only finalize AFTER Guard's catalog/reentry postconditions. A late guard
  // failure must still emit failure evidence, not be masked by finished=true.
  finished=true;
 }
 // Terminal transport may only follow THIS actual sink's completed catalog.
 // Both calls around transport completion must run on the owning thread;
 // ConfigureAwait(false) followed by this method is not an ownership handoff.
 public void RequireCompletedCatalog(MixedContextTemplateCatalog expected)
 {
  traceOwner.RequireControl();traceOwner.CheckHealthy();
  Need(finished&&!failed&&!disposed&&!busy&&!reentered&&active is null&&pointContext is null&&MixedTrace.Active is null&&
   ReferenceEquals(plan.Templates,expected),"same healthy completed sink/catalog before terminal transport");
  CheckCatalog();Need(completedContexts.Count==MixedAuditPlan.RunContextCount,"all sink contexts completed before terminal transport");
 }
 public void Fail(Exception exception)
 {
  traceOwner.RequireControl();
  if(failed||finished||disposed)return;failed=true;var current=active;
  // Failure snapshots are observational/incomplete, not successful graphs.
  // Their separate byte reservations were mandatory before any calculation;
  // exhausting a normal budget cannot invent a new path or unlimited reserve.
  var graphSnapshots=new List<object>();
  void Snapshot(Context? context)
  {
   if(context is null||context.Session is null)return;
   if(opened.Contains(context.Plan.GraphPath)){graphSnapshots.Add(new{context=context.Plan.Id,path=context.Plan.GraphPath,status="previously-opened-file-preserved",complete=false});return;}
   try{var graph=traceOwner.FailureSnapshot(context.Session);Write(context,context.Plan.GraphPath,context.Plan.Resources.Trace.GraphBytes,s=>MixedTrace.WriteGraph(s,graph,context.Plan.Resources.Trace),true);graphSnapshots.Add(new{context=context.Plan.Id,path=context.Plan.GraphPath,status="incomplete-snapshot-written",complete=false});}
   catch(Exception snapshotFailure){graphSnapshots.Add(new{context=context.Plan.Id,path=context.Plan.GraphPath,status="snapshot-write-failed-partial-files-preserved",complete=false,errorType=snapshotFailure.GetType().FullName});}
  }
  Snapshot(current);if(!ReferenceEquals(current,pointContext))Snapshot(pointContext);
  traceOwner.Detach();
  if(current?.Session is { } currentSession)traceOwner.DisposeSession(currentSession);
  if(pointContext?.Session is { } pointSession&&!ReferenceEquals(current,pointContext))traceOwner.DisposeSession(pointSession);
  string path=PathFor(plan.FailurePath);Need(opened.Add(plan.FailurePath),"one failure record only");Directory.CreateDirectory(Path.GetDirectoryName(path)!);
  using var file=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);using var guarded=new GuardedStream(file,plan.FailureBytes,_=>{});
  JsonSerializer.Serialize(guarded,new{schema="phase627-failure-evidence-v1",status="failed",context=current?.Plan.Id,exceptionType=exception.GetType().FullName,graphSnapshots=graphSnapshots.ToArray(),pointBackgroundCheckpoint=pointContext?.BackgroundCheckpoint,pointTraversalComplete=false,partialContextMetadata=current?.Metadata,suspendedPointMetadata=ReferenceEquals(current,pointContext)?null:pointContext?.Metadata,openedPaths=opened.Order(StringComparer.Ordinal).ToArray(),completedFiles=completedFiles.ToArray()},MixedTrace.JsonOptions);guarded.WriteByte(10);guarded.Flush();file.Flush(true);
 }
 public void Dispose()
 {
  if(disposed)return;traceOwner.RequireControl();
  try{if(!finished&&!failed)Fail(new InvalidOperationException("evidence writer disposed before Complete"));}
  finally{traceOwner.Dispose();disposed=true;}
 }
}
