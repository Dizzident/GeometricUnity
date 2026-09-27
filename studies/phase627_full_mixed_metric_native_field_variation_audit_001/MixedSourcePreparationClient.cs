using System.Security.Cryptography;
using System.Text.Json;

// Stream-only bridge, not a process owner or scientific prerequisite pack.
// A reviewed owner supplies authenticated streams, profile and real admissions.
internal sealed class MixedSourcePreparationClient:IDisposable
{
 const string RequestSchema="phase627-source-preparation-request-v1",ReplySchema="phase627-source-preparation-reply-v1";
 readonly Stream requests,replies;readonly JsonElement profile;readonly MixedCaptureDeclaration capture;
 readonly MixedTemplateTransportLimits limits;readonly object gate=new();readonly byte[] inputBuffer=new byte[4096];
 readonly string[] ids=MixedAuditPlan.RunContextIds();
 int inputAt,inputCount,sequence,validated,prepared,nextEvent,diagnosticMenus;long requestBytes,replyBytes;
 string? acceptedDiagnostic;
 bool begun,templatesCompleted,pending,finished,failed,busy,disposed;string stage="idle";
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 source client: "+why);}
 public MixedSourcePreparationClient(Stream requests,Stream replies,JsonElement profile,MixedCaptureDeclaration capture,MixedTemplateTransportLimits limits)
 {
  ArgumentNullException.ThrowIfNull(requests);ArgumentNullException.ThrowIfNull(replies);ArgumentNullException.ThrowIfNull(limits);
  Need(requests.CanWrite&&replies.CanRead&&!ReferenceEquals(requests,replies),"distinct writable/readable transport streams");
  Need(limits.FrameBytes>0&&limits.ReplyBytes>0&&limits.TotalRequestBytes>0&&limits.TotalReplyBytes>0&&limits.TimeoutMilliseconds>0,"explicit positive transport ceilings");
  Need(profile.ValueKind==JsonValueKind.Object,"full producer profile object");
  // Bound the detached profile copy prospectively; transport accounting is
  // separate from native serializer/JSON storage and proves no whole RSS bound.
  using(var bounded=new BoundedBuffer((int)Math.Min(limits.FrameBytes,limits.TotalRequestBytes)))JsonSerializer.Serialize(bounded,profile,MixedTrace.JsonOptions);
  this.profile=profile.Clone();this.capture=capture??throw new ArgumentNullException(nameof(capture));
  this.requests=requests;this.replies=replies;this.limits=limits;
 }
 async Task Guard(Func<CancellationToken,Task> action)
 {
  lock(gate){if(failed||busy||finished||disposed){failed=true;throw new InvalidOperationException("A68 source client: failed, reentrant or closed");}busy=true;}
  using var cancellation=new CancellationTokenSource(limits.TimeoutMilliseconds);
  try{await action(cancellation.Token).ConfigureAwait(false);lock(gate)Need(!failed&&!disposed,"swallowed concurrent/reentrant failure");}
  catch(OperationCanceledException error){lock(gate)failed=true;requests.Dispose();replies.Dispose();throw new OperationCanceledException($"A68 source client: deadline at sequence {sequence}, stage {stage}",error,cancellation.Token);}
  catch{lock(gate)failed=true;requests.Dispose();replies.Dispose();throw;}
  finally{lock(gate)busy=false;}
 }
 sealed class BoundedBuffer(int ceiling):Stream
 {
  readonly MemoryStream inner=new();public byte[] ToArray()=>inner.ToArray();
  public override void Write(ReadOnlySpan<byte> bytes){Need(bytes.Length<=ceiling-Length,"pre-serialization frame ceiling");inner.Write(bytes);}
  public override void Write(byte[] bytes,int offset,int count)=>Write(bytes.AsSpan(offset,count));
  public override void WriteByte(byte value){Need(Length<ceiling,"pre-serialization LF ceiling");inner.WriteByte(value);}
  public override long Length=>inner.Length;public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;
  public override long Position{get=>inner.Position;set=>throw new NotSupportedException();}public override void Flush()=>inner.Flush();
  public override int Read(byte[] b,int o,int n)=>throw new NotSupportedException();public override long Seek(long n,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long n)=>throw new NotSupportedException();
  protected override void Dispose(bool disposing){if(disposing)inner.Dispose();base.Dispose(disposing);}
 }
 async Task<byte[]> ReadFrame(CancellationToken token)
 {
  using var frame=new BoundedBuffer(limits.ReplyBytes);
  while(true)
  {
   if(inputAt==inputCount)
   {
    int take=(int)Math.Min(inputBuffer.Length,limits.TotalReplyBytes-replyBytes);Need(take>0,"cumulative reply ceiling");
    inputCount=await replies.ReadAsync(inputBuffer.AsMemory(0,take),token).AsTask().WaitAsync(token).ConfigureAwait(false);inputAt=0;
    Need(inputCount>0,"reply stream ended before acknowledgement");replyBytes=checked(replyBytes+inputCount);
   }
   int lf=Array.IndexOf(inputBuffer,(byte)10,inputAt,inputCount-inputAt),end=lf<0?inputCount:lf+1;
   frame.Write(inputBuffer.AsSpan(inputAt,end-inputAt));inputAt=end;if(lf>=0)return frame.ToArray();
  }
 }
 async Task Exchange(string operation,object? payload,Func<JsonElement,object?> expectedReport,bool closeRequests,CancellationToken token)
 {
  using var frame=new BoundedBuffer(limits.FrameBytes);
  JsonSerializer.Serialize(frame,new{schema=RequestSchema,sequence,operation,payload},MixedTrace.JsonOptions);frame.WriteByte(10);
  var bytes=frame.ToArray();Need(bytes.Length<=limits.TotalRequestBytes-requestBytes,"cumulative request ceiling before write");
  string hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();requestBytes=checked(requestBytes+bytes.Length);
  stage="request-write";await requests.WriteAsync(bytes,token).AsTask().WaitAsync(token).ConfigureAwait(false);await requests.FlushAsync(token).WaitAsync(token).ConfigureAwait(false);
  if(closeRequests)requests.Dispose();
  stage="reply-read";var wire=await ReadFrame(token).ConfigureAwait(false);
  Need(wire.Length>1&&wire[^1]==10&&wire.AsSpan(0,wire.Length-1).IndexOf((byte)10)<0,"one complete LF reply");
  foreach(byte value in wire.AsSpan(0,wire.Length-1))Need(value is >=32 and <=126,"ASCII canonical reply");
  using var actual=JsonDocument.Parse(wire.AsMemory(0,wire.Length-1),new(){MaxDepth=16});
  var report=expectedReport(actual.RootElement.GetProperty("report"));
  using var expected=new BoundedBuffer(limits.ReplyBytes);
  JsonSerializer.Serialize(expected,new{schema=ReplySchema,sequence,operation,requestSha256=hash,status="accepted",report},MixedTrace.JsonOptions);
  // Exact reconstructed bytes reject extra/duplicate/reordered fields,
  // alternate encodings and promoted scope, including dynamic leaf replies.
  Need(wire.AsSpan(0,wire.Length-1).SequenceEqual(expected.ToArray()),"exact independently bound canonical acknowledgement");sequence++;
 }
 Task Fixed(string operation,object? payload,object? report,CancellationToken token,bool close=false)=>Exchange(operation,payload,_=>report,close,token);
 (string Kind,string Id) Event()
 {
  Need(nextEvent>=0&&nextEvent<707,"remaining checkpoint event");
  if(nextEvent>=704)return("diagnostic","diagnostic/"+new[]{"grade10","acceleration","secondJets"}[nextEvent-704]);
  int point=nextEvent/352,offset=nextEvent%352;string id="point"+point;
  return offset==0?("background",id):offset==351?("final",id):("germ",id+"/m"+(offset-1)/35+"_j"+(offset-1)%35);
 }
 public Task BeginAsync()=>Guard(async token=>
 {Need(!begun&&sequence==0,"one producer handshake");await Fixed("begin",new{profile,capture},null,token).ConfigureAwait(false);begun=true;});
 public void RequireCaptureDeclaration(MixedCaptureDeclaration actual)
 {
  lock(gate)try{Need(begun&&!failed&&!busy&&!finished&&!disposed,"healthy accepted capture handshake");Need(capture.Matches(actual),"actual sink capture differs from handshake");}
  catch{failed=true;throw;}
 }
 public Task ValidateAsync(MixedSinkContextTemplate template)=>Guard(async token=>
 {
  Need(begun&&!templatesCompleted&&validated<ids.Length&&template is not null&&template.Id==ids[validated],"ordered complete705 template validation");
  var actual=template!;await Fixed("validate",actual.Wire(),new{contextId=actual.Id},token).ConfigureAwait(false);validated++;
 });
 public Task CompleteTemplateValidationAsync()=>Guard(async token=>
 {Need(begun&&!templatesCompleted&&validated==705,"complete705 before freeze");await Fixed("freeze",null,new{contexts=705,scientificExecutionAuthorized=false},token).ConfigureAwait(false);templatesCompleted=true;});
 public Task ValidateDiagnosticMenuAsync(string diagnostic,DiagnosticMenu menu)=>Guard(async token=>
 {
  var expected=Event();
  Need(templatesCompleted&&!pending&&acceptedDiagnostic is null&&expected.Kind=="diagnostic"&&
   diagnosticMenus==nextEvent-704&&diagnostic is not null&&expected.Id=="diagnostic/"+diagnostic&&menu is not null&&menu.Id==diagnostic,
   "one complete next diagnostic menu before source preparation");
  // The independent service validates EVERY field and ordered name array.
  // Its reply binds the exact bounded serialized request, not a producer hash
  // offered as an independent expectation. No mutable menu is retained here.
  await Fixed("diagnostic-menu",new{diagnostic,menu},new{contextId=expected.Id,scientificExecutionAuthorized=false},token).ConfigureAwait(false);
  acceptedDiagnostic=expected.Id;diagnosticMenus++;
 });
 public async Task<IReadOnlyList<MixedTrace.LeafSpec>> PrepareAsync(MixedSinkContextTemplate template)
 {
  IReadOnlyList<MixedTrace.LeafSpec>? result=null;
  await Guard(async token=>
  {
   var expected=Event();Need(templatesCompleted&&!pending&&prepared<705&&expected.Kind!="final"&&template is not null&&template.Id==expected.Id&&template.Id==ids[prepared],"one ordered preparation before checkpoint");
   Need(expected.Kind=="diagnostic"?acceptedDiagnostic==expected.Id:acceptedDiagnostic is null,"independently accepted diagnostic menu before prepare");
   var actual=template!;await Exchange("prepare",actual.Wire(),report=>
   {
    var leaves=report.GetProperty("leaves");Need(leaves.ValueKind==JsonValueKind.Array&&leaves.GetArrayLength()==actual.Leaves.Count,"complete ordered leaf reply");
    var copy=new MixedTrace.LeafSpec[actual.Leaves.Count];
    for(int i=0;i<copy.Length;i++)
    {
     var declared=actual.Leaves[i];string? sha=leaves[i].GetProperty("sha256").GetString();
     Need(sha is {Length:64}&&sha.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f'),"canonical independent leaf SHA");
     copy[i]=new(declared.Id,declared.Degree,declared.Source,sha!);
    }
    result=Array.AsReadOnly(copy);return new{contextId=actual.Id,leaves=copy};
   },false,token).ConfigureAwait(false);pending=true;prepared++;acceptedDiagnostic=null;
  }).ConfigureAwait(false);
  return result!;
 }
 Task Checkpoint(object? checkpoint,bool background)=>Guard(async token=>
 {
  string? context=checkpoint switch{MixedPointBackgroundCheckpoint point=>point.Context,MixedContextComputationalCheckpoint child=>child.Context,_=>null};
  string kind=background?"background":context?.StartsWith("diagnostic/",StringComparison.Ordinal)==true?"diagnostic":"germ";
  var expected=Event();Need(checkpoint is not null&&templatesCompleted&&pending&&expected.Kind==kind&&expected.Id==context,"exact pending checkpoint kind and context");
  await Fixed("checkpoint",checkpoint,new{@event=nextEvent,kind=expected.Kind,contextId=expected.Id,scientificExecutionAuthorized=false},token).ConfigureAwait(false);pending=false;nextEvent++;
 });
 public Task AcceptBackgroundAsync(MixedPointBackgroundCheckpoint checkpoint)=>Checkpoint(checkpoint,true);
 public Task AcceptContextAsync(MixedContextComputationalCheckpoint checkpoint)=>Checkpoint(checkpoint,false);
 public Task CompletePointAsync(MixedPointCompletionCheckpoint checkpoint)=>Guard(async token=>
 {
  var expected=Event();Need(templatesCompleted&&!pending&&expected.Kind=="final"&&checkpoint is not null&&checkpoint.Context==expected.Id,"all same-point children before final");
  await Fixed("pointfinal",checkpoint,new{@event=nextEvent,kind=expected.Kind,contextId=expected.Id,scientificExecutionAuthorized=false},token).ConfigureAwait(false);nextEvent++;
 });
 public Task CompleteAsync()=>Guard(async token=>
 {
  Need(templatesCompleted&&prepared==705&&nextEvent==707&&diagnosticMenus==3&&acceptedDiagnostic is null&&!pending&&sequence==2122,"all preparations, diagnostic menus and checkpoints before terminal finish");
  await Fixed("finish",null,new{contexts=705,checkpoints=707,scientificExecutionAuthorized=false},token,true).ConfigureAwait(false);
  Need(inputAt==inputCount,"no queued bytes after final acknowledgement");Need(replyBytes<limits.TotalReplyBytes,"terminal EOF probe reservation");replyBytes++;
  stage="terminal-EOF";Need(await replies.ReadAsync(inputBuffer.AsMemory(0,1),token).AsTask().WaitAsync(token).ConfigureAwait(false)==0,"clean terminal reply EOF");finished=true;
 });
 public bool TemplatesCompleted{get{lock(gate)return templatesCompleted&&!failed&&!disposed;}}
 public bool Completed{get{lock(gate)return finished&&!failed&&!disposed;}}
 public bool Failed{get{lock(gate)return failed;}}
 public void Dispose(){lock(gate){if(disposed)return;if(!finished||busy)failed=true;disposed=true;}requests.Dispose();replies.Dispose();}
}
