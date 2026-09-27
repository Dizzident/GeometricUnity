using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;

// Actual catalog/client/coordinator with a MANUFACTURED acknowledgement peer.
// No scientific sink, source geometry, retained reader or output is executed.
static partial class Program
{
 sealed class SourceCoordinatorFixture:IDisposable
 {
  public readonly SourceClientFixture Transport=new();
  public readonly object Catalog,Coordinator;
  public int Admissions,Factories,PlanAdmissions,LeafAdmissions,DiagnosticAdmissions;public Action<string>? OnAdmission;public Func<string,object>? OnFactory;
  public Action<string>? OnDiagnosticAdmission;public readonly List<string> DiagnosticStages=new();
  public Action<string,string>? OnPlanAdmission;public readonly List<(string Context,string Stage)> PlanStages=new();
  public Action<string,string,int>? OnLeafAdmission;public readonly List<(string Context,string Stage,int Units)> LeafStages=new();
  object? boundPlan;
  public SourceCoordinatorFixture(long planBytes=2000000,long totalPlanBytes=1000000000,long fingerprints=1000000,long leafBytes=1000000000)
  {
   Catalog=Program.Catalog(id=>{Need(Admissions==Factories+1,"source admission before factory");Factories++;return OnFactory?.Invoke(id)??Template(id);});
   Coordinator=New("MixedSourcePreparationCoordinator",Transport.Client,Catalog,(Action<string>)(id=>{OnAdmission?.Invoke(id);Admissions++;}),
    New("MixedContextPlanValidationLimits",planBytes,totalPlanBytes),(Action<string,string>)((id,stage)=>
    {PlanAdmissions++;PlanStages.Add((id,stage));OnPlanAdmission?.Invoke(id,stage);}),
    New("MixedLeafValidationLimits",fingerprints,leafBytes),(Action<string,string,int>)((id,stage,units)=>
    {LeafAdmissions++;LeafStages.Add((id,stage,units));OnLeafAdmission?.Invoke(id,stage,units);}),
    (Action<string>)(diagnostic=>{DiagnosticAdmissions++;DiagnosticStages.Add(diagnostic);OnDiagnosticAdmission?.Invoke(diagnostic);}));
  }
  public void Start(){Await(Coordinator,"BeginAsync");Call(Coordinator,"ValidateCaptureDeclaration",CaptureDeclaration());}
  public void Admit(string id)=>Call(Coordinator,"CheckTemplateConstructionResources",id);
  public void Validate(object template)=>Call(Coordinator,"ValidateContextTemplate",template);
  public void Freeze(){Start();Program.Freeze(Catalog,Admit,Validate);Call(Coordinator,"CompleteTemplateValidation");}
  public void Menu(string? diagnostic,object? menu)=>Call(Coordinator,"ValidateDiagnosticMenu",diagnostic,menu);
  public object Bind(string id,Func<object,object>? binder=null,bool validatePlan=true,bool validateMenu=true)
  {
   if(validateMenu&&id.StartsWith("diagnostic/",StringComparison.Ordinal))Menu(id[11..],SourceDiagnosticMenu(id[11..]));
   var plan=Materialize(Catalog,id,binder??(t=>Call(Coordinator,"BindContextLeaves",t)!),Admit);boundPlan=plan;if(validatePlan)ValidatePlan(plan);return plan;
  }
  public void ValidatePlan(object? plan)=>Call(Coordinator,"ValidateContextPlan",new object?[]{plan});
  public void Leaf(string id,string role,object? declared,object? tensor)=>Call(Coordinator,"ValidateLeaf",id,role,declared,tensor);
  public object LeafSpec(int index=0)=>((IEnumerable)Get(boundPlan!,"Leaves")).Cast<object>().ElementAt(index);
  public void ImportEmptyLeaves()
  {
   if(boundPlan is null)return;
   using var scope=SourceLeafScope();var roles=(IReadOnlyDictionary<string,string>)Get(boundPlan,"LeafRoles");
   foreach(var leaf in ((IEnumerable)Get(boundPlan,"Leaves")).Cast<object>())
   {string role=roles.Single(row=>row.Value==(string)Get(leaf,"Id")&&row.Key!="baseline/GradientPieces[0]").Key;Leaf((string)Get(boundPlan,"Id"),role,leaf,SourceTensor());}
  }
  public void Background(string id,bool importLeaves=true)
  {if(importLeaves)ImportEmptyLeaves();Call(Coordinator,"ValidatePointBackground",SourceBackground(id));boundPlan=null;}
  public void Context(string id,bool importLeaves=true)
  {if(importLeaves)ImportEmptyLeaves();Call(Coordinator,"ValidateContextEvidence",SourceContextCheckpoint(id));boundPlan=null;}
  public void Final(string id)=>Call(Coordinator,"ValidatePointCompletion",SourcePointFinal(id));
  public void AllEvidence()
  {
   foreach(string id in SourceIds())
   {
    Bind(id);if(id is "point0" or "point1")Background(id);else Context(id);
    if(id is "point0/m9_j34" or "point1/m9_j34")Final(id[..6]);
   }
  }
  public void PointEvidence()
  {
   foreach(string id in SourceIds().Take(702))
   {
    Bind(id);if(id is "point0" or "point1")Background(id);else Context(id);
    if(id is "point0/m9_j34" or "point1/m9_j34")Final(id[..6]);
   }
  }
  public void Reject(Action action)
  {
   bool refused=false;try{action();}catch(Exception error)when(error is InvalidOperationException||error.GetType()==T("MixedProducerAdmission+Refused")||error.GetType()==T("MixedTrace+EvidenceFailure")){refused=true;}
   Need(refused,"source coordinator refusal");Need((bool)Get(Coordinator,"Failed")&&!(bool)Get(Coordinator,"Completed"),"sticky source coordinator failure");
  }
  public void Dispose(){boundPlan=null;((IDisposable)Coordinator).Dispose();Transport.Dispose();}
 }
 static object SourceLeafLimits(long integerBits=4096,long records=1000000,long formatCharacters=1000000)=>New("MixedProducerAdmission+Limits",
  integerBits,16384L,1000000L,100000000L,1000000L,1000000L,1000000L,formatCharacters,1000000L,128L,1000000L,
  New("MixedProducerAdmission+TensorLimits",records,1000000L,1000000L,1000000L,1000000L,1000000L,10000000L,1000000L,10000000L,1000000L),
  New("MixedProducerAdmission+MatrixLimits",64L,100000L,1000000L,1000000L,100000L,1000000L));
 static object SourceLeafStatic(string type,string method,params object?[] args)
 {try{return T(type).GetMethod(method)!.Invoke(null,args)!;}catch(TargetInvocationException error)when(error.InnerException is not null){ExceptionDispatchInfo.Capture(error.InnerException).Throw();throw;}}
 static IDisposable SourceLeafScope(object? limits=null)=>(IDisposable)SourceLeafStatic("MixedProducerAdmission","Enter",limits??SourceLeafLimits());
 static IDictionary SourceTensor(params (int Form,int Blade,long Value)[] entries)
 {
  var result=(IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof((int,int,int,int)),T("Scalar")))!;
  foreach(var row in entries)result.Add((row.Form,row.Blade,0,0),New("Scalar",row.Value));return result;
 }
 static string SourceTensorHash(params (int Form,int Blade,long Value)[] entries)=>Hash(entries.OrderBy(row=>row.Form).ThenBy(row=>row.Blade)
  .Select(row=>new{form=row.Form,blade=row.Blade,k0=0,k1=0,real=row.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),imaginary="0"}).ToArray());
 static void SourceLeavesCleared(object coordinator)
 {
  foreach(string field in new[]{"expectedLeaves","expectedRoles","expectedTrace","acceptedLeaves"})Need(SourceCoordinatorField(coordinator,field) is null,"retired current leaf authority "+field);
  Need((int)SourceCoordinatorField(coordinator,"acceptedLeafCount")! ==0,"retired leaf census");
 }
 static object CopySourcePlan(object plan,string field="",object? replacement=null)
 {
  string[] fields=["Id","GraphPath","MetadataPath","Resources","Leaves","LeafRoles","Marks","Callbacks","PointCheckpoint"];
  return New("MixedSinkContextPlan",fields.Select(name=>name==field?replacement:Get(plan,name)).ToArray());
 }
 static object ChangeSourceRecord(object original,string field,object? replacement)
 {
  var constructor=original.GetType().GetConstructors().Single();
  return New(original.GetType().FullName!,constructor.GetParameters().Select(parameter=>parameter.Name==field?replacement:Get(original,parameter.Name!)).ToArray());
 }
 // TEST-ONLY corruption of the actual manufactured materialized object. The
 // production weak identity guard rejects surrogate plans before hashing;
 // reflecting into this private test instance independently exercises every
 // committed field without weakening that production identity guard.
 static void AlterSourcePlan(object plan,string field,object? replacement)
 {
  void Set(string name,object? value)=>plan.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(plan,value);
  if(field=="Callbacks")
  {Set("callbackId",Get(replacement!,"Id"));Set("callbackEntries",Get(replacement!,"Callbacks"));}
  else Set("<"+field+">k__BackingField",replacement);
 }
 static long SourcePlanBytes(object plan)=>JsonSerializer.SerializeToUtf8Bytes(new{schema="phase627-bound-context-plan-v1",
  id=Get(plan,"Id"),graphPath=Get(plan,"GraphPath"),metadataPath=Get(plan,"MetadataPath"),resources=Get(plan,"Resources"),
  pointCheckpoint=Get(plan,"PointCheckpoint"),leaves=Get(plan,"Leaves"),leafRoles=Get(plan,"LeafRoles"),marks=Get(plan,"Marks"),callbacks=Get(plan,"Callbacks")},Json).LongLength+1;
 static object? SourceCoordinatorField(object coordinator,string field)=>coordinator.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(coordinator);
 static void SourcePlanCleared(object coordinator,bool validated=false)
 {Need(SourceCoordinatorField(coordinator,"expectedPlan") is null&&(bool)SourceCoordinatorField(coordinator,"planValidated")! ==validated,"released pending full-plan digest and expected validation state");}
 static object SourcePlanWithTwoLeaves(string id)
 {
  var original=Template(id);
  return New("MixedSinkContextTemplate",id,Get(original,"GraphPath"),Get(original,"MetadataPath"),Get(original,"Resources"),
   Items("MixedSinkLeafDeclaration",New("MixedSinkLeafDeclaration","x",1,"manufactured-source"),New("MixedSinkLeafDeclaration","y",2,"other-source")),
   new Dictionary<string,string>{{"input/X","x"},{"other","y"}},
   Items("MixedSinkMark",New("MixedSinkMark","tiny",1,false,null),New("MixedSinkMark","other",2,false,null)),
   Get(original,"Callbacks"),Get(original,"PointCheckpoint"));
 }
 static void SourcePreparationCoordinatorTests()
 {
  Test("materialization identity is exact scoped and cleared after binding",()=>
  {
   var catalog=Catalog();Freeze(catalog);object? retained=null;
   Materialize(catalog,"point0",t=>{Call(catalog,"RequireCurrentMaterialization",t);retained=t;return Leaves(t);});
   Need((int)Get(Usage(catalog),"BoundContexts")==1,"healthy exact actual callback");
   Reject(()=>Call(catalog,"RequireCurrentMaterialization",retained),"stale callback object");
   var other=Catalog();Reject(()=>Call(other,"RequireCurrentMaterialization",Template("point0")),"outside materialization");
  });
  Test("equal-looking materialization substitutes and swallowed rejection poison catalog",()=>
  {
   var catalog=Catalog();Freeze(catalog);
   Reject(()=>Materialize(catalog,"point0",t=>
   {Reject(()=>Call(catalog,"RequireCurrentMaterialization",Template("point0")),"different exact object");return Leaves(t);}),"outer materialization poison");
   Need((int)Get(Usage(catalog),"BoundContexts")==0,"no successful binding after swallowed rejection");
  });
  Test("source coordinator owns full second-producer prepass before any binding",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();var state=Usage(f.Catalog);
   Need(f.Admissions==705&&f.Factories==705&&f.Transport.Requests.Frames==707&&
    (int)Get(state,"BoundContexts")==0&&(bool)Get(f.Coordinator,"TemplatesCompleted")&&!(bool)Get(f.Coordinator,"Completed"),"full metadata prepass not scientific completion");
  });
  Test("source coordinator routes all707 manufactured checkpoints but cannot finish without actual sink",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();f.AllEvidence();var state=Usage(f.Catalog);
   Need(f.Admissions==1410&&f.Factories==1410&&(int)Get(state,"BoundContexts")==705&&f.Transport.Requests.Frames==2122&&
    f.Transport.Requests.Events==707&&!(bool)Get(f.Coordinator,"Completed"),"full callback sequence without scientific sink");
   Need(f.PlanAdmissions==1410&&f.PlanStages.Select((row,i)=>row.Context==SourceIds()[i/2]&&row.Stage==(i%2==0?"expected-plan":"actual-plan")).All(value=>value),
    "exactly one expected and actual full-plan admission per context");
   Need(f.LeafAdmissions==1410&&f.LeafStages.Count(row=>row.Stage=="bindings"&&row.Units==1)==705&&
    f.LeafStages.Count(row=>row.Stage=="fingerprint"&&row.Units==0)==705&&(long)SourceCoordinatorField(f.Coordinator,"fingerprints")! ==705,
    "complete705 manufactured empty leaf imports before checkpoints");SourceLeavesCleared(f.Coordinator);
   f.Reject(()=>Call(f.Coordinator,"Complete",new object?[]{null}));
   Need(f.DiagnosticAdmissions==3&&f.Transport.Requests.DiagnosticMenus==3&&f.DiagnosticStages.SequenceEqual(new[]{"diagnostic/grade10","diagnostic/acceleration","diagnostic/secondJets"}),"all three diagnostic menus admitted independently before preparation");
   Need(f.Transport.Requests.Frames==2122,"no terminal request from manufactured evidence alone");
  });
  Test("source coordinator requires actual capture and full template phase",()=>
  {
   foreach(string mode in new[]{"no-handshake","capture","early-freeze","early-bind","duplicate-freeze"})
   {
    using var f=new SourceCoordinatorFixture();
    if(mode=="duplicate-freeze")f.Freeze();else if(mode!="no-handshake")f.Start();
    f.Reject(()=>
    {
     if(mode=="no-handshake")f.Admit("point0");
     else if(mode=="capture")Call(f.Coordinator,"ValidateCaptureDeclaration",CaptureDeclaration(1));
     else if(mode=="early-bind")Call(f.Coordinator,"BindContextLeaves",Template("point0"));
     else Call(f.Coordinator,"CompleteTemplateValidation");
    });
   }
  });
  Test("source coordinator cannot validate another template object or bind outside Materialize",()=>
  {
   using(var f=new SourceCoordinatorFixture())
   {
    f.Start();f.Reject(()=>Program.Freeze(f.Catalog,f.Admit,t=>f.Validate(Template((string)Get(t,"Id")))));
    Need(f.Transport.Requests.Frames==1,"no validation for equal but unowned template");
   }
   using(var f=new SourceCoordinatorFixture())
   {
    f.Freeze();f.Reject(()=>f.Bind("point0",_=>Call(f.Coordinator,"BindContextLeaves",Template("point0"))!));
    Need(f.Transport.Requests.Frames==707,"no preparation for a substituted materialization object");
   }
   using(var f=new SourceCoordinatorFixture())
   {f.Freeze();f.Admit("point0");f.Reject(()=>Call(f.Coordinator,"BindContextLeaves",Template("point0")));Need(f.Transport.Requests.Frames==707,"outside catalog materialization");}
  });
  Test("source coordinator source admission cannot skip pending checkpoint or point final",()=>
  {
   using(var f=new SourceCoordinatorFixture())
   {f.Freeze();f.Bind("point0");f.Reject(()=>f.Bind("point0/m0_j0"));Need(f.Factories==706,"no child factory before background acceptance");}
   using(var f=new SourceCoordinatorFixture())
   {
    f.Freeze();f.Bind("point0");f.Background("point0");
    for(int m=0;m<10;m++)for(int j=0;j<35;j++){string id="point0/m"+m+"_j"+j;f.Bind(id);f.Context(id);}
    f.Reject(()=>f.Bind("point1"));Need(f.Factories==1056,"no next point before point final");
   }
  });
  Test("source coordinator rejects mismatched checkpoint kinds contexts nulls and early finals",()=>
  {
   foreach(string mode in new[]{"unprepared","wrong-point","context-kind","early-final","null-background","null-context","null-final"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();if(mode!="unprepared")f.Bind("point0");
    f.Reject(()=>
    {
     if(mode=="unprepared")f.Background("point0");else if(mode=="wrong-point")f.Background("point1");
     else if(mode=="context-kind")f.Context("point0");else if(mode=="early-final")f.Final("point0");
     else Call(f.Coordinator,mode=="null-background"?"ValidatePointBackground":mode=="null-context"?"ValidateContextEvidence":"ValidatePointCompletion",new object?[]{null});
    });
   }
  });
  Test("source coordinator construction failure and swallowed reentry precede factory",()=>
  {
   foreach(bool reentry in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Start();f.OnAdmission=id=>
    {if(reentry)Reject(()=>f.Admit(id),"nested source admission");else throw new InvalidOperationException("manufactured admission refused");};
    f.Reject(()=>Program.Freeze(f.Catalog,f.Admit,f.Validate));Need(f.Factories==0,"failed prospective admission before source factory");
   }
  });
  Test("source coordinator transport failure and late catalog misuse revoke acceptance",()=>
  {
   using(var f=new SourceCoordinatorFixture())
   {
    f.Freeze();f.Transport.Requests.Change=(op,wire)=>op=="prepare"?wire.Replace("\"status\":\"accepted\"","\"status\":\"rejected\"",StringComparison.Ordinal):wire;
    f.Reject(()=>f.Bind("point0"));Need((int)Get(Usage(f.Catalog),"BoundContexts")==0,"rejected transport cannot bind");
   }
   using(var f=new SourceCoordinatorFixture())
   {f.Freeze();Reject(()=>Call(f.Catalog,"RequireCurrentMaterialization",Template("point0")),"external catalog misuse");f.Reject(()=>f.Bind("point0"));}
  });
  Test("source coordinator cannot prepare a changed regenerated factory template",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();f.OnFactory=id=>Template(id,"resources");
   f.Reject(()=>f.Bind("point0"));Need(f.Transport.Requests.Frames==707&&(int)Get(Usage(f.Catalog),"BoundContexts")==0,
    "catalog fingerprint mismatch before source preparation transport");
  });
  Test("source coordinator validates exactly one actual bound plan before its checkpoint",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();var plan=f.Bind("point0",validatePlan:false);
   Need(f.PlanAdmissions==1&&f.PlanStages[0]==("point0","expected-plan")&&SourceCoordinatorField(f.Coordinator,"expectedPlan") is not null,
    "only one compact expected digest is retained after source binding");
   f.ValidatePlan(plan);SourcePlanCleared(f.Coordinator,true);Need(f.PlanAdmissions==2,"actual plan admitted once");
   f.Background("point0");SourcePlanCleared(f.Coordinator);
   f.Bind("point0/m0_j0");f.Context("point0/m0_j0");SourcePlanCleared(f.Coordinator);
   Need(f.PlanAdmissions==4&&f.Transport.Requests.Events==2&&(int)Get(Usage(f.Catalog),"BoundContexts")==2,"released plan state permits next independently validated context");
  });
  Test("source coordinator rejects early null surrogate duplicate and stale plan validation",()=>
  {
   foreach(string mode in new[]{"early","null","surrogate","duplicate","stale"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();object? plan=null;
    if(mode!="early")plan=f.Bind("point0",validatePlan:false);
    else{var unbound=Template("point0");plan=Call(unbound,"Bind",Leaves(unbound));}
    if(mode is "duplicate" or "stale")f.ValidatePlan(plan);
    if(mode=="stale"){f.Background("point0");f.Bind("point0/m0_j0",validatePlan:false);}
    int admissions=f.PlanAdmissions;
    f.Reject(()=>f.ValidatePlan(mode=="null"?null:mode=="surrogate"?CopySourcePlan(plan!):plan));
    Need(f.PlanAdmissions==admissions,"identity or ordering failure precedes actual-plan admission");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator rejects checkpoint before actual plan validation",()=>
  {
   foreach(bool child in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();
    if(child){f.Bind("point0");f.Background("point0");f.Bind("point0/m0_j0",validatePlan:false);}
    else f.Bind("point0",validatePlan:false);
    int frames=f.Transport.Requests.Frames;f.Reject(()=>{if(child)f.Context("point0/m0_j0",importLeaves:false);else f.Background("point0",importLeaves:false);});
    Need(f.Transport.Requests.Frames==frames,"no checkpoint request for an unvalidated plan");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator checks all actual plan path identity and checkpoint fields",()=>
  {
   foreach(string field in new[]{"Id","GraphPath","MetadataPath","checkpoint-path","checkpoint-bytes","checkpoint-null"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();var plan=f.Bind("point0",validatePlan:false);
    if(field.StartsWith("checkpoint",StringComparison.Ordinal))
    {
     var checkpoint=Get(plan,"PointCheckpoint");
     AlterSourcePlan(plan,"PointCheckpoint",field=="checkpoint-null"?null:ChangeSourceRecord(checkpoint,field=="checkpoint-path"?"RelativePath":"Bytes",field=="checkpoint-path"?"point0/changed.json":99999L));
    }
    else AlterSourcePlan(plan,field,field=="Id"?"point1":"point0/changed.json");
    f.Reject(()=>f.ValidatePlan(plan));Need(f.PlanAdmissions==(field=="Id"?1:2),"full path/checkpoint comparison only after proper admission");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator checks every resource and trace ceiling in the actual bound plan",()=>
  {
   foreach(string field in new[]{"FileBytes","ContextBytes","MetadataBytes","FailureGraphBytes","Nodes","Marks","TensorRecords","RationalCharacters","TensorBytes","GraphBytes","PairVisits","SlotVisits","LiveRecords"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();var plan=f.Bind("point0",validatePlan:false);var resources=Get(plan,"Resources");
    bool traceField=field is not ("FileBytes" or "ContextBytes" or "MetadataBytes" or "FailureGraphBytes");var target=traceField?Get(resources,"Trace"):resources;
    object old=Get(target,field),replacement=old is int integer?(object)(integer+1):(long)old+1;
    var changed=ChangeSourceRecord(target,field,replacement);AlterSourcePlan(plan,"Resources",traceField?ChangeSourceRecord(resources,"Trace",changed):changed);
    f.Reject(()=>f.ValidatePlan(plan));Need(f.PlanAdmissions==2,"all resource fields included in admitted full-plan comparison");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator checks every leaf role mark and callback field in actual plan",()=>
  {
   foreach(string field in new[]{"leaf-id","leaf-degree","leaf-source","leaf-hash","roles-key","roles-value","mark-name","mark-degree","mark-expanded","mark-path",
    "callback-id","callback-category","callback-name","callback-degree","callback-expanded","callback-length"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();var plan=f.Bind("point0",validatePlan:false);
    if(field.StartsWith("leaf-",StringComparison.Ordinal))
    {
     var leaf=((IEnumerable)Get(plan,"Leaves")).Cast<object>().Single();
     string key=field switch{"leaf-id"=>"Id","leaf-degree"=>"Degree","leaf-source"=>"Source",_=>"Sha256"};
     object value=field switch{"leaf-degree"=>2,"leaf-hash"=>new string('f',64),_=>"changed"};
     AlterSourcePlan(plan,"Leaves",Items("MixedTrace+LeafSpec",ChangeSourceRecord(leaf,key,value)));
    }
    else if(field.StartsWith("roles-",StringComparison.Ordinal))AlterSourcePlan(plan,"LeafRoles",new Dictionary<string,string>{{field=="roles-key"?"changed":"input/X",field=="roles-value"?"changed":"x"}});
    else if(field.StartsWith("mark-",StringComparison.Ordinal))
    {
     var mark=((IEnumerable)Get(plan,"Marks")).Cast<object>().Single();
     string key=field switch{"mark-name"=>"Name","mark-degree"=>"Degree","mark-expanded"=>"Expanded",_=>"RelativePath"};
     object value=field switch{"mark-degree"=>2,"mark-expanded"=>true,_=>"changed"};
     AlterSourcePlan(plan,"Marks",Items("MixedSinkMark",ChangeSourceRecord(mark,key,value)));
    }
    else
    {
     var callbacks=Get(plan,"Callbacks");var entries=((IEnumerable)Get(callbacks,"Callbacks")).Cast<object>().ToArray();
     if(field=="callback-id")callbacks=ChangeSourceRecord(callbacks,"Id","changed");
     else
     {
      string key=field switch{"callback-category"=>"Category","callback-name"=>"Name","callback-degree"=>"Degree","callback-expanded"=>"Expanded",_=>"Length"};
      object value=field switch{"callback-degree"=>2,"callback-expanded"=>true,"callback-length"=>3,_=>"changed"};
      entries[0]=ChangeSourceRecord(entries[0],key,value);callbacks=ChangeSourceRecord(callbacks,"Callbacks",Items("MixedPlannedCallback",entries));
     }
     AlterSourcePlan(plan,"Callbacks",callbacks);
    }
    f.Reject(()=>f.ValidatePlan(plan));Need(f.PlanAdmissions==2,"all leaf/role/mark/callback fields included in full-plan comparison");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator rejects changed collection order or census in actual bound plan",()=>
  {
   foreach(string field in new[]{"Leaves","LeafRoles","Marks","Callbacks"})foreach(bool omit in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.OnFactory=SourcePlanWithTwoLeaves;f.Freeze();var plan=f.Bind("point0",validatePlan:false);
    if(field=="LeafRoles")
    {
     var roles=((IReadOnlyDictionary<string,string>)Get(plan,"LeafRoles")).Reverse().ToArray();
     AlterSourcePlan(plan,field,(omit?roles.Take(1):roles).ToDictionary(row=>row.Key,row=>row.Value));
    }
    else
    {
     var owner=field=="Callbacks"?Get(plan,"Callbacks"):plan;
     var rows=((IEnumerable)Get(owner,field)).Cast<object>().Reverse().ToArray();if(omit)rows=rows.Take(1).ToArray();
     var changed=Items(field=="Leaves"?"MixedTrace+LeafSpec":field=="Marks"?"MixedSinkMark":"MixedPlannedCallback",rows);
     AlterSourcePlan(plan,field,field=="Callbacks"?ChangeSourceRecord(owner,"Callbacks",changed):changed);
    }
    f.Reject(()=>f.ValidatePlan(plan));SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator detects a binder substituting a valid-looking hash after authentic ACK",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();
   var plan=f.Bind("point0",template=>{Call(f.Coordinator,"BindContextLeaves",template);return Leaves(template,new string('f',64));},validatePlan:false);
   Need((int)Get(Usage(f.Catalog),"BoundContexts")==1,"shape-valid substituted hash reached actual bound plan");
   f.Reject(()=>f.ValidatePlan(plan));Need(f.PlanAdmissions==2&&f.Transport.Requests.Events==0,"full plan hash is bound to authenticated ACK, not binder return");SourcePlanCleared(f.Coordinator);
  });
  Test("source coordinator admits expected and actual plan serialization before byte limits",()=>
  {
   long bytes;using(var measure=new SourceCoordinatorFixture()){measure.Freeze();bytes=SourcePlanBytes(measure.Bind("point0",validatePlan:false));}
   foreach(string mode in new[]{"expected-per-plan","expected-total","actual-total","actual-per-plan"})
   {
    using var f=new SourceCoordinatorFixture(planBytes:mode=="expected-per-plan"?1:mode=="actual-per-plan"?bytes:2000000,
     totalPlanBytes:mode=="expected-total"?bytes-1:mode=="actual-total"?2*bytes-1:1000000000);f.Freeze();
    if(mode.StartsWith("expected",StringComparison.Ordinal))
    {f.Reject(()=>f.Bind("point0",validatePlan:false));Need(f.PlanAdmissions==1&&(int)Get(Usage(f.Catalog),"BoundContexts")==0,"expected serialization admitted before refusal");}
    else
    {
     var plan=f.Bind("point0",validatePlan:false);if(mode=="actual-per-plan")AlterSourcePlan(plan,"GraphPath","point0/"+new string('x',2000)+".json");
     f.Reject(()=>f.ValidatePlan(plan));Need(f.PlanAdmissions==2&&(int)Get(Usage(f.Catalog),"BoundContexts")==1,"actual serialization admitted before refusal");
    }
    SourcePlanCleared(f.Coordinator);Need(f.Transport.Requests.Events==0,"budget refusal never acknowledges a checkpoint");
   }
   using var exact=new SourceCoordinatorFixture(bytes,2*bytes);exact.Freeze();exact.Bind("point0");exact.Background("point0");SourcePlanCleared(exact.Coordinator);
   exact.Reject(()=>exact.Bind("point0/m0_j0",validatePlan:false));Need(exact.PlanAdmissions==3&&exact.Transport.Requests.Events==1,"cumulative budget cannot reset after released point digest");
  });
  Test("source coordinator plan admission throw or swallowed reentry prevents acceptance",()=>
  {
   foreach(string stage in new[]{"expected-plan","actual-plan"})foreach(bool reentry in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();object? plan=null;
    f.OnPlanAdmission=(id,operation)=>
    {if(operation!=stage)return;Need(id=="point0","stage identifies pending plan");if(reentry)Reject(()=>f.ValidatePlan(plan),"nested plan validation");else throw new InvalidOperationException("manufactured plan admission refusal");};
    if(stage=="expected-plan")f.Reject(()=>f.Bind("point0",validatePlan:false));
    else{plan=f.Bind("point0",validatePlan:false);f.Reject(()=>f.ValidatePlan(plan));}
    Need(f.PlanAdmissions==(stage=="expected-plan"?1:2)&&f.Transport.Requests.Events==0,"rejected stage cannot accept plan or evidence");SourcePlanCleared(f.Coordinator);
   }
  });
  Test("source coordinator diagnostic admission constructor has no optional or null bypass",()=>
  {
   using var transport=new SourceClientFixture();var catalog=Catalog();int callbacks=0;
   var parameter=T("MixedSourcePreparationCoordinator").GetConstructors().Single().GetParameters().Last();
   Need(parameter.ParameterType==typeof(Action<string>)&&!parameter.IsOptional,"explicit mandatory final diagnostic admission callback");
   bool refused=false;
   try
   {
    _=New("MixedSourcePreparationCoordinator",transport.Client,catalog,(Action<string>)(_=>callbacks++),
     New("MixedContextPlanValidationLimits",2000000L,1000000000L),(Action<string,string>)((_,_)=>callbacks++),
     New("MixedLeafValidationLimits",1000000L,1000000000L),(Action<string,string,int>)((_,_,_)=>callbacks++),null);
   }catch(ArgumentNullException error){refused=error.ParamName=="admitDiagnosticMenu";}
   Need(refused&&callbacks==0&&transport.Requests.Frames==0,"null admission denied without callbacks or transport");
  });
  Test("source coordinator diagnostic menu admission precedes serialization and construction and retires on bind",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();f.PointEvidence();int frames=f.Transport.Requests.Frames,factories=f.Factories;
   var menu=SourceDiagnosticMenu("grade10");var tensors=Get(menu,"Tensors");
   f.OnDiagnosticAdmission=context=>
   {
    Need(context=="diagnostic/grade10"&&f.Transport.Requests.Frames==frames&&f.Factories==factories,
     "admit exact next menu before serialization transport or diagnostic factory");
    Need(SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null,"no premature menu authority during admission");
   };
   f.Transport.Requests.OnWrite=operation=>{if(operation=="diagnostic-menu")Need(f.DiagnosticAdmissions==1,"prospective admission before transport write");};
   f.Menu("grade10",menu);
   Need(f.Transport.Requests.Frames==frames+1&&f.Factories==factories&&(string)SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic")! =="diagnostic/grade10",
    "acknowledged identity only, no diagnostic construction");
   Need(f.Coordinator.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic).All(field=>
    !ReferenceEquals(field.GetValue(f.Coordinator),menu)&&!ReferenceEquals(field.GetValue(f.Coordinator),tensors)),"no mutable menu or names array retained");
   f.Bind("diagnostic/grade10",validateMenu:false);
   Need(SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null&&f.Factories==factories+1&&f.Transport.Requests.Frames==frames+2,
    "one accepted menu consumed by one materialization");
   f.Context("diagnostic/grade10");Need(SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null,"menu remains retired after diagnostic checkpoint");
  });
  Test("source coordinator diagnostic menus reject missing premature wrong null duplicate and stale identities",()=>
  {
   foreach(string mode in new[]{"before-begin","before-freeze","point","point-pending","missing","wrong-context","wrong-id","null-name","null-menu","duplicate","admitted","pending","stale"})
   {
    using var f=new SourceCoordinatorFixture();
    if(mode=="before-freeze")f.Start();
    else if(mode!="before-begin")
    {
     f.Freeze();if(mode=="point-pending")f.Bind("point0");else if(mode!="point")f.PointEvidence();
    }
    if(mode is "duplicate" or "admitted" or "pending" or "stale")f.Menu("grade10",SourceDiagnosticMenu("grade10"));
    if(mode=="admitted")f.Admit("diagnostic/grade10");
    if(mode is "pending" or "stale")f.Bind("diagnostic/grade10",validateMenu:false);
    if(mode=="stale")f.Context("diagnostic/grade10");
    int frames=f.Transport.Requests.Frames,admissions=f.DiagnosticAdmissions,factories=f.Factories;
    f.Reject(()=>
    {
     if(mode=="missing")f.Bind("diagnostic/grade10",validateMenu:false);
     else f.Menu(mode=="null-name"?null:mode=="wrong-context"?"acceleration":"grade10",
      mode=="null-menu"?null:SourceDiagnosticMenu(mode=="wrong-id"?"acceleration":"grade10"));
    });
    Need(f.DiagnosticAdmissions==admissions&&f.Transport.Requests.Frames==frames&&f.Factories==factories,
     "invalid menu stage/identity refuses before admission serialization or factory");
    Need(SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null,"failed menu authority cleared");
   }
  });
  Test("source coordinator diagnostic admission failures and swallowed reentry precede menu transport",()=>
  {
   foreach(string mode in new[]{"throw","reentry","client","catalog"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();f.PointEvidence();int frames=f.Transport.Requests.Frames,factories=f.Factories;
    f.OnDiagnosticAdmission=_=>
    {
     if(mode=="throw")throw new InvalidOperationException("manufactured diagnostic admission refusal");
     if(mode=="reentry")f.Reject(()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")));
     else if(mode=="client")((IDisposable)f.Transport.Client).Dispose();
     else Reject(()=>Call(f.Catalog,"RequireCurrentMaterialization",Template("diagnostic/grade10")),"late catalog misuse during menu admission");
    };
    f.Reject(()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")));
    Need(f.DiagnosticAdmissions==1&&f.Transport.Requests.Frames==frames&&f.Factories==factories&&
     SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null,"failed or poisoned callback cannot authorize serialization or construction");
   }
  });
  Test("source coordinator rejected diagnostic acknowledgement cannot authorize a factory",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();f.PointEvidence();int factories=f.Factories,frames=f.Transport.Requests.Frames;
   f.Transport.Requests.Change=(operation,wire)=>operation=="diagnostic-menu"?
    wire.Replace("\"scientificExecutionAuthorized\":false","\"scientificExecutionAuthorized\":true",StringComparison.Ordinal):wire;
   f.Reject(()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")));
   Need(f.DiagnosticAdmissions==1&&f.Transport.Requests.Frames==frames+1&&f.Factories==factories&&
    SourceCoordinatorField(f.Coordinator,"acceptedDiagnostic") is null,"bad acknowledgement leaves no accepted diagnostic identity");
   f.Reject(()=>f.Bind("diagnostic/grade10",validateMenu:false));Need(f.Factories==factories,"sticky failure prevents later factory");
  });
  Test("source coordinator leaf validation requires a live producer scope and accepted actual plan",()=>
  {
   foreach(string mode in new[]{"missing-scope","unvalidated-plan","unprepared","null-tensor","null-declaration"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();if(mode!="unprepared")f.Bind("point0",validatePlan:mode!="unvalidated-plan");
    object? spec=mode=="unprepared"?New("MixedTrace+LeafSpec","x",1,"manufactured-source",EmptyHash):f.LeafSpec();
    using var scope=mode=="missing-scope"?null:SourceLeafScope();
    f.Reject(()=>f.Leaf("point0","input/X",mode=="null-declaration"?null:spec,mode=="null-tensor"?null:SourceTensor()));
    Need(f.LeafStages.All(row=>row.Stage!="fingerprint"),"scope/plan/declaration requirements precede fingerprint admission");SourceLeavesCleared(f.Coordinator);
    if(scope is not null)Need((bool)Get(Call(scope,"Snapshot")!,"Poisoned"),"leaf refusal poisons active arithmetic scope");
   }
  });
  Test("source coordinator authenticates exact leaf context role identity degree source and hash",()=>
  {
   foreach(string field in new[]{"context","role","Id","Degree","Source","Sha256"})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();f.Bind("point0");var spec=f.LeafSpec();
    if(field is not ("context" or "role"))spec=ChangeSourceRecord(spec,field,field=="Degree"?2:field=="Sha256"?new string('f',64):"changed");
    using var scope=SourceLeafScope();f.Reject(()=>f.Leaf(field=="context"?"point1":"point0",field=="role"?"changed":"input/X",spec,SourceTensor()));
    Need(f.LeafStages.All(row=>row.Stage!="fingerprint")&&(bool)Get(Call(scope,"Snapshot")!,"Poisoned"),"identity checks precede arithmetic and poison scope");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("source coordinator canonical tensor hashing ignores insertion order but checks complete support",()=>
  {
   string expected=SourceTensorHash((1,0,2),(2,1,-3));
   foreach(bool corrupt in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Transport.Requests.Change=(op,wire)=>op=="prepare"?wire.Replace(EmptyHash,expected,StringComparison.Ordinal):wire;
    f.Freeze();f.Bind("point0");using(var scope=SourceLeafScope())
    {
     var tensor=SourceTensor((2,1,corrupt?-4:-3),(1,0,2));
     if(corrupt){f.Reject(()=>f.Leaf("point0","input/X",f.LeafSpec(),tensor));SourceLeavesCleared(f.Coordinator);}
     else{f.Leaf("point0","input/X",f.LeafSpec(),tensor);Need((int)SourceCoordinatorField(f.Coordinator,"acceptedLeafCount")! ==1,"all reordered support independently hashed");}
    }
    Need(f.LeafStages.Last().Stage=="fingerprint"&&f.LeafStages.Last().Units==2,"complete actual record count admitted before hashing");
    if(!corrupt){f.Background("point0",importLeaves:false);SourceLeavesCleared(f.Coordinator);}
   }
  });
  Test("source coordinator rejects duplicate canonical imports and GradientPieces alias",()=>
  {
   using(var f=new SourceCoordinatorFixture())
   {
    f.Freeze();f.Bind("point0");using var scope=SourceLeafScope();var tensor=SourceTensor();var spec=f.LeafSpec();
    f.Leaf("point0","input/X",spec,tensor);f.Reject(()=>f.Leaf("point0","input/X",spec,tensor));
    Need(f.LeafStages.Count(row=>row.Stage=="fingerprint")==1,"duplicate import refused before second fingerprint");SourceLeavesCleared(f.Coordinator);
   }
   using(var f=new SourceCoordinatorFixture())
   {
    f.OnFactory=id=>{var original=Template(id);return New("MixedSinkContextTemplate",id,Get(original,"GraphPath"),Get(original,"MetadataPath"),Get(original,"Resources"),
     Get(original,"Leaves"),new Dictionary<string,string>{{"input/X","x"},{"baseline/GradientPieces[0]","x"}},Get(original,"Marks"),Get(original,"Callbacks"),Get(original,"PointCheckpoint"));};
    f.Freeze();f.Bind("point0");using var scope=SourceLeafScope();f.Reject(()=>f.Leaf("point0","baseline/GradientPieces[0]",f.LeafSpec(),SourceTensor()));
    Need(f.LeafStages.All(row=>row.Stage!="fingerprint"),"known alias is not a second canonical leaf import");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("distinct leaf IDs sharing the same tensor hash each require and permit one import",()=>
  {
   using var f=new SourceCoordinatorFixture();f.OnFactory=SourcePlanWithTwoLeaves;f.Freeze();f.Bind("point0");
   using(var scope=SourceLeafScope())
   {var tensor=SourceTensor();f.Leaf("point0","input/X",f.LeafSpec(0),tensor);f.Leaf("point0","other",f.LeafSpec(1),tensor);}
   Need((int)SourceCoordinatorField(f.Coordinator,"acceptedLeafCount")! ==2&&f.LeafStages.Count(row=>row.Stage=="fingerprint")==2,
    "equal coefficients do not merge distinct canonical source identities");f.Background("point0",importLeaves:false);SourceLeavesCleared(f.Coordinator);
  });
  Test("complete leaf census is mandatory before background and child checkpoints",()=>
  {
   foreach(bool child in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();f.Bind("point0");
    if(child){f.Background("point0");f.Bind("point0/m0_j0");}
    int frames=f.Transport.Requests.Frames;f.Reject(()=>{if(child)f.Context("point0/m0_j0",false);else f.Background("point0",false);});
    Need(f.Transport.Requests.Frames==frames,"missing import prevents checkpoint transport");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("zero-leaf manufactured plans have an explicit empty census without fake imports",()=>
  {
   using var f=new SourceCoordinatorFixture();f.OnFactory=id=>{var original=Template(id);return New("MixedSinkContextTemplate",id,
    Get(original,"GraphPath"),Get(original,"MetadataPath"),Get(original,"Resources"),Items("MixedSinkLeafDeclaration"),new Dictionary<string,string>(),
    Get(original,"Marks"),Get(original,"Callbacks"),Get(original,"PointCheckpoint"));};
   f.Freeze();f.Bind("point0");Need(f.LeafStages.Single()==("point0","bindings",0),"zero binding slots explicitly admitted");
   f.Background("point0",false);Need((long)SourceCoordinatorField(f.Coordinator,"fingerprints")! ==0,"no manufactured placeholder fingerprint");SourceLeavesCleared(f.Coordinator);
  });
  Test("leaf bit support formatting and serialized-byte caps fail closed under real admission",()=>
  {
   foreach(string mode in new[]{"bits","scope-records","trace-records","trace-bytes","rational-characters","formatting","aggregate-bytes"})
   {
    using var f=new SourceCoordinatorFixture(leafBytes:mode=="aggregate-bytes"?1:1000000000);
    string expected=mode=="aggregate-bytes"?EmptyHash:mode=="bits"?Hash(new[]{
     new{form=1,blade=0,k0=0,k1=0,real=(BigInteger.One<<100).ToString(System.Globalization.CultureInfo.InvariantCulture),imaginary="0"},
     new{form=2,blade=1,k0=0,k1=0,real="2",imaginary="0"}}):SourceTensorHash((1,0,123),(2,1,2));
    f.Transport.Requests.Change=(op,wire)=>op=="prepare"?wire.Replace(EmptyHash,expected,StringComparison.Ordinal):wire;
    if(mode.StartsWith("trace-",StringComparison.Ordinal)||mode=="rational-characters")f.OnFactory=id=>
    {
     var original=Template(id);var resources=Get(original,"Resources");var trace=Get(resources,"Trace");
     string key=mode=="trace-records"?"TensorRecords":mode=="trace-bytes"?"TensorBytes":"RationalCharacters";
     object limit=mode=="trace-bytes"?(object)1L:1;
     resources=ChangeSourceRecord(resources,"Trace",ChangeSourceRecord(trace,key,limit));
     return New("MixedSinkContextTemplate",id,Get(original,"GraphPath"),Get(original,"MetadataPath"),resources,Get(original,"Leaves"),Get(original,"LeafRoles"),Get(original,"Marks"),Get(original,"Callbacks"),Get(original,"PointCheckpoint"));
    };
    f.Freeze();f.Bind("point0");IDictionary tensor;
    using(var generous=SourceLeafScope())
    {
     tensor=mode=="aggregate-bytes"?SourceTensor():SourceTensor((1,0,123),(2,1,2));
     if(mode=="bits")tensor[(1,0,0,0)]=New("Scalar",New("Rational",BigInteger.One<<100,BigInteger.One),Activator.CreateInstance(T("Rational"))!);
    }
    using var scope=SourceLeafScope(SourceLeafLimits(integerBits:mode=="bits"?64:4096,records:mode=="scope-records"?1:1000000,formatCharacters:mode=="formatting"?1:1000000));
    f.Reject(()=>f.Leaf("point0","input/X",f.LeafSpec(),tensor));Need((bool)Get(Call(scope,"Snapshot")!,"Poisoned"),"resource refusal poisons current scope");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("leaf fingerprint count and byte usage accumulate across retired contexts",()=>
  {
   foreach(bool count in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture(fingerprints:count?1:1000000,leafBytes:count?1000000000:2);f.Freeze();f.Bind("point0");f.Background("point0");SourceLeavesCleared(f.Coordinator);
    Need((long)SourceCoordinatorField(f.Coordinator,"fingerprints")! ==1&&(long)SourceCoordinatorField(f.Coordinator,"leafSerializedBytes")! ==2,"exact empty tensor bytes retained in cumulative usage");
    f.Bind("point0/m0_j0");using var scope=SourceLeafScope();f.Reject(()=>f.Leaf("point0/m0_j0","input/X",f.LeafSpec(),SourceTensor()));
    Need(f.LeafStages.Count(row=>row.Stage=="fingerprint")==(count?1:2),"fingerprint count checked before callback, bytes checked before excess serialization");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("binding admission precedes retained arrays and bad canonical-role censuses fail",()=>
  {
   foreach(string mode in new[]{"inspect","missing","duplicate"})
   {
    using var f=new SourceCoordinatorFixture();f.OnLeafAdmission=(id,stage,units)=>
    {if(stage=="bindings"){Need(id=="point0"&&units==1,"exact prospective leaf slots");SourceLeavesCleared(f.Coordinator);}};
    if(mode!="inspect")f.OnFactory=id=>{var original=Template(id);return New("MixedSinkContextTemplate",id,Get(original,"GraphPath"),Get(original,"MetadataPath"),Get(original,"Resources"),Get(original,"Leaves"),
     mode=="missing"?new Dictionary<string,string>():new Dictionary<string,string>{{"input/X","x"},{"duplicate","x"}},Get(original,"Marks"),Get(original,"Callbacks"),Get(original,"PointCheckpoint"));};
    f.Freeze();if(mode=="inspect"){f.Bind("point0");Need(SourceCoordinatorField(f.Coordinator,"expectedLeaves") is not null,"bindings retained only after admission");}
    else{f.Reject(()=>f.Bind("point0"));Need((int)Get(Usage(f.Catalog),"BoundContexts")==0,"invalid canonical role census cannot bind plan");SourceLeavesCleared(f.Coordinator);}
   }
  });
  Test("leaf admission failures and swallowed reentry revoke state before progress",()=>
  {
   foreach(string stage in new[]{"bindings","fingerprint"})foreach(bool reentry in new[]{false,true})
   {
    using var f=new SourceCoordinatorFixture();f.Freeze();f.OnLeafAdmission=(id,operation,units)=>
    {if(operation!=stage)return;if(reentry)f.Reject(()=>f.Leaf(id,"input/X",null,SourceTensor()));else throw new InvalidOperationException("manufactured leaf admission denied");};
    if(stage=="bindings")f.Reject(()=>f.Bind("point0"));
    else{f.Bind("point0");using var scope=SourceLeafScope();f.Reject(()=>f.Leaf("point0","input/X",f.LeafSpec(),SourceTensor()));Need((bool)Get(Call(scope,"Snapshot")!,"Poisoned"),"fingerprint callback failure poisons scope");}
    Need(f.Transport.Requests.Events==0,"no evidence after rejected leaf stage");SourceLeavesCleared(f.Coordinator);
   }
  });
  Test("leaf authority retires on checkpoint failure and disposal without retaining FT",()=>
  {
   using var f=new SourceCoordinatorFixture();f.Freeze();f.Bind("point0");IDictionary tensor;
   using(var scope=SourceLeafScope()){tensor=SourceTensor();f.Leaf("point0","input/X",f.LeafSpec(),tensor);}
   Need(f.Coordinator.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic).All(field=>!ReferenceEquals(field.GetValue(f.Coordinator),tensor)),"coordinator retains no actual tensor field");
   f.Background("point0",false);SourceLeavesCleared(f.Coordinator);f.Bind("point0/m0_j0");f.Dispose();SourceLeavesCleared(f.Coordinator);
  });
 }
 static JsonElement ActualSourceProfile()
 {
  var budgets=SourceIds().Select(id=>{var budget=SourceBudget(id);return new{id,resources=Get(budget,"Resources"),checkpointBytes=Get(budget,"CheckpointBytes")};}).ToArray();
  return JsonSerializer.SerializeToElement(new{retention=new{background=false,geometry=false,structured=false,secondJets=false},budgets,capture=CaptureDeclaration()},Json);
 }
 static void SourcePreparationPrefix()
 {
  // Actual metadata-only producer prefix for the cross-language negative
  // integration. JS MUST deny the first source admission; no sink is created.
  Need(OperatingSystem.IsLinux(),"dedicated pipe fixture requires Linux");
  using var requests=new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle((IntPtr)3,true),FileAccess.Write);
  using var replies=new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle((IntPtr)4,true),FileAccess.Read);
  var client=New("MixedSourcePreparationClient",requests,replies,ActualSourceProfile(),CaptureDeclaration(),
   New("MixedTemplateTransportLimits",2000000,536870912L,100000,8388608L,15000));
  var factory=SourceFactory();int admissions=0,factories=0,planAdmissions=0,leafAdmissions=0,diagnosticAdmissions=0;
  var catalog=Catalog(id=>{Need(admissions==factories+1,"real producer admission before metadata factory");factories++;return Call(factory,"Create",id)!;},2000000,1000000000);
  var coordinator=New("MixedSourcePreparationCoordinator",client,catalog,(Action<string>)(id=>
  {Need(id==SourceIds()[admissions%705],"ordered real producer metadata factory");admissions++;}),
   New("MixedContextPlanValidationLimits",2000000L,1000000000L),(Action<string,string>)((_,_)=>{planAdmissions++;throw new InvalidOperationException("FIRST prefix must never admit a bound source plan");}),
   New("MixedLeafValidationLimits",1000000L,1000000000L),(Action<string,string,int>)((_,_,_)=>{leafAdmissions++;throw new InvalidOperationException("FIRST prefix must never admit source leaf work");}),
   (Action<string>)(_=>{diagnosticAdmissions++;throw new InvalidOperationException("FIRST prefix must never admit diagnostic menu work");}));
  try
  {
   Await(coordinator,"BeginAsync");Call(coordinator,"ValidateCaptureDeclaration",CaptureDeclaration());
   Freeze(catalog,id=>Call(coordinator,"CheckTemplateConstructionResources",id),t=>Call(coordinator,"ValidateContextTemplate",t));
   Call(coordinator,"CompleteTemplateValidation");Need((bool)Get(coordinator,"TemplatesCompleted"),"actual producer full metadata prepass");
   Materialize(catalog,"point0",t=>Call(coordinator,"BindContextLeaves",t)!,id=>Call(coordinator,"CheckTemplateConstructionResources",id));
   throw new InvalidOperationException("FIRST test guard unexpectedly allowed source preparation");
  }
  finally{((IDisposable)coordinator).Dispose();Need(planAdmissions==0&&leafAdmissions==0&&diagnosticAdmissions==0,"FIRST source denial precedes every bound-plan leaf and diagnostic admission");}
 }
}
