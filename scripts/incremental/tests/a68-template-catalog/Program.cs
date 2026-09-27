using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Manufactured structural metadata ONLY. Never execute a background/germ
// scientific constructor, source algebra, operator or sink that emits science.
// Geometry-wire tests use explicitly uninitialized manufactured containers.
// Minimal fixtures exercise the generic catalog, NOT valid scientific menus.
static partial class Program
{
 static readonly Assembly Source=Assembly.Load("Phase627FullMixedMetricNativeFieldVariationAudit");
 static readonly string EmptyHash=Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes("[]"))).ToLowerInvariant();
 static int tests;
 static Type T(string name)=>Source.GetType(name,true)!;
 static object New(string name,params object?[] args)
 {try{return Activator.CreateInstance(T(name),args)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
 static object? Call(object target,string name,params object?[] args)
 {try{return target.GetType().GetMethod(name)!.Invoke(target,args);}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
 static object Get(object target,string name)=>target.GetType().GetProperty(name)!.GetValue(target)!;
 static Array Items(string type,params object[] values){var a=Array.CreateInstance(T(type),values.Length);for(int i=0;i<values.Length;i++)a.SetValue(values[i],i);return a;}
 static void Need(bool ok,string why){if(!ok)throw new Exception("metadata test: "+why);}
 static void Reject(Action action,string why)
 {try{action();}catch(InvalidOperationException){return;}throw new Exception("expected rejection: "+why);}
 static void Test(string name,Action action){action();tests++;Console.WriteLine("PASS metadata-only: "+name);}
 static Delegate Factory(Func<string,object> f)
 {var p=Expression.Parameter(typeof(string));return Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(string),T("MixedSinkContextTemplate")),Expression.Convert(Expression.Invoke(Expression.Constant(f),p),T("MixedSinkContextTemplate")),p).Compile();}
 static Delegate Validator(Action<object> f)
 {var p=Expression.Parameter(T("MixedSinkContextTemplate"));return Expression.Lambda(typeof(Action<>).MakeGenericType(p.Type),Expression.Invoke(Expression.Constant(f),Expression.Convert(p,typeof(object))),p).Compile();}
 static Delegate Binder(Func<object,object> f)
 {var p=Expression.Parameter(T("MixedSinkContextTemplate"));var output=typeof(IReadOnlyList<>).MakeGenericType(T("MixedTrace+LeafSpec"));return Expression.Lambda(typeof(Func<,>).MakeGenericType(p.Type,output),Expression.Convert(Expression.Invoke(Expression.Constant(f),Expression.Convert(p,typeof(object))),output),p).Compile();}
 static object Template(string id,string change="")
 {
  var trace=New("MixedTrace+Limits",100,100,100,128,100000L,100000L,100000L,100000L,100000L);
  var resources=New("MixedSinkResources",trace,100000L,1000000L,100000L,change=="resources"?99999L:100000L);
  var leaves=Items("MixedSinkLeafDeclaration",New("MixedSinkLeafDeclaration","x",change=="degree"?2:1,change=="source"?"changed-source":"manufactured-source"));
  var marks=Items("MixedSinkMark",New("MixedSinkMark",change=="mark"?"changed":"tiny",1,change=="expansion",change=="expansion"?id+"/tiny.json":null));
  var callbacks=New("MixedContextPlan",id,Items("MixedPlannedCallback",New("MixedPlannedCallback",change=="callback"?"Other":"Begin","$",null,null,null),New("MixedPlannedCallback","End","$",null,null,null)));
  return New("MixedSinkContextTemplate",id,id+"/graph.json",id+(change=="path"?"/other.json":"/metadata.json"),resources,leaves,
   new Dictionary<string,string>{{change=="role"?"other":"input/X","x"}},marks,callbacks,
   id is "point0" or "point1"?New("MixedPointCheckpointPlan",id+"/checkpoint.json",100000L):null);
 }
 static object Catalog(Func<string,object>? factory=null,long per=100000,long total=10000000)=>New("MixedContextTemplateCatalog",Factory(factory??(id=>Template(id))),New("MixedTemplateResources",per,total));
 static object Leaves(object template,string hash="",string id="x",string source="manufactured-source",int degree=1)=>Items("MixedTrace+LeafSpec",New("MixedTrace+LeafSpec",id,degree,source,hash==""?EmptyHash:hash));
 static void Freeze(object catalog,Action<string>? admit=null,Action<object>? validate=null)=>Call(catalog,"Freeze",admit??(_=>{}),Validator(validate??(_=>{})));
 static object Materialize(object catalog,string id,Func<object,object>? binder=null,Action<string>? admit=null)=>Call(catalog,"Materialize",id,admit??(_=>{}),Binder(binder??(t=>Leaves(t))))!;
 static object Usage(object c)=>Call(c,"Snapshot")!;
 static object Describe(string id)
 {try{return T("MixedSourceContextFactory").GetMethod("Describe")!.Invoke(null,[id])!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
 static string[] SourceIds()=> (string[])T("MixedAuditPlan").GetMethod("RunContextIds")!.Invoke(null,null)!;
 static readonly JsonSerializerOptions Json=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase,Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
 static string Hash(object value)=>Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value,Json))).ToLowerInvariant();
 static object SourceBudget(string id,long? checkpointOverride=null)
 {
  var trace=New("MixedTrace+Limits",3000000,5000,100000,128,100000000L,100000000L,100000000L,100000000L,100000000L);
  return New("MixedSourceTemplateBudget",id,New("MixedSinkResources",trace,100000000L,1000000000L,100000000L,100000000L),
   checkpointOverride??(id is "point0" or "point1"?100000000L:null));
 }
 static object SourceFactory(bool optional=false,Array? budgets=null)=>New("MixedSourceContextFactory",
  budgets??Items("MixedSourceTemplateBudget",SourceIds().Select(id=>SourceBudget(id)).ToArray()),New("MixedSourceRetention",optional,optional,optional,optional));
 // TEST-ONLY topology-covering wire values, NOT sufficient production resources.
 static object CaptureDeclaration(long firstCopies=0)=>New("MixedCaptureDeclaration",
  New("MixedTrace+CaptureLimits",New("MixedTrace+CopyCounts",705L,422622614L,422624729L,999971128L,999971128L),
   New("MixedTrace+CopyCounts",0L,0L,0L,0L,0L),New("MixedTrace+CopyCounts",2L,2399809L,2399812L,5681242L,5681242L)),
  Items("MixedCaptureInspection",SourceIds().Select((id,i)=>New("MixedCaptureInspection",id,i==0?firstCopies:0L)).ToArray()));
 static void SourceSummaries()
 {
  // Bounded metadata stdout only, never phase output or scientific evaluation.
  foreach(string id in SourceIds())
  {
   var menu=Describe(id);var callbacks=((IEnumerable)Get(Get(menu,"Callbacks"),"Callbacks")).Cast<object>()
    .OrderBy(c=>(string)Get(c,"Category"),StringComparer.Ordinal).ThenBy(c=>(string)Get(c,"Name"),StringComparer.Ordinal).ToArray();
   Console.WriteLine(JsonSerializer.Serialize(new{id,marks=Hash(Get(menu,"Marks")),markCount=((IEnumerable)Get(menu,"Marks")).Cast<object>().Count(),
    leaves=Get(menu,"Leaves"),leafRoles=Get(menu,"LeafRoles"),callbacks=Hash(callbacks),callbackCount=callbacks.Length},Json));
  }
 }
 static void SourceTemplates()
 {
  // Actual template wire, stdout ONLY. No scientific sink/geometry execution.
  var factory=SourceFactory();var options=(JsonSerializerOptions)T("MixedTrace").GetField("JsonOptions")!.GetValue(null)!;
  foreach(string id in SourceIds())
  {
   var template=Call(factory,"Create",id)!;
   var wire=template.GetType().GetMethod("Wire",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(template,null)!;
   Console.WriteLine(JsonSerializer.Serialize(wire,options));
  }
 }
 static void DiagnosticMenus()
 {
  // Actual complete diagnostic declarations, preserving every array's source
  // order and all three integer counters. No diagnostic/scientific execution.
  foreach(string method in new[]{"GradeTenMenu","AccelerationMenu","SecondJetsMenu"})
   Console.WriteLine(JsonSerializer.Serialize(T("MixedDiagnostics").GetMethod(method)!.Invoke(null,null),Json));
 }
 static async Task PreflightClient(string profileSha)
 {
  // Linux-only integration fixture: own dedicated CHILD protocol FDs, not
  // stdout (the runtime retains an alias, preventing EOF until process exit).
  // Never close runtime-owned descriptors to manufacture transport completion.
  // No production process launcher, sink or scientific operator is invoked.
  Need(OperatingSystem.IsLinux(),"pipe fixture uses Linux dedicated descriptors");
  using var requests=new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle((IntPtr)3,true),FileAccess.Write);
  using var replies=new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle((IntPtr)4,true),FileAccess.Read);
  var client=New("MixedTemplatePreflightClient",requests,replies,profileSha,New("MixedTemplateTransportLimits",2000000,536870912L,4096,4194304L,15000),CaptureDeclaration());
  try
  {
   var factory=SourceFactory();int admissions=0,factories=0;
   var catalog=Catalog(id=>{Need(admissions==factories+1,"admission before actual source factory");factories++;return Call(factory,"Create",id)!;},per:2000000,total:1000000000);
   // TEST-ONLY resource callback: proves ordering, not sufficient production memory.
   var coordinator=New("MixedTemplatePreflightCoordinator",client,catalog,(Action<string>)(id=>
   {Need(id==SourceIds()[admissions],"ordered actual705 construction admission");admissions++;}));
   try
   {
    await ((Task)Call(coordinator,"BeginAsync")!);Call(coordinator,"ValidateCaptureDeclaration",CaptureDeclaration());
    Freeze(catalog,id=>Call(coordinator,"CheckTemplateConstructionResources",id),template=>Call(coordinator,"ValidateContextTemplate",template));
    Need(!(bool)Get(coordinator,"Completed"),"freeze alone is not terminal acceptance");Call(coordinator,"CompleteTemplateValidation");
    var usage=Usage(catalog);
    Need((bool)Get(coordinator,"Completed")&&!(bool)Get(coordinator,"Failed")&&(bool)Get(client,"Completed")&&!(bool)Get(client,"Failed"),"complete metadata-only duplex transaction");
    Need(admissions==705&&factories==705&&(int)Get(usage,"FactoryCalls")==705&&(int)Get(usage,"FrozenContexts")==705&&
     (int)Get(usage,"BoundContexts")==0&&(bool)Get(usage,"Frozen")&&!(bool)Get(usage,"Failed"),"healthy real full catalog prepass with no leaf binding");
   }
   finally{((IDisposable)coordinator).Dispose();}
  }
  finally{((IDisposable)client).Dispose();}
 }
 static string BeginReply()
 {
  var request=Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new{schema="phase627-template-preflight-request-v2",sequence=0,operation="begin",profileSha256=EmptyHash,template=CaptureDeclaration()},Json)+"\n");
  string hash=Convert.ToHexString(SHA256.HashData(request)).ToLowerInvariant();
  return JsonSerializer.Serialize(new{schema="phase627-template-preflight-reply-v2",sequence=0,operation="begin",requestSha256=hash,status="accepted",report=(object?)null},Json)+"\n";
 }
 static object Client(Stream requests,Stream replies,int frame=2000000,long total=536870912,int reply=4096,long replyTotal=4194304,int timeout=1000)
  =>New("MixedTemplatePreflightClient",requests,replies,EmptyHash,New("MixedTemplateTransportLimits",frame,total,reply,replyTotal,timeout),CaptureDeclaration());
 static void Await(object target,string method,params object?[] args)=>((Task)Call(target,method,args)!).GetAwaiter().GetResult();
 static void RejectClient(object client,Action action)
 {
  bool rejected=false;try{action();}catch(Exception error)when(error is InvalidOperationException or JsonException or OperationCanceledException){rejected=true;}
  Need(rejected&&(bool)Get(client,"Failed")&&!(bool)Get(client,"Completed"),"failed transport cannot report completion");
 }
 sealed class NeverReply:Stream
 {
  public override bool CanRead=>true;public override bool CanWrite=>false;public override bool CanSeek=>false;
  public override long Length=>throw new NotSupportedException();public override long Position{get=>throw new NotSupportedException();set=>throw new NotSupportedException();}
  public override async ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken token=default){await Task.Delay(Timeout.Infinite,token);return 0;}
  public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override void Write(byte[] b,int o,int c)=>throw new NotSupportedException();public override void Flush(){}
  public override long Seek(long n,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long n)=>throw new NotSupportedException();
 }
 static void ClientTests()
 {
  Test("capture declaration requires exact705 policy safe caps and a detached input array",()=>
  {
   var original=CaptureDeclaration();var limits=Get(original,"Limits");var policy=Items("MixedCaptureInspection",SourceIds().Select(id=>New("MixedCaptureInspection",id,0L)).ToArray());
   var copy=New("MixedCaptureDeclaration",limits,policy);policy.SetValue(New("MixedCaptureInspection","point0",99L),0);
   Need((bool)Call(original,"Matches",copy)!&&(long)Call(copy,"InspectionCopiesFor","point0")! ==0L,"caller array mutation cannot change declared policy");
   Reject(()=>Call(copy,"InspectionCopiesFor","unknown"),"exact context domain");
   Reject(()=>New("MixedCaptureDeclaration",limits,Items("MixedCaptureInspection")),"missing full policy");
   policy.SetValue(New("MixedCaptureInspection","point1",0L),0);Reject(()=>New("MixedCaptureDeclaration",limits,policy),"ordered context identity");
   policy.SetValue(New("MixedCaptureInspection","point0",-1L),0);Reject(()=>New("MixedCaptureDeclaration",limits,policy),"negative inspection count");
   var bad=New("MixedTrace+CaptureLimits",New("MixedTrace+CopyCounts",9007199254740992L,0L,0L,0L,0L),Get(limits,"Inspection"),Get(limits,"Failure"));
   policy.SetValue(New("MixedCaptureInspection","point0",0L),0);Reject(()=>New("MixedCaptureDeclaration",bad,policy),"unsafe cap");
  });
  Test("sink capture binding requires an accepted handshake and exact immutable declaration",()=>
  {
   using(var request=new MemoryStream())using(var reply=new MemoryStream(Encoding.ASCII.GetBytes(BeginReply())))
   {
    var client=Client(request,reply);RejectClient(client,()=>Call(client,"RequireCaptureDeclaration",CaptureDeclaration()));((IDisposable)client).Dispose();
   }
   using(var request=new MemoryStream())using(var reply=new MemoryStream(Encoding.ASCII.GetBytes(BeginReply())))
   {
    var client=Client(request,reply);Await(client,"BeginAsync");Call(client,"RequireCaptureDeclaration",CaptureDeclaration());
    using var wire=JsonDocument.Parse(request.ToArray());Need(wire.RootElement.GetProperty("template").GetProperty("inspectionPolicy").GetArrayLength()==705,"actual full declaration travels in begin frame");
    RejectClient(client,()=>Call(client,"RequireCaptureDeclaration",CaptureDeclaration(1)));((IDisposable)client).Dispose();
   }
  });
  Test("client accepts only the exact request-bound canonical handshake",()=>
  {
   string valid=BeginReply();
   foreach(string wire in new[]{valid.Replace("\"sequence\":0","\"sequence\":1"),valid.Replace("\"sequence\":0","\"sequence\":0,\"sequence\":0"),
    valid.Replace("\"report\":null","\"report\":{}")," "+valid,valid[..^1],"{}\n",valid.Replace("accepted","rejected")})
   {using var request=new MemoryStream();using var reply=new MemoryStream(Encoding.ASCII.GetBytes(wire));var client=Client(request,reply);RejectClient(client,()=>Await(client,"BeginAsync"));RejectClient(client,()=>Await(client,"BeginAsync"));((IDisposable)client).Dispose();}
  });
  Test("client request quotas reject before emitting an incomplete frame",()=>
  {
   foreach(bool perFrame in new[]{true,false})
   {using var request=new MemoryStream();using var reply=new MemoryStream(Encoding.ASCII.GetBytes(BeginReply()));var client=Client(request,reply,frame:perFrame?1:2000000,total:perFrame?536870912:1);RejectClient(client,()=>Await(client,"BeginAsync"));Need(request.ToArray().Length==0,"no emitted request before frame/aggregate admission");((IDisposable)client).Dispose();}
  });
  Test("client reply quotas and absent acknowledgements poison the session",()=>
  {
   for(int variant=0;variant<3;variant++)
   {using var request=new MemoryStream();using var reply=new MemoryStream(variant==2?[]:Encoding.ASCII.GetBytes(BeginReply()));var client=Client(request,reply,reply:variant==0?1:4096,replyTotal:variant==1?1:4194304);RejectClient(client,()=>Await(client,"BeginAsync"));((IDisposable)client).Dispose();}
  });
  Test("client cannot skip contexts complete early or repeat its handshake",()=>
  {
   foreach(string operation in new[]{"skip","complete","repeat"})
   {using var request=new MemoryStream();using var reply=new MemoryStream(Encoding.ASCII.GetBytes(BeginReply()));var client=Client(request,reply);Await(client,"BeginAsync");Need(!(bool)Get(client,"Completed"),"handshake alone is not full validation");RejectClient(client,()=>{if(operation=="skip")Await(client,"ValidateAsync",Template("point1"));else Await(client,operation=="complete"?"CompleteAsync":"BeginAsync");});((IDisposable)client).Dispose();}
  });
  Test("client deadline cancels a stalled acknowledgement without retry",()=>
  {using var request=new MemoryStream();using var reply=new NeverReply();var client=Client(request,reply,timeout:25);RejectClient(client,()=>Await(client,"BeginAsync"));RejectClient(client,()=>Await(client,"BeginAsync"));((IDisposable)client).Dispose();});
 }
 static void SourceTests()
 {
  Test("source factory covers all705 real menus without coefficient execution",()=>
  {
   var factory=SourceFactory();int marks=0,leaves=0,selected=0;var paths=new HashSet<string>(StringComparer.Ordinal);
   foreach(string id in SourceIds())
   {
    var menu=Describe(id);var list=((IEnumerable)Get(menu,"Marks")).Cast<object>().ToArray();marks+=list.Length;
    leaves+=((IEnumerable)Get(menu,"Leaves")).Cast<object>().Count();if(list.Length==3976)selected++;
    var template=Call(factory,"Create",id)!;var actual=((IEnumerable)Get(template,"Marks")).Cast<object>().ToArray();Need(actual.Length==list.Length,"unchanged full mark census");
    Need(paths.Add((string)Get(template,"GraphPath"))&&paths.Add((string)Get(template,"MetadataPath")),"unique graph/metadata paths");
    foreach(var mark in actual)if((bool)Get(mark,"Expanded"))Need(paths.Add((string)Get(mark,"RelativePath")),"globally unique tensor path");
   }
   Need(marks==291199&&selected==6&&leaves==20316,"complete real mark, selected Ward and leaf census");
  });
  Test("source IDs reject aliases malformed coordinates and unknown diagnostics",()=>
  {foreach(string id in new[]{"point2","point0/m10_j0","point0/m0_j35","point0/m00_j0","point0/m0_j00","point0/m0_j-1","point0/m0_j0/","diagnostic/secondjets","point0\n","../point0"})Reject(()=>Describe(id),id);});
  Test("source retention cannot weaken required expansion or alter fixed DAG-only fields",()=>
  {
   var small=SourceFactory();var large=SourceFactory(true);
   foreach(string id in new[]{"point0","point1/m0_j4","diagnostic/grade10","diagnostic/acceleration","diagnostic/secondJets"})
   {
    var menu=((IEnumerable)Get(Describe(id),"Marks")).Cast<object>().ToArray();
    var a=((IEnumerable)Get(Call(small,"Create",id)!,"Marks")).Cast<object>().ToArray();var b=((IEnumerable)Get(Call(large,"Create",id)!,"Marks")).Cast<object>().ToArray();
    for(int i=0;i<menu.Length;i++)
    {
     var required=menu[i].GetType().GetProperty("RequiredExpanded")!.GetValue(menu[i]);
     Need((bool)Get(a[i],"Expanded")==((bool?)required??false)&&(bool)Get(b[i],"Expanded")==((bool?)required??true),"mandatory and optional retention distinguished");
     if(required is true)Need((string)Get(a[i],"RelativePath")== (string)Get(b[i],"RelativePath"),"paths never renumber when optional retention changes");
    }
   }
  });
  Test("all real source templates freeze and regenerate before late leaf binding",()=>
  {
   var source=SourceFactory();var catalog=Catalog(id=>Call(source,"Create",id)!,per:2000000,total:1000000000);int admitted=0;
   Freeze(catalog,_=>admitted++);foreach(string id in SourceIds())Materialize(catalog,id,t=>
   {
    // Genuine SHA of a manufactured empty tensor; NEVER source authentication.
    return Items("MixedTrace+LeafSpec",((IEnumerable)Get(t,"Leaves")).Cast<object>().Select(l=>New("MixedTrace+LeafSpec",Get(l,"Id"),Get(l,"Degree"),Get(l,"Source"),EmptyHash)).ToArray());
   },_=>admitted++);
   Need(admitted==1410&&(int)Get(Usage(catalog),"BoundContexts")==705,"two complete independently admitted passes");
  });
  Test("resource profiles are complete explicit and copied before use",()=>
  {
   var budgets=Items("MixedSourceTemplateBudget",SourceIds().Select(id=>SourceBudget(id)).ToArray());var source=SourceFactory(budgets:budgets);
   budgets.SetValue(SourceBudget("point1"),0);Need((string)Get(Call(source,"Create","point0")!,"Id")=="point0","caller array mutation cannot change profile");
   Reject(()=>SourceFactory(budgets:budgets),"duplicate point and missing point");
   Reject(()=>SourceFactory(budgets:Items("MixedSourceTemplateBudget")),"missing budgets");
   var wrong=Items("MixedSourceTemplateBudget",SourceIds().Select(id=>SourceBudget(id)).ToArray());wrong.SetValue(SourceBudget("point0/m0_j0",1),1);
   Reject(()=>SourceFactory(budgets:wrong),"nonpoint checkpoint forbidden");
   wrong.SetValue(SourceBudget("point0/m0_j0"),1);wrong.SetValue(SourceBudget("point0",0),0);Reject(()=>SourceFactory(budgets:wrong),"nonpositive checkpoint");
  });
  Test("source menu callback copies cannot mutate later generated templates",()=>
  {
   var first=Describe("point0/m0_j4");var entries=(Array)Get(Get(first,"Callbacks"),"Callbacks");entries.SetValue(null,0);
   Need(((Array)Get(Get(Describe("point0/m0_j4"),"Callbacks"),"Callbacks")).GetValue(0) is not null,"fresh callback arrays");
   Need(((IList)Get(first,"Leaves")).IsReadOnly&&((IList)Get(first,"Marks")).IsReadOnly,"read-only declaration lists");
  });
 }
 static void Main(string[] args)
 {
  if(args.SequenceEqual(new[]{"--context-envelope"})){Console.WriteLine(JsonSerializer.Serialize(ContextEnvelope(),Json));return;}
  if(args.SequenceEqual(new[]{"--point-completion-envelope"})){Console.WriteLine(JsonSerializer.Serialize(PointCompletionEnvelope(),Json));return;}
  if(args.SequenceEqual(new[]{"--source-structures"})){SourceSummaries();return;}
  if(args.SequenceEqual(new[]{"--diagnostic-menus"})){DiagnosticMenus();return;}
  if(args.SequenceEqual(new[]{"--geometry-wire-fixtures"})){GeometryWireFixtures();return;}
  if(args.Length==2&&args[0]=="--geometry-wire-guarded"){GeometryWireGuarded(args[1]);return;}
  if(args.SequenceEqual(new[]{"--source-templates"})){SourceTemplates();return;}
  if(args.Length==2&&args[0]=="--preflight-client")
  {try{PreflightClient(args[1]).GetAwaiter().GetResult();}catch(Exception e){Console.Error.WriteLine(e.Message);Environment.ExitCode=1;}return;}
  if(args.Length==1&&args[0]=="--source-preparation")
  {try{SourcePreparationPrefix();}catch(Exception e){Console.Error.WriteLine(e.Message);Environment.ExitCode=1;}return;}
  if(args.Length==3&&args[0]=="--preflight-client-tail")
  {
   Need(args[2] is "exit7" or "stdout" or "linger","closed metadata-only process-tail fixture");
   try
   {
    PreflightClient(args[1]).GetAwaiter().GetResult();
    if(args[2]=="exit7")Environment.ExitCode=7;
    else if(args[2]=="stdout")Console.WriteLine("unexpected metadata fixture stdout");
    else Thread.Sleep(Timeout.Infinite);
   }
   catch(Exception e){Console.Error.WriteLine(e.Message);Environment.ExitCode=1;}return;
  }
  Need(args.Length==0,"closed manufactured-test argument menu");
  Test("exact705 structure freezes without any coefficient hash or leaf binding",()=>
  {
   int calls=0,admitted=0,validated=0;var c=Catalog(id=>{Need(admitted==calls+1,"admission before factory");calls++;return Template(id);});
   Freeze(c,_=>admitted++,t=>{validated++;Need(Get(t,"Leaves") is IEnumerable,"leaf declarations only");});
   var usage=Usage(c);Need(calls==705&&admitted==705&&validated==705&&(int)Get(usage,"FrozenContexts")==705&&(int)Get(usage,"BoundContexts")==0,"complete prepass only");
   Need((bool)Get(usage,"Frozen")&&!(bool)Get(usage,"Failed"),"healthy complete freeze");
   Need(T("MixedSinkLeafDeclaration").GetProperty("Sha256") is null,"no placeholder hash field");
  });
  Test("all705 contexts bind exactly once after freeze",()=>
  {
   var c=Catalog();Freeze(c);int bindings=0;
   foreach(string id in (IEnumerable)Get(c,"ContextIds")){var p=Materialize(c,id,t=>{bindings++;return Leaves(t);});Need((string)Get(p,"Id")==id,"same bound context");}
   var u=Usage(c);Need(bindings==705&&(int)Get(u,"FactoryCalls")==1410&&(int)Get(u,"BoundContexts")==705,"complete two-pass census");
   Reject(()=>Materialize(c,"point0"),"duplicate bind");Need((bool)Get(Usage(c),"Failed"),"no retry after duplicate");
  });
  Test("materialization before freeze and unknown IDs poison before factories",()=>
  {
   int calls=0;var c=Catalog(id=>{calls++;return Template(id);});Reject(()=>Materialize(c,"point0"),"pre-freeze bind");Need(calls==0,"factory untouched");Reject(()=>Freeze(c),"poison cannot be repaired");
   var d=Catalog();Freeze(d);Reject(()=>Materialize(d,"point2"),"unknown context");Need((int)Get(Usage(d),"FactoryCalls")==705,"no unknown factory");
  });
  Test("changed menus paths resources roles or leaf identities fail before binding",()=>
  {
   foreach(string change in new[]{"mark","expansion","callback","path","resources","role","degree","source"})
   {string mode="";var c=Catalog(id=>Template(id,mode));Freeze(c);mode=change;bool called=false;Reject(()=>Materialize(c,"point0",t=>{called=true;return Leaves(t);}),change);Need(!called,"structural mismatch precedes binder");Reject(()=>Materialize(c,"point1"),"poison after mismatch");}
  });
  Test("only valid matching late leaf SHA may change from the template",()=>
  {
   foreach(var variant in new[]{"bad-hash","wrong-id","wrong-source","wrong-degree","missing","extra"})
   {
    var c=Catalog();Freeze(c);object Bind(object t)=>variant switch{
     "bad-hash"=>Leaves(t,"not-a-hash"),"wrong-id"=>Leaves(t,id:"other"),"wrong-source"=>Leaves(t,source:"other"),"wrong-degree"=>Leaves(t,degree:2),
     "missing"=>Items("MixedTrace+LeafSpec"),_=>Items("MixedTrace+LeafSpec",((Array)Leaves(t)).GetValue(0)!,((Array)Leaves(t)).GetValue(0)!)};
    Reject(()=>Materialize(c,"point0",Bind),variant);Need((int)Get(Usage(c),"BoundContexts")==0,"no partial bound plan");
   }
  });
  Test("explicit construction admission failure precedes factory allocation",()=>
  {int calls=0;var c=Catalog(id=>{calls++;return Template(id);});Reject(()=>Freeze(c,_=>throw new InvalidOperationException("manufactured cap")),"construction cap");Need(calls==0,"factory never invoked");});
  Test("per-template and cumulative serialization limits fail closed",()=>
  {foreach(var c in new[]{Catalog(per:1),Catalog(total:1)}){Reject(()=>Freeze(c),"serialization quota");Need((bool)Get(Usage(c),"Failed")&&(int)Get(Usage(c),"FrozenContexts")==0,"no partial freeze");}});
  Test("preflight validator failure cannot leave a usable partial catalog",()=>
  {var c=Catalog();Reject(()=>Freeze(c,validate:t=>{if((string)Get(t,"Id")=="point0/m0_j0")throw new InvalidOperationException("manufactured semantic failure");}),"validator failure");Reject(()=>Materialize(c,"point0"),"partial prepass cannot bind");});
  Test("swallowed reentrant freeze or binding failure poisons outer operation",()=>
  {
   object? c=null;c=Catalog();Reject(()=>Freeze(c,validate:_=>Reject(()=>Materialize(c,"point0"),"nested operation")),"outer freeze poisoned");
   var d=Catalog();Freeze(d);Reject(()=>Materialize(d,"point0",t=>{Reject(()=>Materialize(d,"point1"),"nested bind");return Leaves(t);}),"outer bind poisoned");Need((int)Get(Usage(d),"BoundContexts")==0,"no leaked successful binding");
  });
  Test("factory cannot silently omit or substitute a context",()=>
  {var c=Catalog(_=>Template("point0"));Reject(()=>Freeze(c),"factory wrong ID");Need((int)Get(Usage(c),"FrozenContexts")==1,"second ID mismatch rejects immediately");});
  Test("returned ID and structural collections cannot be mutated",()=>
  {
   var c=Catalog();var ids=(IList)Get(c,"ContextIds");Need(ids.IsReadOnly&&ids.Count==705,"read-only complete IDs");
   var t=Template("point0");Need(((IList)Get(t,"Leaves")).IsReadOnly&&((IList)Get(t,"Marks")).IsReadOnly,"read-only structure collections");
   var first=Get(t,"Callbacks");((Array)Get(first,"Callbacks")).SetValue(null,0);Need(((Array)Get(Get(t,"Callbacks"),"Callbacks")).GetValue(0) is not null,"callback copies do not alter template");
  });
  SourceTests();
  ClientTests();
  CoordinatorTests();
  SourcePreparationClientTests();
  SourcePreparationCoordinatorTests();
  GeometryWireTests();
  SealTests();
  PointSealTests();
  Console.WriteLine($"{tests} metadata-only template-catalog/source-factory tests passed; no source geometry, coefficients or scientific sink executed.");
 }
 // Synthetic acknowledgement peer ONLY for coordinator lifecycle tests. It
 // deliberately does not validate scientific/template semantics. The real705
 // integration above uses the independent production JS validator instead.
 sealed class AckReplies:Stream
 {
  readonly Queue<byte> pending=new();public bool Closed;public bool End;
  public void Queue(byte[] bytes){foreach(byte b in bytes)pending.Enqueue(b);}
  public override ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken token=default)
  {
   if(Closed)throw new ObjectDisposedException(nameof(AckReplies));token.ThrowIfCancellationRequested();
   Need(pending.Count>0||End,"synthetic peer must queue reply before read");int n=Math.Min(buffer.Length,pending.Count);
   for(int i=0;i<n;i++)buffer.Span[i]=pending.Dequeue();return ValueTask.FromResult(n);
  }
  public override bool CanRead=>!Closed;public override bool CanWrite=>false;public override bool CanSeek=>false;
  public override long Length=>throw new NotSupportedException();public override long Position{get=>throw new NotSupportedException();set=>throw new NotSupportedException();}
  public override int Read(byte[] b,int o,int n)=>throw new NotSupportedException();public override void Write(byte[] b,int o,int n)=>throw new NotSupportedException();
  public override void Flush(){}public override long Seek(long n,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long n)=>throw new NotSupportedException();
  protected override void Dispose(bool disposing){Closed=true;base.Dispose(disposing);}
 }
 sealed class AckRequests(AckReplies replies):MemoryStream
 {
  public int Frames;public int RejectSequence=-1;public bool TrailingFinish;
  public override ValueTask WriteAsync(ReadOnlyMemory<byte> bytes,CancellationToken token=default)
  {
   token.ThrowIfCancellationRequested();using var document=JsonDocument.Parse(bytes);var root=document.RootElement;
   int sequence=root.GetProperty("sequence").GetInt32();string operation=root.GetProperty("operation").GetString()!;object? report=null;
   var scope=new{structureOnly=true,numericalReplayComplete=false,sourceAuthenticityEstablished=false,resourceSufficiencyProved=false,totalProcessMemoryProved=false,scientificExecutionAuthorized=false};
   if(operation=="validate")
   {
    var template=root.GetProperty("template");report=new{contextId=template.GetProperty("id").GetString(),marks=template.GetProperty("marks").GetArrayLength(),
     leaves=template.GetProperty("leaves").GetArrayLength(),callbacks=template.GetProperty("callbacks").GetProperty("callbacks").GetArrayLength(),scope};
   }
   else if(operation=="finish")report=new{contexts=705,marks=291199,leaves=20316,callbacks=940365,scope};
   string hash=Convert.ToHexString(SHA256.HashData(bytes.Span)).ToLowerInvariant();
   string wire=JsonSerializer.Serialize(new{schema="phase627-template-preflight-reply-v2",sequence,operation,requestSha256=hash,status=sequence==RejectSequence?"rejected":"accepted",report},Json)+"\n";
   if(operation=="finish"&&TrailingFinish)wire+="x";
   replies.Queue(Encoding.ASCII.GetBytes(wire));Frames++;return ValueTask.CompletedTask;
  }
  protected override void Dispose(bool disposing){replies.End=true;base.Dispose(disposing);}
 }
 sealed class CoordinatorFixture:IDisposable
 {
  public readonly AckReplies Replies=new();public readonly AckRequests Requests;
  public readonly object Client,Catalog,Coordinator;public int Admissions,Factories;
  public Action<string>? OnAdmission;public Func<string,object>? OnFactory;
  public CoordinatorFixture()
  {
   Requests=new(Replies);Client=Program.Client(Requests,Replies);
   Catalog=Program.Catalog(id=>{Need(Admissions==Factories+1,"admitted before manufactured factory");Factories++;return OnFactory?.Invoke(id)??Template(id);});
   Coordinator=New("MixedTemplatePreflightCoordinator",Client,Catalog,(Action<string>)(id=>{OnAdmission?.Invoke(id);Admissions++;}));
  }
  public void Start(){Await(Coordinator,"BeginAsync");Call(Coordinator,"ValidateCaptureDeclaration",CaptureDeclaration());}
  public void Admit(string id)=>Call(Coordinator,"CheckTemplateConstructionResources",id);
  public void Validate(object template)=>Call(Coordinator,"ValidateContextTemplate",template);
  public void Freeze(Action<object>? validate=null)=>Program.Freeze(Catalog,Admit,validate??Validate);
  public void Finish(){Freeze();Call(Coordinator,"CompleteTemplateValidation");}
  public void Reject(Action action)
  {
   Program.Reject(action,"coordinator rejection");Need((bool)Get(Coordinator,"Failed")&&!(bool)Get(Coordinator,"Completed"),"sticky failed coordinator");
  }
  public void Dispose()=>((IDisposable)Coordinator).Dispose();
 }
 static void CoordinatorTests()
 {
  Test("coordinator joins sole full705 catalog freeze and terminal707 exchange with zero bindings",()=>
  {
   using var f=new CoordinatorFixture();f.Start();f.Freeze();Need(!(bool)Get(f.Coordinator,"Completed"),"full freeze is not terminal acceptance");
   Call(f.Coordinator,"CompleteTemplateValidation");var state=Usage(f.Catalog);
   Need((bool)Get(f.Coordinator,"Completed")&&!(bool)Get(f.Coordinator,"Failed")&&f.Admissions==705&&f.Factories==705&&f.Requests.Frames==707,"complete exact metadata transaction");
   Need((int)Get(state,"BoundContexts")==0&&(int)Get(state,"FrozenContexts")==705,"no leaf binding during preflight");
  });
  Test("coordinator refuses construction and validation before handshake or capture acceptance",()=>
  {
   for(int mode=0;mode<4;mode++)
   {
    using var f=new CoordinatorFixture();if(mode>=2)Await(f.Coordinator,"BeginAsync");
    if(mode%2==0)f.Reject(()=>f.Admit("point0"));else f.Reject(()=>f.Validate(Template("point0")));
    Need(f.Factories==0,"no unadmitted factory");
   }
  });
  Test("coordinator rejects mismatched repeated and pre-handshake capture declarations",()=>
  {
   for(int mode=0;mode<3;mode++)
   {
    using var f=new CoordinatorFixture();if(mode>0)Await(f.Coordinator,"BeginAsync");if(mode==2)Call(f.Coordinator,"ValidateCaptureDeclaration",CaptureDeclaration());
    f.Reject(()=>Call(f.Coordinator,"ValidateCaptureDeclaration",CaptureDeclaration(mode==1?1:0)));
   }
  });
  Test("coordinator rejects skipped unknown repeated admissions and validation without admission",()=>
  {
   foreach(string id in new[]{"point1","unknown"}){using var f=new CoordinatorFixture();f.Start();f.Reject(()=>f.Admit(id));}
   using(var f=new CoordinatorFixture()){f.Start();f.Admit("point0");f.Reject(()=>f.Admit("point0"));}
   using(var f=new CoordinatorFixture()){f.Start();f.Reject(()=>f.Validate(Template("point0")));}
  });
  Test("catalog object identity forbids factory self-validation and same-ID alternate objects",()=>
  {
   using(var f=new CoordinatorFixture())
   {
    f.Start();f.OnFactory=id=>{var alternate=Template(id);f.Validate(alternate);return Template(id,"source");};
    f.Reject(()=>f.Freeze(_=>{}));Need(f.Requests.Frames==1,"factory cannot submit alternate before catalog callback");
   }
   using(var f=new CoordinatorFixture())
   {
    f.Start();f.Reject(()=>f.Freeze(t=>f.Validate(Template((string)Get(t,"Id")))));
    Need(f.Requests.Frames==1,"equal content or ID is not actual callback identity");
   }
   var catalog=Program.Catalog();Program.Reject(()=>Call(catalog,"RequireCurrentValidation",Template("point0")),"outside callback phase");
  });
  Test("early and repeated completion cannot reuse an accepted preflight",()=>
  {
   using(var f=new CoordinatorFixture()){f.Start();f.Reject(()=>Call(f.Coordinator,"CompleteTemplateValidation"));}
   using(var f=new CoordinatorFixture()){f.Start();f.Finish();f.Reject(()=>Call(f.Coordinator,"CompleteTemplateValidation"));}
  });
  Test("construction callback failure swallowed reentry and concurrent misuse poison preflight",()=>
  {
   for(int mode=0;mode<3;mode++)
   {
    using var f=new CoordinatorFixture();f.Start();f.OnAdmission=id=>
    {
     if(mode==0)throw new InvalidOperationException("manufactured construction cap");
     if(mode==1)Program.Reject(()=>f.Admit(id),"swallowed nested admission");
     else Task.Run(()=>Program.Reject(()=>f.Admit(id),"concurrent nested admission")).GetAwaiter().GetResult();
    };
    f.Reject(()=>f.Freeze());Need(f.Factories==0,"failed admission cannot reach factory");
   }
  });
  Test("factory failure poisons actual catalog and cannot become coordinator completion",()=>
  {
   using var f=new CoordinatorFixture();f.Start();f.OnFactory=_=>throw new InvalidOperationException("manufactured factory failure");
   f.Reject(()=>f.Freeze());f.Reject(()=>Call(f.Coordinator,"CompleteTemplateValidation"));Need(f.Requests.Frames==1,"no validation after failed factory");
  });
  Test("failed per-template transport poisons coordinator and catalog",()=>
  {
   using var f=new CoordinatorFixture();f.Requests.RejectSequence=1;f.Start();f.Reject(()=>f.Freeze());
   Need((bool)Get(Usage(f.Catalog),"Failed")&&f.Factories==1,"stop at first rejected template");
  });
  Test("terminal rejection and trailing bytes cannot certify a frozen catalog",()=>
  {
   for(int mode=0;mode<2;mode++)
   {
    using var f=new CoordinatorFixture();if(mode==0)f.Requests.RejectSequence=706;else f.Requests.TrailingFinish=true;
    f.Start();f.Freeze();f.Reject(()=>Call(f.Coordinator,"CompleteTemplateValidation"));
    Need((int)Get(Usage(f.Catalog),"BoundContexts")==0,"terminal failure cannot bind leaves");
   }
  });
  Test("disposed coordinator or externally poisoned client cannot authorize construction",()=>
  {
   for(int mode=0;mode<3;mode++)
   {
    using var f=new CoordinatorFixture();f.Start();if(mode==2)f.Finish();
    if(mode==0)f.Dispose();else ((IDisposable)f.Client).Dispose();
    Need(!(bool)Get(f.Coordinator,"Completed"),"no stale terminal receipt");f.Reject(()=>f.Admit("point0"));
   }
  });
  Test("second construction pass remains admitted before each regeneration without extra wire messages",()=>
  {
   using var f=new CoordinatorFixture();f.Start();f.Finish();
   foreach(string id in SourceIds())Materialize(f.Catalog,id,admit:context=>
   {f.Admit(context);Need(!(bool)Get(f.Coordinator,"Completed"),"pending materialization not a complete boundary");});
   Need(f.Admissions==1410&&f.Factories==1410&&f.Requests.Frames==707&&(int)Get(Usage(f.Catalog),"BoundContexts")==705&&
    (bool)Get(f.Coordinator,"Completed"),"two exact admitted passes; manufactured leaf hashes only");
   f.Reject(()=>f.Admit("point0"));
  });
  Test("repeated second admission and external catalog mutation cannot hide behind completion",()=>
  {
   using(var f=new CoordinatorFixture()){f.Start();f.Finish();f.Admit("point0");f.Reject(()=>f.Admit("point0"));}
   using(var f=new CoordinatorFixture())
   {
    f.Start();f.Finish();Materialize(f.Catalog,"point0",admit:_=>f.Admissions++);
    Need(!(bool)Get(f.Coordinator,"Completed"),"external binding invalidates coordinator boundary");f.Reject(()=>f.Admit("point0"));
   }
  });
 }
 static object Emitted(string path="graph.json",long bytes=3,string? hash=null)=>New("MixedEmittedArtifact",path,bytes,hash??EmptyHash);
 static object PointSeal()=>New("MixedPointEvidenceSeal","point0","point0/graph.json","point0/background-checkpoint.json","point0/metadata.json");
 static object BackgroundCheckpoint(string context="point0")=>New("MixedPointBackgroundCheckpoint",context,Emitted("point0/graph.json"),Emitted("point0/background-checkpoint.json"));
 static Delegate PointWriter(Func<object,object> action)
 {var p=Expression.Parameter(T("MixedPointBackgroundCheckpoint"));return Expression.Lambda(typeof(Func<,>).MakeGenericType(p.Type,T("MixedEmittedArtifact")),Expression.Convert(Expression.Invoke(Expression.Constant(action),Expression.Convert(p,typeof(object))),T("MixedEmittedArtifact")),p).Compile();}
 static Delegate PointValidator(Action<object> action)
 {var p=Expression.Parameter(T("MixedPointCompletionCheckpoint"));return Expression.Lambda(typeof(Action<>).MakeGenericType(p.Type),Expression.Invoke(Expression.Constant(action),Expression.Convert(p,typeof(object))),p).Compile();}
 static object? PointComplete(object seal,object? background=null,Func<object,object>? write=null,Action<object>? validate=null)
  =>Call(seal,"Complete",background??BackgroundCheckpoint(),PointWriter(write??(_=>Emitted("point0/metadata.json"))),PointValidator(validate??(_=>{})));
 static object PointCompletionEnvelope()
 {
  var graph=Emitted("point0/graph.json",3,Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes("{}\n"))).ToLowerInvariant());
  var metadata=new Dictionary<string,object>{{"check/manufactured",true}};
  var background=new{schema="phase627-point-background-checkpoint-v1",context="point0",status="background-sealed",pointTraversalComplete=false,graph,metadata};
  var bytes=Encoding.ASCII.GetBytes(JsonSerializer.Serialize(background,Json)+"\n");
  var pin=Emitted("point0/background-checkpoint.json",bytes.Length,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
  return new{background,final=T("MixedPointEvidenceSeal").GetMethod("Envelope")!.Invoke(null,["point0",pin,metadata])};
 }
 static void PointSealTests()
 {
  Test("point final seal preserves background and final metadata before mandatory independent callback",()=>
  {
   var seal=PointSeal();var background=BackgroundCheckpoint();var final=Emitted("point0/metadata.json");var events=new List<string>();
   PointComplete(seal,background,b=>{Need(ReferenceEquals(b,background),"same accepted background reference");events.Add("write");return final;},c=>
   {
    Need(events.SequenceEqual(new[]{"write"})&&!(bool)Get(seal,"ValidationReturned"),"final evidence exists before acceptance");
    Need(ReferenceEquals(Get(c,"Background"),background)&&ReferenceEquals(Get(c,"Metadata"),final),"actual artifact identities");events.Add("validate");
   });
   Need((bool)Get(seal,"ValidationReturned")&&!(bool)Get(seal,"Failed")&&events.SequenceEqual(new[]{"write","validate"}),"one complete terminal sequence");
  });
  Test("invalid point background identity rejects before final write",()=>
  {
   foreach(var background in new[]{BackgroundCheckpoint("point1"),New("MixedPointBackgroundCheckpoint","point0",Emitted("wrong.json"),Emitted("point0/background-checkpoint.json")),
    New("MixedPointBackgroundCheckpoint","point0",Emitted("point0/graph.json"),Emitted("point0/background-checkpoint.json",1))})
   {var seal=PointSeal();int writes=0;Reject(()=>PointComplete(seal,background,_=>{writes++;return Emitted("point0/metadata.json");}),"invalid background");Need(writes==0,"no write on wrong background");}
  });
  Test("point final write or independent rejection cannot complete or retry",()=>
  {
   for(int mode=0;mode<2;mode++)
   {
    var seal=PointSeal();int writes=0,validations=0;
    Reject(()=>PointComplete(seal,write:_=>{writes++;if(mode==0)throw new InvalidOperationException("write failed");return Emitted("point0/metadata.json");},
     validate:_=>{validations++;throw new InvalidOperationException("child replay missing");}),"failed terminal step");
    Reject(()=>PointComplete(seal),"no retry");Need(writes==1&&validations==mode&&(bool)Get(seal,"Failed")&&!(bool)Get(seal,"ValidationReturned"),"retain evidence without acceptance");
   }
  });
  Test("point final swallowed writer or validator reentry poisons outer completion",()=>
  {
   for(int mode=0;mode<2;mode++)
   {
    var seal=PointSeal();int validations=0;Reject(()=>PointComplete(seal,write:_=>
    {if(mode==0)Reject(()=>PointComplete(seal),"nested writer");return Emitted("point0/metadata.json");},validate:_=>
    {validations++;if(mode==1)Reject(()=>PointComplete(seal),"nested validator");}),"sticky outer failure");
    Need(validations==mode&&!(bool)Get(seal,"ValidationReturned"),"no acceptance after swallowed reentry");
   }
  });
  Test("wrong final pin and repeated successful seal cannot preserve acceptance",()=>
  {
   foreach(var bad in new[]{Emitted("wrong.json"),Emitted("point0/metadata.json",1),Emitted("point0/metadata.json",3,new string('X',64))})
   {var seal=PointSeal();int validated=0;Reject(()=>PointComplete(seal,write:_=>bad,validate:_=>validated++),"bad final pin");Need(validated==0,"bad artifact before validator");}
   var complete=PointSeal();PointComplete(complete);Reject(()=>PointComplete(complete),"repeat terminal seal");Need(!(bool)Get(complete,"ValidationReturned"),"misuse revokes local acceptance");
  });
  Test("point final envelope is explicitly producer-only and never claims independent traversal acceptance",()=>
  {
   using var json=JsonDocument.Parse(JsonSerializer.Serialize(PointCompletionEnvelope(),Json));var final=json.RootElement.GetProperty("final");
   Need(final.EnumerateObject().Select(p=>p.Name).SequenceEqual(new[]{"schema","context","status","independentValidationComplete","backgroundCheckpoint","metadata"}),"exact terminal envelope fields");
   Need(final.GetProperty("schema").GetString()=="phase627-point-computational-completion-v1"&&final.GetProperty("status").GetString()=="producer-complete"&&
    !final.GetProperty("independentValidationComplete").GetBoolean(),"producer does not certify itself");
  });
 }
 static Delegate ArtifactWriter(Func<object> f)=>Expression.Lambda(typeof(Func<>).MakeGenericType(T("MixedEmittedArtifact")),Expression.Convert(Expression.Invoke(Expression.Constant(f)),T("MixedEmittedArtifact"))).Compile();
 static Delegate MetadataWriter(Func<object,object> f)
 {var p=Expression.Parameter(T("MixedEmittedArtifact"));return Expression.Lambda(typeof(Func<,>).MakeGenericType(p.Type,p.Type),Expression.Convert(Expression.Invoke(Expression.Constant(f),Expression.Convert(p,typeof(object))),p.Type),p).Compile();}
 static Delegate CheckpointValidator(Action<object> f)
 {var p=Expression.Parameter(T("MixedContextComputationalCheckpoint"));return Expression.Lambda(typeof(Action<>).MakeGenericType(p.Type),Expression.Invoke(Expression.Constant(f),Expression.Convert(p,typeof(object))),p).Compile();}
 static object Seal()=>New("MixedContextEvidenceSeal","point0/m0_j0","graph.json","metadata.json");
 static object? SealComplete(object seal,Func<object>? graph=null,Func<object,object>? metadata=null,Action<object>? validate=null)=>Call(seal,"Complete",ArtifactWriter(graph??(()=>Emitted())),MetadataWriter(metadata??(_=>Emitted("metadata.json"))),CheckpointValidator(validate??(_=>{})));
 static object ContextEnvelope()
 {
  var graph=new{schemaVersion="phase627-typed-mixed-dag-v1",leaves=Array.Empty<object>(),nodes=Array.Empty<object>(),marks=Array.Empty<object>()};
  var bytes=Encoding.ASCII.GetBytes(JsonSerializer.Serialize(graph,Json)+"\n");
  var pin=Emitted("graph.json",bytes.Length,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
  return T("MixedContextEvidenceSeal").GetMethod("Envelope")!.Invoke(null,["diagnostic/secondJets",pin,new Dictionary<string,object>{{"check/tiny",true}}])!;
 }
 static void SealTests()
 {
  Test("computational seal preserves graph then metadata before synchronous independent validation",()=>
  {
   var seal=Seal();var events=new List<string>();var graph=Emitted();var metadata=Emitted("metadata.json");
   var checkpoint=SealComplete(seal,()=>{events.Add("graph");return graph;},g=>{Need(ReferenceEquals(g,graph),"metadata links actual graph pin");events.Add("metadata");return metadata;},c=>
   {Need(events.SequenceEqual(new[]{"graph","metadata"}),"both artifacts already returned");Need(!(bool)Get(seal,"ValidationReturned"),"validation has not returned yet");Need(ReferenceEquals(Get(c,"Graph"),graph)&&ReferenceEquals(Get(c,"Metadata"),metadata),"exact emitted pins");events.Add("validate");});
   Need((string)Get(checkpoint!,"Context")=="point0/m0_j0"&&(bool)Get(seal,"ValidationReturned")&&!(bool)Get(seal,"Failed"),"healthy returned validation");
   Need(events.SequenceEqual(new[]{"graph","metadata","validate"}),"complete one-shot order");
  });
  Test("graph recording failure cannot write metadata validate or retry",()=>
  {
   var seal=Seal();int later=0;Reject(()=>SealComplete(seal,()=>throw new InvalidOperationException("graph failed"),g=>{later++;return Emitted("metadata.json");},c=>later++),"graph failure");
   Reject(()=>SealComplete(seal),"no retry");Need(later==0&&(bool)Get(seal,"Failed")&&!(bool)Get(seal,"ValidationReturned"),"no completion after graph failure");
  });
  Test("metadata recording failure preserves prior graph and prevents validation",()=>
  {
   var seal=Seal();var events=new List<string>();Reject(()=>SealComplete(seal,()=>{events.Add("graph");return Emitted();},g=>{events.Add("metadata");throw new InvalidOperationException("metadata failed");},c=>events.Add("validate")),"metadata failure");
   Need(events.SequenceEqual(new[]{"graph","metadata"})&&(bool)Get(seal,"Failed"),"no retry deletion or subsequent validator");
  });
  Test("independent validation failure leaves both computational artifacts without acceptance",()=>
  {
   var seal=Seal();int recorded=0;Reject(()=>SealComplete(seal,()=>{recorded++;return Emitted();},g=>{recorded++;return Emitted("metadata.json");},c=>throw new InvalidOperationException("independent mismatch")),"validator failure");
   Need(recorded==2&&(bool)Get(seal,"Failed")&&!(bool)Get(seal,"ValidationReturned"),"recorded artifacts are not acceptance");Reject(()=>SealComplete(seal),"no repaired seal");
  });
  Test("swallowed reentry at any recording or validation stage poisons completion",()=>
  {
   for(int stage=0;stage<3;stage++)
   {
    var seal=Seal();int at=0;void Visit(){if(at++==stage)Reject(()=>SealComplete(seal),"nested seal");}
    Reject(()=>SealComplete(seal,()=>{Visit();return Emitted();},g=>{Visit();return Emitted("metadata.json");},c=>Visit()),"outer poison");
    Need((bool)Get(seal,"Failed")&&!(bool)Get(seal,"ValidationReturned")&&at==stage+1,"stop immediately after poisoned callback");
   }
  });
  Test("wrong paths sizes or hashes cannot reach computational validation",()=>
  {
   foreach(var bad in new[]{Emitted("wrong.json"),Emitted(bytes:1),Emitted(hash:new string('X',64))})
   {var seal=Seal();int later=0;Reject(()=>SealComplete(seal,()=>bad,g=>{later++;return Emitted("metadata.json");},c=>later++),"invalid graph pin");Need(later==0,"invalid pin before next stage");}
   var other=Seal();Reject(()=>SealComplete(other,metadata:g=>Emitted()),"metadata pin cannot name graph");
   var complete=Seal();SealComplete(complete);Reject(()=>SealComplete(complete),"no second completion");Need(!(bool)Get(complete,"ValidationReturned"),"closed misuse poisons prior state");
  });
  Test("actual computational envelope reports producer completion and explicitly denies independent validation",()=>
  {
   var options=(JsonSerializerOptions)T("MixedTrace").GetField("JsonOptions")!.GetValue(null)!;
   using var json=JsonDocument.Parse(JsonSerializer.Serialize(ContextEnvelope(),options));var root=json.RootElement;
   Need(root.EnumerateObject().Select(p=>p.Name).SequenceEqual(new[]{"schema","context","status","independentValidationComplete","graph","metadata"}),"fixed computational envelope fields");
   Need(root.GetProperty("schema").GetString()=="phase627-context-computational-evidence-v1"&&root.GetProperty("status").GetString()=="producer-complete"&&!root.GetProperty("independentValidationComplete").GetBoolean(),"no producer self-certification");
  });
 }
}
