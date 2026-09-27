using System.Security.Cryptography;
using System.Text.Json;

// Stream-only metadata client. A separately reviewed host must start/pin the
// validator process, supply its independent frozen profile and construction
// admission, own termination and check process exit. This class grants no
// source/numerical authority and never launches a scientific entry point.
internal sealed record MixedTemplateTransportLimits(int FrameBytes,long TotalRequestBytes,int ReplyBytes,long TotalReplyBytes,int TimeoutMilliseconds);
internal sealed class MixedTemplatePreflightClient:IDisposable
{
 const string RequestSchema="phase627-template-preflight-request-v2",ReplySchema="phase627-template-preflight-reply-v2";
 readonly MixedCaptureDeclaration capture;
 readonly Stream requests,replies;readonly MixedTemplateTransportLimits limits;readonly string profileSha;
 readonly object gate=new();readonly byte[] inputBuffer=new byte[4096];int inputAt,inputCount,sequence;
 long requestBytes,replyBytes;bool begun,finished,failed,busy,disposed;string stage="idle";
 static object Scope()=>new{structureOnly=true,numericalReplayComplete=false,sourceAuthenticityEstablished=false,resourceSufficiencyProved=false,totalProcessMemoryProved=false,scientificExecutionAuthorized=false};
 public MixedTemplatePreflightClient(Stream requests,Stream replies,string profileSha256,MixedTemplateTransportLimits limits,MixedCaptureDeclaration capture)
 {
  ArgumentNullException.ThrowIfNull(requests);ArgumentNullException.ThrowIfNull(replies);ArgumentNullException.ThrowIfNull(limits);
  Need(requests.CanWrite&&replies.CanRead,"separate writable/readable transport streams");Need(!ReferenceEquals(requests,replies),"distinct request/reply streams");
  Need(profileSha256 is {Length:64}&&profileSha256.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f'),"explicit canonical independent profile commitment");
  Need(limits.FrameBytes>0&&limits.ReplyBytes>0&&limits.TotalRequestBytes>0&&limits.TotalReplyBytes>0&&limits.TimeoutMilliseconds>0,"explicit positive transport ceilings");
  this.requests=requests;this.replies=replies;this.profileSha=profileSha256;this.limits=limits;this.capture=capture??throw new ArgumentNullException(nameof(capture));
 }
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 template client: "+why);}
 async Task Guard(Func<CancellationToken,Task> action)
 {
  lock(gate){if(failed||busy||finished||disposed){failed=true;throw new InvalidOperationException("A68 template client: failed, reentrant or closed");}busy=true;}
  using var cancellation=new CancellationTokenSource(limits.TimeoutMilliseconds);
  try{await action(cancellation.Token).ConfigureAwait(false);lock(gate)Need(!failed,"swallowed concurrent/reentrant failure");}
  catch(OperationCanceledException error){lock(gate)failed=true;requests.Dispose();replies.Dispose();throw new OperationCanceledException($"A68 template client: deadline at sequence {sequence}, stage {stage}",error,cancellation.Token);}
  catch{lock(gate)failed=true;requests.Dispose();replies.Dispose();throw;}
  finally{lock(gate)busy=false;}
 }
 sealed class BoundedBuffer(int ceiling):Stream
 {
  readonly MemoryStream inner=new();
  public byte[] ToArray()=>inner.ToArray();
  public override void Write(ReadOnlySpan<byte> bytes){Need(bytes.Length<=ceiling-Length,"pre-serialization frame ceiling");inner.Write(bytes);}
  public override void Write(byte[] bytes,int offset,int count)=>Write(bytes.AsSpan(offset,count));
  public override void WriteByte(byte value){Need(Length<ceiling,"pre-serialization LF ceiling");inner.WriteByte(value);}
  public override long Length=>inner.Length;public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;
  public override long Position{get=>inner.Position;set=>throw new NotSupportedException();}public override void Flush()=>inner.Flush();
  public override int Read(byte[] b,int o,int n)=>throw new NotSupportedException();public override long Seek(long n,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long n)=>throw new NotSupportedException();
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
   frame.Write(inputBuffer.AsSpan(inputAt,end-inputAt));inputAt=end;
   if(lf>=0)return frame.ToArray();
  }
 }
 static void CheckReply(byte[] wire,object expected)
 {
  Need(wire.Length>1&&wire[^1]==10&&wire.AsSpan(0,wire.Length-1).IndexOf((byte)10)<0,"one complete LF reply");
  foreach(byte value in wire.AsSpan(0,wire.Length-1))Need(value is >=32 and <=126,"ASCII canonical reply");
  using var actual=JsonDocument.Parse(wire.AsMemory(0,wire.Length-1),new(){MaxDepth=16});
  var expectedBytes=JsonSerializer.SerializeToUtf8Bytes(expected,MixedTrace.JsonOptions);
  // Fixed reply field order and exact bytes forbid extra/duplicate fields,
  // alternate numeric/escape encodings and any promoted scope flag.
  Need(wire.AsSpan(0,wire.Length-1).SequenceEqual(expectedBytes),"exact independently expected bound acknowledgement");
 }
 async Task Exchange(string operation,object? template,object? report,bool closeRequests,CancellationToken token)
 {
  using var frame=new BoundedBuffer(limits.FrameBytes);
  JsonSerializer.Serialize(frame,new{schema=RequestSchema,sequence,operation,profileSha256=sequence==0?profileSha:null,template},MixedTrace.JsonOptions);frame.WriteByte(10);
  var bytes=frame.ToArray();Need(bytes.Length<=limits.TotalRequestBytes-requestBytes,"cumulative request ceiling before write");
  string hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();requestBytes=checked(requestBytes+bytes.Length);
  stage="request-write";await requests.WriteAsync(bytes,token).AsTask().WaitAsync(token).ConfigureAwait(false);await requests.FlushAsync(token).WaitAsync(token).ConfigureAwait(false);
  if(closeRequests)requests.Dispose(); // EOF is mandatory BEFORE final acknowledgement.
  stage="reply-read";var response=await ReadFrame(token).ConfigureAwait(false);
  CheckReply(response,new{schema=ReplySchema,sequence,operation,requestSha256=hash,status="accepted",report});sequence++;
 }
 public Task BeginAsync()=>Guard(async token=>
 {Need(!begun&&sequence==0,"one profile handshake");await Exchange("begin",capture,null,false,token).ConfigureAwait(false);begun=true;});
 public void RequireCaptureDeclaration(MixedCaptureDeclaration actual)
 {
  lock(gate)try
  {
   Need(begun&&!failed&&!busy&&!disposed,"accepted healthy capture handshake required");
   Need(capture.Matches(actual),"actual sink capture declaration differs from accepted handshake");
  }
  catch{failed=true;throw;}
 }
 public Task ValidateAsync(MixedSinkContextTemplate template)=>Guard(async token=>
 {
  Need(begun&&sequence is >=1 and <=705,"template validation follows handshake");
  Need(template.Id==MixedAuditPlan.RunContextIds()[sequence-1],"exact705 template sequence");
  var report=new{contextId=template.Id,marks=template.Marks.Count,leaves=template.Leaves.Count,callbacks=template.Callbacks.Callbacks.Length,scope=Scope()};
  await Exchange("validate",template.Wire(),report,false,token).ConfigureAwait(false);
 });
 public Task CompleteAsync()=>Guard(async token=>
 {
  Need(begun&&sequence==706,"full705 acknowledgements before completion");
  var report=new{contexts=705,marks=291199,leaves=20316,callbacks=940365,scope=Scope()};
  await Exchange("finish",null,report,true,token).ConfigureAwait(false);
  Need(inputAt==inputCount,"no queued data after final acknowledgement");
  Need(replyBytes<limits.TotalReplyBytes,"terminal EOF probe reservation");replyBytes++;
  stage="terminal-EOF";Need(await replies.ReadAsync(inputBuffer.AsMemory(0,1),token).AsTask().WaitAsync(token).ConfigureAwait(false)==0,"clean terminal reply EOF");
  lock(gate){Need(!failed,"healthy terminal transaction");finished=true;}
 });
 public bool Completed{get{lock(gate)return finished&&!failed&&!disposed;}}
 public bool Failed{get{lock(gate)return failed;}}
 public void Dispose(){lock(gate){if(disposed)return;if(!finished)failed=true;disposed=true;}requests.Dispose();replies.Dispose();}
}
