using System.Security.Cryptography;
using System.Text.Json;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal sealed record MixedContextPlanValidationLimits(long PlanBytes,long TotalSerializedBytes);
internal sealed record MixedLeafValidationLimits(long Fingerprints,long TotalSerializedBytes);

// Actual catalog/callback connection for a separately owned producer session.
// NOT a complete IMixedAuditSinkPrerequisites implementation: geometry,
// parameter and whole-resource validation remain required.
// No process launcher, scientific entry point or implicit FIRST authorization.
internal sealed class MixedSourcePreparationCoordinator:IDisposable
{
 readonly MixedSourcePreparationClient client;
 readonly MixedContextTemplateCatalog catalog;
 readonly Action<string> admitConstruction;
 readonly MixedContextPlanValidationLimits planLimits;
 readonly Action<string,string> admitPlanValidation;
 readonly MixedLeafValidationLimits leafLimits;
 readonly Action<string,string,int> admitLeafValidation;
 readonly Action<string> admitDiagnosticMenu;
 readonly object gate=new();
 bool begun,captureAccepted,templatesCompleted,completed,failed,busy,disposed;
 int admitted,validated,secondAdmissions,bound,next;
 string? pendingValidation,pendingCheckpoint;
 (long Bytes,string Sha256)? expectedPlan;
 bool planValidated;long planSerializedBytes;
 IReadOnlyList<MixedTrace.LeafSpec>? expectedLeaves;
 IReadOnlyDictionary<string,string>? expectedRoles;
 MixedTrace.Limits? expectedTrace;
 bool[]? acceptedLeaves;int acceptedLeafCount;long fingerprints,leafSerializedBytes;
 string? acceptedDiagnostic;int diagnosticMenus;
 public MixedSourcePreparationCoordinator(MixedSourcePreparationClient client,MixedContextTemplateCatalog catalog,Action<string> admitConstruction,
  MixedContextPlanValidationLimits planLimits,Action<string,string> admitPlanValidation,
  MixedLeafValidationLimits leafLimits,Action<string,string,int> admitLeafValidation,Action<string> admitDiagnosticMenu)
 {
  this.client=client??throw new ArgumentNullException(nameof(client));
  this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));
  this.admitConstruction=admitConstruction??throw new ArgumentNullException(nameof(admitConstruction));
  this.planLimits=planLimits??throw new ArgumentNullException(nameof(planLimits));
  this.admitPlanValidation=admitPlanValidation??throw new ArgumentNullException(nameof(admitPlanValidation));
  Need(planLimits.PlanBytes>0&&planLimits.TotalSerializedBytes>0,"explicit positive plan serialization ceilings");
  this.leafLimits=leafLimits??throw new ArgumentNullException(nameof(leafLimits));
  this.admitLeafValidation=admitLeafValidation??throw new ArgumentNullException(nameof(admitLeafValidation));
  Need(leafLimits.Fingerprints>0&&leafLimits.TotalSerializedBytes>0,"explicit positive leaf fingerprint ceilings");
  this.admitDiagnosticMenu=admitDiagnosticMenu??throw new ArgumentNullException(nameof(admitDiagnosticMenu));
 }
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 source coordinator: "+why);}
 void Healthy()
 {
  Need(!failed&&!disposed&&!client.Failed&&!catalog.Snapshot().Failed,"healthy coordinator client and actual catalog");
  if(templatesCompleted)Need(client.TemplatesCompleted,"live full-template phase acceptance");
  if(completed)Need(client.Completed,"live terminal client acceptance");
 }
 async Task Guard(Func<Task> action)
 {
  bool entered=false;
  try
  {
   lock(gate){Need(!failed&&!disposed&&!busy&&!completed,"failed reentrant or closed operation");busy=true;entered=true;Healthy();}
   await action().ConfigureAwait(false);
   lock(gate)Healthy();
  }
  catch{lock(gate){failed=true;ClearPlan();}client.Dispose();throw;}
  finally{if(entered)lock(gate)busy=false;}
 }
 void Sync(Action action)=>Guard(()=>{action();return Task.CompletedTask;}).GetAwaiter().GetResult();
 void ClearPlan()
 {expectedPlan=null;planValidated=false;expectedLeaves=null;expectedRoles=null;expectedTrace=null;acceptedLeaves=null;acceptedLeafCount=0;acceptedDiagnostic=null;}
 static bool AliasRole(string role)=>role=="baseline/GradientPieces[0]";
 void RequireAllLeaves()=>Need(expectedLeaves is not null&&acceptedLeaves is not null&&acceptedLeafCount==expectedLeaves.Count,
  "every independently bound canonical leaf accepted before checkpoint");
 // Only one digest is retained across Materialize, never the full705 plans.
 // Mandatory admission precedes callback-array projection and serializer/hash
 // allocation. Byte ceilings bound writes, NOT serializer scratch or whole RSS;
 // those remain the reviewed admission owner's responsibility.
 (long Bytes,string Sha256) FingerprintPlan(object wire)
 {
  using var stream=new PlanHashStream(planLimits.PlanBytes,n=>
  {Need(n<=planLimits.TotalSerializedBytes-planSerializedBytes,"cumulative plan serialization ceiling");planSerializedBytes=checked(planSerializedBytes+n);});
  JsonSerializer.Serialize(stream,wire,MixedTrace.JsonOptions);stream.WriteByte(10);
  return(stream.Length,stream.Finish());
 }
 static object PlanWire(string id,string graphPath,string metadataPath,MixedSinkResources resources,
  MixedPointCheckpointPlan? pointCheckpoint,IReadOnlyList<MixedTrace.LeafSpec> leaves,
  IReadOnlyDictionary<string,string> leafRoles,IReadOnlyList<MixedSinkMark> marks,MixedContextPlan callbacks)=>
  new{schema="phase627-bound-context-plan-v1",id,graphPath,metadataPath,resources,pointCheckpoint,leaves,leafRoles,marks,callbacks};
 sealed class PlanHashStream(long ceiling,Action<int> charge):Stream
 {
  readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long count;bool finished;
  public override void Write(ReadOnlySpan<byte> bytes)
  {Need(!finished&&bytes.Length<=ceiling-count,"plan serialization prewrite ceiling");charge(bytes.Length);hash.AppendData(bytes);count+=bytes.Length;}
  public string Finish(){Need(!finished,"one plan hash completion");finished=true;return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();}
  public override void Write(byte[] bytes,int offset,int length)=>Write(bytes.AsSpan(offset,length));
  public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;public override long Length=>count;
  public override long Position{get=>count;set=>throw new NotSupportedException();}public override void Flush(){}
  public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override long Seek(long o,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long l)=>throw new NotSupportedException();
  protected override void Dispose(bool disposing){if(disposing)hash.Dispose();base.Dispose(disposing);}
 }
 void FirstPass(int factories,int frozen)
 {
  var state=catalog.Snapshot();
  Need(!state.Failed&&!state.Frozen&&state.FactoryCalls==factories&&state.FrozenContexts==frozen&&state.BoundContexts==0,
   "actual unbound first-pass catalog state");
 }
 void Frozen(int constructions,int bindings)
 {
  var state=catalog.Snapshot();
  Need(!state.Failed&&state.Frozen&&state.FrozenContexts==705&&state.FactoryCalls==705+constructions&&state.BoundContexts==bindings,
   "actual frozen construction and binding counts");
 }
 (string Kind,string Context) Event()
 {
  Need(next is >=0 and <707,"fixed707 checkpoint sequence");
  if(next>=704)return("diagnostic","diagnostic/"+new[]{"grade10","acceleration","secondJets"}[next-704]);
  int point=next/352,offset=next%352;string id="point"+point;
  return offset==0?("background",id):offset==351?("final",id):("germ",id+"/m"+((offset-1)/35)+"_j"+((offset-1)%35));
 }
 public Task BeginAsync()=>Guard(async()=>
 {
  Need(!begun&&!captureAccepted&&admitted==0&&validated==0,"one initial producer handshake");FirstPass(0,0);
  await client.BeginAsync().ConfigureAwait(false);FirstPass(0,0);begun=true;
 });
 public void ValidateCaptureDeclaration(MixedCaptureDeclaration declaration)=>Sync(()=>
 {
  Need(begun&&!captureAccepted&&admitted==0&&validated==0,"actual sink capture immediately after handshake");FirstPass(0,0);
  client.RequireCaptureDeclaration(declaration);captureAccepted=true;
 });
 public void CheckTemplateConstructionResources(string context)=>Sync(()=>
 {
  Need(begun&&captureAccepted,"capture acceptance before construction");
  if(!templatesCompleted)
  {
   Need(pendingValidation is null&&admitted==validated&&validated<705&&context==catalog.ContextIds[validated],"one ordered first-pass admission");
   FirstPass(validated,validated);admitConstruction(context);Healthy();FirstPass(validated,validated);
   pendingValidation=context;admitted++;
  }
  else
  {
   var expected=Event();
   Need(pendingCheckpoint is null&&expected.Kind!="final"&&bound<705&&secondAdmissions==bound&&
    context==catalog.ContextIds[bound]&&context==expected.Context,"one source construction after previous evidence acceptance");
   Need(expected.Kind=="diagnostic"?acceptedDiagnostic==context:acceptedDiagnostic is null,"complete independent diagnostic menu before factory admission");
   Frozen(bound,bound);admitConstruction(context);Healthy();Frozen(bound,bound);secondAdmissions++;
  }
 });
 public void ValidateContextTemplate(MixedSinkContextTemplate template)=>Sync(()=>
 {
  Need(begun&&captureAccepted&&!templatesCompleted&&pendingValidation is not null&&template is not null&&
   template.Id==pendingValidation&&admitted==validated+1,"exact pending full-template validation");
  FirstPass(admitted,validated);catalog.RequireCurrentValidation(template!);
  client.ValidateAsync(template!).GetAwaiter().GetResult();Healthy();FirstPass(admitted,validated);catalog.RequireCurrentValidation(template!);
  pendingValidation=null;validated++;
 });
 public void CompleteTemplateValidation()=>Sync(()=>
 {
  Need(begun&&captureAccepted&&!templatesCompleted&&pendingValidation is null&&admitted==705&&validated==705,"complete producer's actual705 prepass");
  Frozen(0,0);client.CompleteTemplateValidationAsync().GetAwaiter().GetResult();Healthy();Frozen(0,0);
  Need(client.TemplatesCompleted,"template-phase acknowledgement without transport EOF");templatesCompleted=true;
 });
 public void ValidateDiagnosticMenu(string diagnostic,DiagnosticMenu menu)=>Sync(()=>
 {
  var expected=Event();
  Need(templatesCompleted&&pendingCheckpoint is null&&acceptedDiagnostic is null&&expected.Kind=="diagnostic"&&
   secondAdmissions==bound&&diagnosticMenus==next-704&&diagnostic is not null&&expected.Context=="diagnostic/"+diagnostic&&
   menu is not null&&menu.Id==diagnostic,"one next full diagnostic menu before materialization admission");
  Frozen(bound,bound);admitDiagnosticMenu(expected.Context);Healthy();Frozen(bound,bound);
  client.ValidateDiagnosticMenuAsync(diagnostic!,menu!).GetAwaiter().GetResult();Healthy();Frozen(bound,bound);
  acceptedDiagnostic=expected.Context;diagnosticMenus++;
 });
 public IReadOnlyList<MixedTrace.LeafSpec> BindContextLeaves(MixedSinkContextTemplate template)
 {
  IReadOnlyList<MixedTrace.LeafSpec>? leaves=null;
  Sync(()=>
  {
   var expected=Event();
   Need(templatesCompleted&&pendingCheckpoint is null&&expected.Kind!="final"&&template is not null&&
    template.Id==expected.Context&&bound<705&&template.Id==catalog.ContextIds[bound]&&secondAdmissions==bound+1,
    "exact next admitted materialization before source binding");
   Need(expected.Kind=="diagnostic"?acceptedDiagnostic==expected.Context:acceptedDiagnostic is null,"accepted full diagnostic menu before source binding");
   Frozen(secondAdmissions,bound);catalog.RequireCurrentMaterialization(template!);
   leaves=client.PrepareAsync(template!).GetAwaiter().GetResult();Healthy();Frozen(secondAdmissions,bound);catalog.RequireCurrentMaterialization(template!);
   Need(expectedPlan is null&&!planValidated,"no previous plan binding retained");
   admitPlanValidation(template!.Id,"expected-plan");Healthy();Frozen(secondAdmissions,bound);catalog.RequireCurrentMaterialization(template);
   // Template identity is catalog-owned; all four leaf fields come from the
   // client's exact authenticated ACK, never from a caller-submitted plan.
   expectedPlan=FingerprintPlan(PlanWire(template.Id,template.GraphPath,template.MetadataPath,template.Resources,
    template.PointCheckpoint,leaves,template.LeafRoles,template.Marks,template.Callbacks));
   admitLeafValidation(template.Id,"bindings",leaves.Count);Healthy();Frozen(secondAdmissions,bound);catalog.RequireCurrentMaterialization(template);
   // Retain only this context's independently bound leaf authority, never FT
   // values or the full template. Source's sole alias is not a second import.
   foreach(var leaf in leaves)
   {
    int roles=0;foreach(var role in template.LeafRoles)if(!AliasRole(role.Key)&&role.Value==leaf.Id)roles++;
    Need(roles==1,"exactly one canonical import role per source-bound leaf");
   }
   expectedLeaves=leaves;expectedRoles=template.LeafRoles;expectedTrace=template.Resources.Trace;
   acceptedLeaves=new bool[leaves.Count];acceptedLeafCount=0;
   pendingCheckpoint=template!.Id;bound++;acceptedDiagnostic=null;
  });
  return leaves!;
 }
 public void ValidateContextPlan(MixedSinkContextPlan plan)=>Sync(()=>
 {
  Need(templatesCompleted&&pendingCheckpoint is not null&&expectedPlan is not null&&!planValidated&&
   plan is not null&&plan.Id==pendingCheckpoint,"one exact pending materialized plan");
  // BoundContexts increments only AFTER the catalog's actual Bind completes.
  Frozen(bound,bound);catalog.RequireLatestBoundPlan(plan!);
  admitPlanValidation(plan!.Id,"actual-plan");Healthy();Frozen(bound,bound);catalog.RequireLatestBoundPlan(plan);
  var actual=FingerprintPlan(PlanWire(plan.Id,plan.GraphPath,plan.MetadataPath,plan.Resources,
   plan.PointCheckpoint,plan.Leaves,plan.LeafRoles,plan.Marks,plan.Callbacks));
  Need(actual==expectedPlan!.Value,"all materialized plan fields match accepted template and source ACK");expectedPlan=null;planValidated=true;
 });
 public void ValidateLeaf(string context,string role,MixedTrace.LeafSpec declared,FT actual)=>Sync(()=>
 {
  // Genuine scope admission in BOTH builds; the callback does not replace it.
  var admission=MixedProducerAdmission.Current;
  try
  {
   Need(templatesCompleted&&planValidated&&pendingCheckpoint==context&&context is not null&&
    expectedLeaves is not null&&expectedRoles is not null&&expectedTrace is not null&&acceptedLeaves is not null,
    "active independently bound plan before leaf validation");
   Frozen(bound,bound);
   Need(role is not null&&!AliasRole(role)&&expectedRoles!.TryGetValue(role,out _),"canonical declared leaf role, not an alias");
   string id=expectedRoles![role!];int index=-1;
   for(int i=0;i<expectedLeaves!.Count;i++)if(expectedLeaves[i].Id==id){index=i;break;}
   Need(index>=0&&!acceptedLeaves![index]&&declared is not null&&declared==expectedLeaves[index]&&actual is not null,
    "unconsumed exact source-bound leaf declaration and actual tensor");
   Need(fingerprints<leafLimits.Fingerprints,"prospective cumulative fingerprint count");fingerprints++;
   admitLeafValidation(context!,"fingerprint",actual!.Count);Healthy();admission.EnsureActive();Frozen(bound,bound);
   var digest=MixedTrace.FingerprintAdmitted(actual,expectedLeaves[index].Degree,expectedTrace!,n=>
   {
    Healthy();admission.EnsureActive();Need(n<=leafLimits.TotalSerializedBytes-leafSerializedBytes,"cumulative leaf serialization ceiling");
    leafSerializedBytes=checked(leafSerializedBytes+n);
   });
   Healthy();admission.EnsureActive();Frozen(bound,bound);
   Need(digest.Sha256==expectedLeaves[index].Sha256,"actual full tensor matches independent source leaf SHA");
   acceptedLeaves![index]=true;acceptedLeafCount++;
  }
  catch{admission.Poison();throw;}
 });
 public void ValidatePointBackground(MixedPointBackgroundCheckpoint checkpoint)=>Sync(()=>
 {
  var expected=Event();Need(templatesCompleted&&planValidated&&expected.Kind=="background"&&pendingCheckpoint==expected.Context&&
   checkpoint is not null&&checkpoint.Context==expected.Context,"prepared exact background before child traversal");
  Frozen(bound,bound);RequireAllLeaves();client.AcceptBackgroundAsync(checkpoint!).GetAwaiter().GetResult();Healthy();Frozen(bound,bound);
  pendingCheckpoint=null;ClearPlan();next++;
 });
 public void ValidateContextEvidence(MixedContextComputationalCheckpoint checkpoint)=>Sync(()=>
 {
  var expected=Event();Need(templatesCompleted&&planValidated&&(expected.Kind is "germ" or "diagnostic")&&pendingCheckpoint==expected.Context&&
   checkpoint is not null&&checkpoint.Context==expected.Context,"prepared exact germ or diagnostic checkpoint");
  Frozen(bound,bound);RequireAllLeaves();client.AcceptContextAsync(checkpoint!).GetAwaiter().GetResult();Healthy();Frozen(bound,bound);
  pendingCheckpoint=null;ClearPlan();next++;
 });
 public void ValidatePointCompletion(MixedPointCompletionCheckpoint checkpoint)=>Sync(()=>
 {
  var expected=Event();Need(templatesCompleted&&expected.Kind=="final"&&pendingCheckpoint is null&&
   checkpoint is not null&&checkpoint.Context==expected.Context,"point final after all350 accepted children");
  Frozen(bound,bound);client.CompletePointAsync(checkpoint!).GetAwaiter().GetResult();Healthy();Frozen(bound,bound);next++;
 });
 // Sink ownership is thread-affine. Keep BOTH actual sink checks on the
 // original caller thread while the private client's async I/O is awaited.
 public void Complete(MixedAuditSink sink)=>Sync(()=>
 {
  Need(templatesCompleted&&next==707&&bound==705&&diagnosticMenus==3&&acceptedDiagnostic is null&&pendingCheckpoint is null&&sink is not null,"all evidence and diagnostic menus before actual sink completion");
  Frozen(705,705);sink!.RequireCompletedCatalog(catalog);
  client.CompleteAsync().GetAwaiter().GetResult();Healthy();Frozen(705,705);sink.RequireCompletedCatalog(catalog);
  Need(client.Completed,"clean final transport completion");completed=true;
 });
 public bool TemplatesCompleted{get{lock(gate)return templatesCompleted&&!failed&&!disposed&&!busy&&client.TemplatesCompleted&&!catalog.Snapshot().Failed;}}
 public bool Completed{get{lock(gate)return completed&&!failed&&!disposed&&!busy&&client.Completed&&!catalog.Snapshot().Failed;}}
 public bool Failed{get{lock(gate)return failed||client.Failed||catalog.Snapshot().Failed;}}
 public void Dispose(){lock(gate){if(disposed)return;if(!completed||busy)failed=true;disposed=true;ClearPlan();}client.Dispose();}
}
