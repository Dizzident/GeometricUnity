// Metadata callback composition only: NOT IMixedAuditSinkPrerequisites and no
// scientific permission, leaf binding, geometry, coefficients or output sink.
// The OWNER performs the sole catalog.Freeze. The same callbacks can therefore
// serve the sink's existing prepass without trying to freeze its catalog twice.
// The external construction admission remains mandatory and independently
// reviewed; forwarding a void callback is not proof of resource sufficiency.
internal sealed class MixedTemplatePreflightCoordinator:IDisposable
{
 readonly MixedTemplatePreflightClient client;
 readonly MixedContextTemplateCatalog catalog;
 readonly Action<string> admitConstruction;
 readonly object gate=new();
 bool begun,captureAccepted,completed,failed,busy,disposed;
 int admitted,validated,secondAdmissions;string? pending;
 public MixedTemplatePreflightCoordinator(MixedTemplatePreflightClient client,MixedContextTemplateCatalog catalog,Action<string> admitConstruction)
 {
  this.client=client??throw new ArgumentNullException(nameof(client));
  this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));
  this.admitConstruction=admitConstruction??throw new ArgumentNullException(nameof(admitConstruction));
 }
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 metadata coordinator: "+why);}
 void Healthy()
 {
  Need(!failed&&!disposed&&!client.Failed&&!catalog.Snapshot().Failed,"healthy coordinator, client and catalog");
  if(completed)Need(client.Completed,"live terminal client acceptance");
 }
 async Task Guard(Func<Task> action,bool afterCompletion=false)
 {
  bool entered=false;
  try
  {
   lock(gate){Need(!failed&&!disposed&&!busy&&(!completed||afterCompletion),"failed, reentrant or closed operation");busy=true;entered=true;Healthy();}
   await action().ConfigureAwait(false);
   lock(gate)Healthy();
  }
  catch
  {
   lock(gate)failed=true;
   client.Dispose();throw;
  }
  finally{if(entered)lock(gate)busy=false;}
 }
 void Sync(Action action,bool afterCompletion=false)=>Guard(()=>{action();return Task.CompletedTask;},afterCompletion).GetAwaiter().GetResult();
 void CheckFirstPass(int factories,int frozen)
 {
  var state=catalog.Snapshot();
  Need(!state.Failed&&!state.Frozen&&state.FactoryCalls==factories&&state.FrozenContexts==frozen&&state.BoundContexts==0,
   "actual unbound catalog first-pass state");
 }
 void CheckFrozen(int constructions,int bindings)
 {
  var state=catalog.Snapshot();
  Need(!state.Failed&&state.Frozen&&state.FrozenContexts==MixedAuditPlan.RunContextCount&&
   state.FactoryCalls==MixedAuditPlan.RunContextCount+constructions&&state.BoundContexts==bindings,
   "actual frozen catalog construction/binding state");
 }
 public Task BeginAsync()=>Guard(async()=>
 {
  Need(!begun&&!captureAccepted&&admitted==0&&validated==0,"one initial handshake");CheckFirstPass(0,0);
  await client.BeginAsync().ConfigureAwait(false);CheckFirstPass(0,0);begun=true;
 });
 public void ValidateCaptureDeclaration(MixedCaptureDeclaration declaration)=>Sync(()=>
 {
  Need(begun&&!captureAccepted&&admitted==0&&validated==0,"capture match immediately after handshake");CheckFirstPass(0,0);
  client.RequireCaptureDeclaration(declaration);captureAccepted=true;
 });
 public void CheckTemplateConstructionResources(string context)=>Sync(()=>
 {
  Need(begun&&captureAccepted,"capture acceptance before construction");
  if(!completed)
  {
   Need(pending is null&&admitted==validated&&validated<MixedAuditPlan.RunContextCount&&context==catalog.ContextIds[validated],
    "one ordered admission before each template");CheckFirstPass(validated,validated);
   admitConstruction(context);Healthy();CheckFirstPass(validated,validated);
   pending=context;admitted++;
  }
  else
  {
   // Materialize regenerates each committed template once BEFORE independent
   // scientific binding. Keep that second admission path usable; it neither
   // binds leaves nor replaces the catalog's exact fingerprint comparison.
   Need(secondAdmissions<MixedAuditPlan.RunContextCount&&context==catalog.ContextIds[secondAdmissions],
    "one ordered second construction per context");CheckFrozen(secondAdmissions,secondAdmissions);
   admitConstruction(context);Healthy();CheckFrozen(secondAdmissions,secondAdmissions);secondAdmissions++;
  }
 },afterCompletion:true);
 public void ValidateContextTemplate(MixedSinkContextTemplate template)=>Sync(()=>
 {
  Need(begun&&captureAccepted&&pending is not null&&template is not null&&template.Id==pending&&admitted==validated+1,
   "validation requires its exact pending construction");CheckFirstPass(admitted,validated);
  catalog.RequireCurrentValidation(template!);
  client.ValidateAsync(template!).GetAwaiter().GetResult();Healthy();CheckFirstPass(admitted,validated);
  catalog.RequireCurrentValidation(template!);
  pending=null;validated++;
 });
 public void CompleteTemplateValidation()=>Sync(()=>
 {
  Need(begun&&captureAccepted&&pending is null&&admitted==MixedAuditPlan.RunContextCount&&validated==admitted,
   "all705 admitted and validated templates before completion");CheckFrozen(0,0);
  client.CompleteAsync().GetAwaiter().GetResult();Need(client.Completed&&!client.Failed,"terminal acknowledgement and EOF");
  CheckFrozen(0,0);completed=true;
 });
 public bool Completed
 {
  get
  {
   lock(gate)
   {
    var state=catalog.Snapshot();
    return completed&&!failed&&!disposed&&!busy&&client.Completed&&!state.Failed&&state.Frozen&&
     state.FrozenContexts==MixedAuditPlan.RunContextCount&&state.FactoryCalls==MixedAuditPlan.RunContextCount+secondAdmissions&&
     state.BoundContexts==secondAdmissions;
   }
  }
 }
 public bool Failed{get{lock(gate)return failed||client.Failed||catalog.Snapshot().Failed;}}
 public void Dispose()
 {
  lock(gate){if(disposed)return;if(!completed||busy)failed=true;disposed=true;}
  client.Dispose();
 }
}
