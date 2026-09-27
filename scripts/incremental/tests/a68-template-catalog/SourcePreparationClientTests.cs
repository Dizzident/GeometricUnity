using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Manufactured stream acknowledgements only: no independent source brand,
// retained input, geometry, scientific recipe, sink or output file is created.
static partial class Program
{
 static object SourceClient(Stream requests,Stream replies,int frame=2000000,long total=536870912,int reply=4096,long replyTotal=4194304,int timeout=1000)
 {
  using var profile=JsonDocument.Parse(JsonSerializer.Serialize(new{capture=CaptureDeclaration()},Json));
  return New("MixedSourcePreparationClient",requests,replies,profile.RootElement,CaptureDeclaration(),New("MixedTemplateTransportLimits",frame,total,reply,replyTotal,timeout));
 }
 sealed class SourceAckRequests(AckReplies replies):MemoryStream
 {
  public int Frames,Events,DiagnosticMenus,LastSequence=-1,MaximumFrameBytes;public long RequestBytes;public bool Closed,TrailingFinish;
  public Func<string,string,string>? Change;public Action<string>? OnWrite;
  public override ValueTask WriteAsync(ReadOnlyMemory<byte> bytes,CancellationToken token=default)
  {
   token.ThrowIfCancellationRequested();Need(!Closed,"request stream still open");using var document=JsonDocument.Parse(bytes);var root=document.RootElement;
   string operation=root.GetProperty("operation").GetString()!;int sequence=root.GetProperty("sequence").GetInt32();var payload=root.GetProperty("payload");
   Need(root.GetProperty("schema").GetString()=="phase627-source-preparation-request-v1"&&sequence==Frames,"exact source request schema and sequence");
   OnWrite?.Invoke(operation);object? report=null;
   if(operation=="validate")report=new{contextId=payload.GetProperty("id").GetString()};
   else if(operation=="freeze")report=new{contexts=705,scientificExecutionAuthorized=false};
   else if(operation=="diagnostic-menu")
   {
    Need(payload.EnumerateObject().Select(property=>property.Name).SequenceEqual(new[]{"diagnostic","menu"}),"exact diagnostic payload fields and order");
    string diagnostic=payload.GetProperty("diagnostic").GetString()!;
    Need(diagnostic==new[]{"grade10","acceleration","secondJets"}[DiagnosticMenus]&&payload.GetProperty("menu").GetProperty("id").GetString()==diagnostic,"ordered diagnostic menu identity");
    DiagnosticMenus++;report=new{contextId="diagnostic/"+diagnostic,scientificExecutionAuthorized=false};
   }
   else if(operation=="prepare")report=new{contextId=payload.GetProperty("id").GetString(),leaves=payload.GetProperty("leaves").EnumerateArray().Select(leaf=>new{
    id=leaf.GetProperty("id").GetString(),degree=leaf.GetProperty("degree").GetInt32(),source=leaf.GetProperty("source").GetString(),sha256=EmptyHash}).ToArray()};
   else if(operation is "checkpoint" or "pointfinal")
   {
    string id=payload.GetProperty("context").GetString()!;
    string kind=operation=="pointfinal"?"final":id.StartsWith("diagnostic/",StringComparison.Ordinal)?"diagnostic":id is "point0" or "point1"?"background":"germ";
    report=new{@event=Events++,kind,contextId=id,scientificExecutionAuthorized=false};
   }
   else if(operation=="finish")report=new{contexts=705,checkpoints=707,scientificExecutionAuthorized=false};
   string hash=Convert.ToHexString(SHA256.HashData(bytes.Span)).ToLowerInvariant();
   string wire=JsonSerializer.Serialize(new{schema="phase627-source-preparation-reply-v1",sequence,operation,requestSha256=hash,status="accepted",report},Json)+"\n";
   if(operation=="finish"&&TrailingFinish)wire+="x";
   if(Change is not null)wire=Change(operation,wire);
   replies.Queue(Encoding.ASCII.GetBytes(wire));LastSequence=sequence;MaximumFrameBytes=Math.Max(MaximumFrameBytes,bytes.Length);RequestBytes+=bytes.Length;Frames++;return ValueTask.CompletedTask;
  }
  protected override void Dispose(bool disposing){Closed=true;replies.End=true;base.Dispose(disposing);}
 }
 sealed class SourceClientFixture:IDisposable
 {
  public readonly AckReplies Replies=new();public readonly SourceAckRequests Requests;public readonly object Client;
  public SourceClientFixture(int frame=2000000,long total=536870912,int reply=4096,long replyTotal=4194304,int timeout=1000)
  {
   Requests=new(Replies);Client=SourceClient(Requests,Replies,frame,total,reply,replyTotal,timeout);
  }
  public void Begin(){Await(Client,"BeginAsync");Call(Client,"RequireCaptureDeclaration",CaptureDeclaration());}
  public void Freeze()
  {
   Begin();foreach(string id in SourceIds())Await(Client,"ValidateAsync",Template(id));
   Need(!(bool)Get(Client,"TemplatesCompleted"),"705 validations alone do not complete template phase");
   Await(Client,"CompleteTemplateValidationAsync");Need((bool)Get(Client,"TemplatesCompleted")&&!Requests.Closed&&!(bool)Get(Client,"Completed"),"freeze keeps producer stream open");
  }
  public void Menu(string? diagnostic,object? menu)=>Await(Client,"ValidateDiagnosticMenuAsync",diagnostic,menu);
  public void Prepare(string id,bool validateMenu=true)
  {
   if(validateMenu&&id.StartsWith("diagnostic/",StringComparison.Ordinal))Menu(id[11..],SourceDiagnosticMenu(id[11..]));
   var task=Call(Client,"PrepareAsync",Template(id))!;((Task)task).GetAwaiter().GetResult();
   var leaves=((IEnumerable)Get(task,"Result")).Cast<object>().ToArray();
   Need(leaves.Length==1&&(string)Get(leaves[0],"Id")=="x"&&(int)Get(leaves[0],"Degree")==1&&
    (string)Get(leaves[0],"Source")=="manufactured-source"&&(string)Get(leaves[0],"Sha256")==EmptyHash,"ordered bounded dynamic leaf response");
  }
  public void PointCheckpoints()
  {
   for(int p=0;p<2;p++)
   {
    string point="point"+p;Prepare(point);Await(Client,"AcceptBackgroundAsync",SourceBackground(point));
    for(int m=0;m<10;m++)for(int j=0;j<35;j++)
    {string id=point+"/m"+m+"_j"+j;Prepare(id);Await(Client,"AcceptContextAsync",SourceContextCheckpoint(id));}
    Await(Client,"CompletePointAsync",SourcePointFinal(point));
   }
  }
  public void AllCheckpoints()
  {
   PointCheckpoints();foreach(string diagnostic in new[]{"grade10","acceleration","secondJets"})
   {string id="diagnostic/"+diagnostic;Prepare(id);Await(Client,"AcceptContextAsync",SourceContextCheckpoint(id));}
  }
  public void Dispose(){((IDisposable)Client).Dispose();Requests.Dispose();Replies.Dispose();}
 }
 // Deliberately tiny, non-scientific menus. The mock peer acknowledges protocol
 // identity only; independent full menu validation belongs to the actual peer.
 static object SourceDiagnosticMenu(string id)=>New("DiagnosticMenu",id,new[]{"tiny"},Array.Empty<string>(),Array.Empty<string>(),
  Array.Empty<string>(),Array.Empty<string>(),Array.Empty<string>(),Array.Empty<string>(),0,0,0);
 static object SourceArtifact(string path)=>New("MixedEmittedArtifact",path,2L,EmptyHash);
 static object SourceBackground(string id)=>New("MixedPointBackgroundCheckpoint",id,SourceArtifact(id+"/graph.json"),SourceArtifact(id+"/background-checkpoint.json"));
 static object SourceContextCheckpoint(string id)=>New("MixedContextComputationalCheckpoint",id,SourceArtifact(id+"/graph.json"),SourceArtifact(id+"/metadata.json"));
 static object SourcePointFinal(string id)=>New("MixedPointCompletionCheckpoint",id,SourceBackground(id),SourceArtifact(id+"/metadata.json"));
 static void SourcePreparationClientTests()
 {
  Test("source client completes exact2123 metadata-only manufactured exchanges and terminal EOF",()=>
  {
   using var f=new SourceClientFixture();f.Freeze();f.AllCheckpoints();Need(f.Requests.Frames==2122&&f.Requests.Events==707&&f.Requests.DiagnosticMenus==3&&!(bool)Get(f.Client,"Completed"),"full event sequence still needs finish");
   Await(f.Client,"CompleteAsync");Need(f.Requests.Frames==2123&&f.Requests.LastSequence==2122&&f.Requests.Closed&&(bool)Get(f.Client,"Completed")&&!(bool)Get(f.Client,"Failed"),"clean2123 transaction");
   RejectClient(f.Client,()=>Await(f.Client,"BeginAsync"));
  });
  Test("source client capture mismatch and premature stages poison without retry",()=>
  {
   foreach(string mode in new[]{"capture-before-begin","capture","validate","freeze","prepare","checkpoint","final","finish","duplicate-begin"})
   {
    using var f=new SourceClientFixture();if(mode!="capture-before-begin")f.Begin();
    RejectClient(f.Client,()=>
    {
     switch(mode)
     {
      case "capture-before-begin":Call(f.Client,"RequireCaptureDeclaration",CaptureDeclaration());break;
      case "capture":Call(f.Client,"RequireCaptureDeclaration",CaptureDeclaration(1));break;
      case "validate":Await(f.Client,"ValidateAsync",Template("point1"));break;
      case "freeze":Await(f.Client,"CompleteTemplateValidationAsync");break;
      case "prepare":f.Prepare("point0");break;
      case "checkpoint":Await(f.Client,"AcceptBackgroundAsync",SourceBackground("point0"));break;
      case "final":Await(f.Client,"CompletePointAsync",SourcePointFinal("point0"));break;
      case "finish":Await(f.Client,"CompleteAsync");break;
      default:Await(f.Client,"BeginAsync");break;
     }
    });
    RejectClient(f.Client,()=>Await(f.Client,"BeginAsync"));
   }
  });
  Test("source client checks full pending-context and point-child ordering",()=>
  {
   foreach(string mode in new[]{"wrong-prepare","double-prepare","wrong-checkpoint","wrong-kind","early-final","duplicate-freeze"})
   {
    using var f=new SourceClientFixture();f.Freeze();
    if(mode is "double-prepare" or "wrong-checkpoint" or "wrong-kind")f.Prepare("point0");
    RejectClient(f.Client,()=>
    {
     switch(mode)
     {
      case "wrong-prepare":f.Prepare("point1");break;
      case "double-prepare":f.Prepare("point0/m0_j0");break;
      case "wrong-checkpoint":Await(f.Client,"AcceptBackgroundAsync",SourceBackground("point1"));break;
      case "wrong-kind":Await(f.Client,"AcceptContextAsync",SourceContextCheckpoint("point0"));break;
      case "early-final":Await(f.Client,"CompletePointAsync",SourcePointFinal("point0"));break;
      default:Await(f.Client,"CompleteTemplateValidationAsync");break;
     }
    });
   }
  });
  Test("source client rejects altered ordered leaf identity hash census and canonical shape",()=>
  {
   Func<string,string>[] changes=[
    wire=>wire.Replace("\"id\":\"x\"","\"id\":\"wrong\"",StringComparison.Ordinal),
    wire=>wire.Replace("\"degree\":1","\"degree\":2",StringComparison.Ordinal),
    wire=>wire.Replace("\"source\":\"manufactured-source\"","\"source\":\"wrong\"",StringComparison.Ordinal),
    wire=>wire.Replace(EmptyHash,EmptyHash.ToUpperInvariant(),StringComparison.Ordinal),
    wire=>wire.Replace("\"sha256\":", "\"extra\":0,\"sha256\":",StringComparison.Ordinal),
    wire=>wire.Replace("\"sha256\":", "\"sha256\":\""+EmptyHash+"\",\"sha256\":",StringComparison.Ordinal),
    wire=>wire.Replace("\"contextId\":\"point0\"","\"contextId\":\"point1\"",StringComparison.Ordinal),
    wire=>wire.Replace("\"leaves\":[{","\"leaves\":[{\"unknown\":null,",StringComparison.Ordinal)
   ];
   foreach(var change in changes)
   {using var f=new SourceClientFixture();f.Freeze();f.Requests.Change=(op,wire)=>op=="prepare"?change(wire):wire;RejectClient(f.Client,()=>f.Prepare("point0"));}
  });
  Test("source client rejects wrong request binding duplicate fields and promoted scope",()=>
  {
   foreach(string mode in new[]{"hash","schema","sequence","duplicate","scope"})
   {
    using var f=new SourceClientFixture();f.Requests.Change=(op,wire)=>mode switch
    {
     "hash"=>wire.Replace("\"requestSha256\":\"","\"requestSha256\":\"0",StringComparison.Ordinal),
     "schema"=>wire.Replace("source-preparation-reply-v1","source-preparation-reply-v0",StringComparison.Ordinal),
     "sequence"=>wire.Replace("\"sequence\":0","\"sequence\":1",StringComparison.Ordinal),
     "duplicate"=>wire.Replace("\"status\":","\"status\":\"accepted\",\"status\":",StringComparison.Ordinal),
     _=>wire.Replace("\"report\":null","\"report\":{\"scientificExecutionAuthorized\":true}",StringComparison.Ordinal)
    };RejectClient(f.Client,()=>f.Begin());
   }
  });
  Test("source client rejects two-leaf reversal and incomplete leaf census",()=>
  {
   foreach(bool reverse in new[]{true,false})
   {
    using var f=new SourceClientFixture();var original=Template("point0");
    var template=New("MixedSinkContextTemplate","point0",Get(original,"GraphPath"),Get(original,"MetadataPath"),Get(original,"Resources"),
     Items("MixedSinkLeafDeclaration",New("MixedSinkLeafDeclaration","x",1,"manufactured-source"),New("MixedSinkLeafDeclaration","y",1,"manufactured-source")),
     Get(original,"LeafRoles"),Get(original,"Marks"),Get(original,"Callbacks"),Get(original,"PointCheckpoint"));
    f.Begin();foreach(string id in SourceIds())Await(f.Client,"ValidateAsync",id=="point0"?template:Template(id));Await(f.Client,"CompleteTemplateValidationAsync");
    f.Requests.Change=(op,wire)=>
    {
     if(op!="prepare")return wire;
     if(reverse)return wire.Replace("\"id\":\"x\"","\"id\":\"temporary\"",StringComparison.Ordinal)
      .Replace("\"id\":\"y\"","\"id\":\"x\"",StringComparison.Ordinal).Replace("\"id\":\"temporary\"","\"id\":\"y\"",StringComparison.Ordinal);
     string second=",{\"id\":\"y\",\"degree\":1,\"source\":\"manufactured-source\",\"sha256\":\""+EmptyHash+"\"}";
     return wire.Replace(second,"",StringComparison.Ordinal);
    };
    RejectClient(f.Client,()=>Await(f.Client,"PrepareAsync",template));
   }
  });
  Test("source client enforces prospective request and dynamic reply quotas",()=>
  {
   int profileBytes=JsonSerializer.SerializeToUtf8Bytes(new{capture=CaptureDeclaration()},Json).Length;
   foreach(bool perFrame in new[]{true,false})
   {using var f=new SourceClientFixture(frame:perFrame?profileBytes+1:2000000,total:perFrame?536870912:profileBytes+1);RejectClient(f.Client,()=>f.Begin());Need(f.Requests.Frames==0,"no request emitted beyond ceiling");}
   using(var f=new SourceClientFixture(reply:300))
   {f.Freeze();RejectClient(f.Client,()=>f.Prepare("point0"));}
   using(var f=new SourceClientFixture(replyTotal:1))RejectClient(f.Client,()=>f.Begin());
  });
  Test("source client sticky timeout and swallowed reentry",()=>
  {
   using(var requests=new MemoryStream())using(var replies=new NeverReply())using(var profile=JsonDocument.Parse("{}"))
   {
    var client=New("MixedSourcePreparationClient",requests,replies,profile.RootElement,CaptureDeclaration(),New("MixedTemplateTransportLimits",2000000,536870912L,4096,4194304L,25));
    try{RejectClient(client,()=>Await(client,"BeginAsync"));}finally{((IDisposable)client).Dispose();}
   }
   using var f=new SourceClientFixture();f.Requests.OnWrite=_=>{try{Await(f.Client,"BeginAsync");}catch(InvalidOperationException){}};
   RejectClient(f.Client,()=>f.Begin());
  });
  Test("source client null checkpoints fail inside sticky guard",()=>
  {
   foreach(string method in new[]{"AcceptBackgroundAsync","AcceptContextAsync"})
   {using var f=new SourceClientFixture();RejectClient(f.Client,()=>Await(f.Client,method,new object?[]{null}));RejectClient(f.Client,()=>f.Begin());}
  });
  Test("source client diagnostic menus require frozen exact-next idle diagnostic and cannot be reused",()=>
  {
   foreach(string mode in new[]{"before-begin","before-freeze","point","point-pending","missing","wrong-context","wrong-id","null-name","null-menu","duplicate","pending","stale"})
   {
    using var f=new SourceClientFixture();
    if(mode=="before-freeze")f.Begin();
    else if(mode!="before-begin")
    {
     f.Freeze();if(mode=="point-pending")f.Prepare("point0");else if(mode!="point")f.PointCheckpoints();
    }
    if(mode is "duplicate" or "pending" or "stale")f.Menu("grade10",SourceDiagnosticMenu("grade10"));
    if(mode is "pending" or "stale")f.Prepare("diagnostic/grade10",validateMenu:false);
    if(mode=="stale")Await(f.Client,"AcceptContextAsync",SourceContextCheckpoint("diagnostic/grade10"));
    int frames=f.Requests.Frames;
    RejectClient(f.Client,()=>
    {
     if(mode=="missing")f.Prepare("diagnostic/grade10",validateMenu:false);
     else f.Menu(mode=="null-name"?null:mode=="wrong-context"?"acceleration":"grade10",
      mode=="null-menu"?null:SourceDiagnosticMenu(mode=="wrong-id"?"acceleration":"grade10"));
    });
    Need(f.Requests.Frames==frames,"invalid menu ordering or identity refused before request serialization/write");
   }
  });
  Test("source client diagnostic-menu acknowledgement binds context scope shape and request",()=>
  {
   foreach(string mode in new[]{"context","scope","extra","duplicate","request-hash"})
   {
    using var f=new SourceClientFixture();f.Freeze();f.PointCheckpoints();
    f.Requests.Change=(operation,wire)=>operation!="diagnostic-menu"?wire:mode switch
    {
     "context"=>wire.Replace("diagnostic/grade10","diagnostic/acceleration",StringComparison.Ordinal),
     "scope"=>wire.Replace("\"scientificExecutionAuthorized\":false","\"scientificExecutionAuthorized\":true",StringComparison.Ordinal),
     "extra"=>wire.Replace("\"report\":{","\"report\":{\"extra\":null,",StringComparison.Ordinal),
     "duplicate"=>wire.Replace("\"contextId\":","\"contextId\":\"diagnostic/grade10\",\"contextId\":",StringComparison.Ordinal),
     _=>wire.Replace("\"requestSha256\":\"","\"requestSha256\":\"0",StringComparison.Ordinal)
    };
    int frames=f.Requests.Frames;RejectClient(f.Client,()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")));
    Need(f.Requests.Frames==frames+1,"one rejected menu exchange, no prepare after bad acknowledgement");
    RejectClient(f.Client,()=>f.Prepare("diagnostic/grade10",validateMenu:false));
   }
  });
  Test("source client diagnostic menu request is bounded before transport writes",()=>
  {
   long prefixBytes;int prefixFrameBytes;
   using(var measure=new SourceClientFixture())
   {measure.Freeze();measure.PointCheckpoints();prefixBytes=measure.Requests.RequestBytes;prefixFrameBytes=measure.Requests.MaximumFrameBytes;}
   foreach(bool frame in new[]{true,false})
   {
    // The measured maximum includes the full capture/profile handshake. It
    // permits every setup frame; only the oversized menu crosses this cap.
    using var f=new SourceClientFixture(frame:frame?prefixFrameBytes:2000000,total:frame?536870912:prefixBytes+1);f.Freeze();f.PointCheckpoints();
    var menu=SourceDiagnosticMenu("grade10");if(frame)menu=ChangeSourceRecord(menu,"Tensors",new[]{new string('x',prefixFrameBytes+1)});
    int frames=f.Requests.Frames;RejectClient(f.Client,()=>f.Menu("grade10",menu));
    Need(f.Requests.Frames==frames&&f.Requests.DiagnosticMenus==0,"per-frame and cumulative ceilings before any menu bytes are written");
   }
  });
  Test("source client diagnostic menu swallowed reentry poisons acceptance",()=>
  {
   using var f=new SourceClientFixture();f.Freeze();f.PointCheckpoints();
   f.Requests.OnWrite=operation=>{if(operation=="diagnostic-menu")Reject(()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")),"nested menu exchange");};
   RejectClient(f.Client,()=>f.Menu("grade10",SourceDiagnosticMenu("grade10")));
   Need(f.Requests.DiagnosticMenus==1,"outer acknowledgement cannot recover poisoned transport");
  });
  Test("source client rejects extra bytes after otherwise complete final acknowledgement",()=>
  {using var f=new SourceClientFixture();f.Freeze();f.AllCheckpoints();f.Requests.TrailingFinish=true;RejectClient(f.Client,()=>Await(f.Client,"CompleteAsync"));});
 }
}
