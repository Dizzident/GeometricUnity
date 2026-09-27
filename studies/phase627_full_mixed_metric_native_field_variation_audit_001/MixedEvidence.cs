using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Algebra;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;
using TensorRecord=System.Collections.Generic.KeyValuePair<(int Form,int Blade,int K0,int K1),Scalar>;

// Metadata-only recording: tensor identities are weak keys; only explicitly
// registered inputs are leaves. Expanded marks are consumed synchronously.
// The outer pack owns the frozen paths and independently validated geometry.
internal static class MixedTrace
{
 public const string Schema="phase627-typed-mixed-dag-v1";
 public sealed class EvidenceFailure(string message):Exception(message);
 public sealed record Limits(int Nodes,int Marks,int TensorRecords,int RationalCharacters,long TensorBytes,long GraphBytes,long PairVisits,long SlotVisits,long LiveRecords);
 public sealed record LeafSpec(string Id,int Degree,string Source,string Sha256);
 public sealed record Node(int Id,string Op,int Degree,int[] Inputs,JsonElement Parameters,int Records,long Bytes,string Sha256);
 public sealed record MarkSpec(string Name,int Degree,int Node,bool Expanded,string Sha256);
 public sealed record Graph(string SchemaVersion,LeafSpec[] Leaves,Node[] Nodes,MarkSpec[] Marks);
 // Cumulative allocation/copy ceilings, NOT live-memory estimates or RSS.
 // No refunds: even a caller retaining every returned graph stays under these
 // logical allocation totals. Library/header/serializer costs remain separate.
 public readonly record struct CopyCounts(long Graphs,long NodeObjects,long Arrays,long Slots,long ElementCopies);
 public sealed record CaptureLimits(CopyCounts Normal,CopyCounts Inspection,CopyCounts Failure);
 public readonly record struct CaptureUsage(CopyCounts Normal,CopyCounts Inspection,CopyCounts Failure,bool NormalFailed,bool InspectionFailed,bool FailureFailed);
 private enum CaptureLane{Normal,Inspection,Failure}
 private sealed class CaptureBudget
 {
  readonly CaptureLimits limits;CopyCounts normal,inspection,failure;bool normalFailed,inspectionFailed,failureFailed;
  public CaptureBudget(CaptureLimits limits)
  {
   ArgumentNullException.ThrowIfNull(limits);Validate(limits.Normal);Validate(limits.Inspection);Validate(limits.Failure);this.limits=limits;
  }
  static void Validate(CopyCounts c)
  {Need(Valid(c.Graphs)&&Valid(c.NodeObjects)&&Valid(c.Arrays)&&Valid(c.Slots)&&Valid(c.ElementCopies),"explicit nonnegative safe capture ceilings required");}
  static bool Valid(long n)=>n>=0&&n<=MaximumSafeInteger;
  public CaptureUsage Usage()=>new(normal,inspection,failure,normalFailed,inspectionFailed,failureFailed);
  public void Admit(CaptureLane lane,CopyCounts cost)
  {
   // Called under Session's ownership gate; all dimensions checked before ANY
   // budget update or graph allocation. A rejected lane cannot be replenished.
   switch(lane)
   {
    case CaptureLane.Normal:Charge(ref normal,ref normalFailed,limits.Normal,cost);break;
    case CaptureLane.Inspection:Charge(ref inspection,ref inspectionFailed,limits.Inspection,cost);break;
    case CaptureLane.Failure:Charge(ref failure,ref failureFailed,limits.Failure,cost);break;
   }
  }
  static void Charge(ref CopyCounts used,ref bool failed,CopyCounts cap,CopyCounts cost)
  {
   if(failed||!Fits(used.Graphs,cost.Graphs,cap.Graphs)||!Fits(used.NodeObjects,cost.NodeObjects,cap.NodeObjects)||!Fits(used.Arrays,cost.Arrays,cap.Arrays)||!Fits(used.Slots,cost.Slots,cap.Slots)||!Fits(used.ElementCopies,cost.ElementCopies,cap.ElementCopies))
   {failed=true;throw new EvidenceFailure("capture lane exhausted before graph allocation");}
   used=new(used.Graphs+cost.Graphs,used.NodeObjects+cost.NodeObjects,used.Arrays+cost.Arrays,used.Slots+cost.Slots,used.ElementCopies+cost.ElementCopies);
  }
  static bool Fits(long used,long cost,long cap)=>cost>=0&&cost<=cap-used;
 }
 // These maxima count dictionaries/entries admitted to the replay node cache
 // (including borrowed leaves), PLUS all primitive scratch: Pullback's form
 // cache, rows and intermediate products included. They exclude caller leaf
 // storage outside that cache, graph/matrix metadata, sorting/hash/serializer
 // buffers and unreduced scalar temporaries. This is NOT a total-memory or
 // RSS bound. Those separate bounds belong to the complete frozen STUDY.
 public sealed record ReplayReport(int Nodes,int Marks,long PairVisits,long SlotVisits,long MaximumLiveRecords,int MaximumLiveTensors);
 private sealed record Identity(int Id);
 public static Session? Active{get=>Session.GetActive();set=>Session.SetActive(value);}
 public static readonly JsonSerializerOptions JsonOptions=new()
 {
  PropertyNamingPolicy=JsonNamingPolicy.CamelCase,WriteIndented=false,
  Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
  UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow
 };
 private static readonly JsonElement Empty=JsonSerializer.SerializeToElement(new{},JsonOptions);
 private const long MaximumSafeInteger=9007199254740991;
 private static bool PrintableAscii(string text)=>text.All(c=>c is >= '\x20' and <= '\x7e');
 public static object MatrixArg(Matrix matrix)
 {
#if A68_GUARDED_FOURIER_TENSOR
  var admission=MixedProducerAdmission.Current;
  if(matrix is null||matrix.N!=14){admission.Poison();throw new EvidenceFailure("matrix dimension");}
  MixedProducerStages.MatrixArgument(matrix);
#else
  if(matrix.N!=14)throw new EvidenceFailure("matrix dimension");
#endif
  // Fixed maximum, not List growth. Keep sparse row-major entries and the
  // original comparison/formatting order even when Active is null.
  var entries=new object[196];int count=0;
  for(int row=0;row<14;row++)for(int column=0;column<14;column++)
   if(matrix[row,column]!=0)entries[count++]=new{row,column,value=matrix[row,column].ToString()};
  var result=new object[count];Array.Copy(entries,result,count);
  return new{matrix=result};
 }
 public static FT Record(string op,FT result,FT[] inputs,object? parameters=null)
 {Session.RecordActive(op,result,inputs,parameters);return result;}
 public static FT Zero()=>Record("zero",new FT(),[]);
 private static void Need([DoesNotReturnIf(false)] bool condition,string message){if(!condition)throw new EvidenceFailure(message);}
 internal static void CheckLimits(Limits limits)
 {
  Need(limits.Nodes>0&&limits.Marks>0&&limits.TensorRecords>0&&limits.RationalCharacters is >0 and <=16384,"positive bounded frozen integer limits required");
  static bool Safe(long value)=>value>0&&value<=MaximumSafeInteger;
  Need(Safe(limits.TensorBytes)&&Safe(limits.GraphBytes)&&Safe(limits.PairVisits)&&Safe(limits.SlotVisits)&&Safe(limits.LiveRecords),"positive cross-runtime safe-integer resource limits required");
 }
 private static void CheckLeaves(LeafSpec[] leaves)
 {
  Need(leaves.Select(x=>x.Id).Distinct(StringComparer.Ordinal).Count()==leaves.Length,"duplicate declared leaf");
  foreach(var leaf in leaves)Need(!string.IsNullOrWhiteSpace(leaf.Id)&&PrintableAscii(leaf.Id)&&!string.IsNullOrWhiteSpace(leaf.Source)&&PrintableAscii(leaf.Source)&&leaf.Degree is >=0 and <=14&&ValidHash(leaf.Sha256),"invalid declared leaf provenance/type/hash");
 }
 private static bool ValidHash(string hash)=>hash.Length==64&&hash.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
 private static Rational Rat(string text,int limit)
 {
#if A68_GUARDED_EXACT_ARITHMETIC
  return MixedCanonicalRational.Parse(text,limit);
#else
  Need(text.Length>0&&text.Length<=limit,"rational length");var value=SpinGeometry.Parse(text);
  Need(value.ToString()==text,"noncanonical rational");return value;
#endif
 }
 private static void Shape(JsonElement element,params string[] names)
 {
  Need(element.ValueKind==JsonValueKind.Object,"operation parameters must be an object");
  Need(element.EnumerateObject().Select(p=>p.Name).SequenceEqual(names),"closed parameter properties/order");
 }
 private static Matrix ReadMatrix(JsonElement parameters,Limits limits)
 {
  Shape(parameters,"matrix");var array=parameters.GetProperty("matrix");Need(array.ValueKind==JsonValueKind.Array&&array.GetArrayLength()<=196,"sparse matrix shape");
  var matrix=new Matrix(14);int previous=-1;foreach(var entry in array.EnumerateArray())
  {
   Shape(entry,"row","column","value");int row=entry.GetProperty("row").GetInt32(),column=entry.GetProperty("column").GetInt32();
   Need(row is >=0 and <14&&column is >=0 and <14&&row*14+column>previous,"canonical sparse matrix indices");
   var value=Rat(entry.GetProperty("value").GetString()!,limits.RationalCharacters);Need(value!=0,"explicit zero matrix entry");matrix[row,column]=value;previous=row*14+column;
  }return matrix;
 }
 private static int DegreeOf(string op,int[] degrees,JsonElement p,Limits limits)
 {
  int Unary(){Need(degrees.Length==1,"unary arity");return degrees[0];}
  void Binary(){Need(degrees.Length==2,"binary arity");}
  switch(op)
  {
   case "leaf":Need(degrees.Length==0,"leaf arity");Shape(p,"id");return -1;
   case "zero":Need(degrees.Length==0,"zero arity");Shape(p);return -1;
   case "unit":
    Need(degrees.Length==0,"unit arity");Shape(p,"form","blade","real","imaginary");
    int form=p.GetProperty("form").GetInt32(),blade=p.GetProperty("blade").GetInt32();Need(form is >=0 and <=16383&&blade is >=0 and <=16383,"unit masks");
    _=Rat(p.GetProperty("real").GetString()!,limits.RationalCharacters);_=Rat(p.GetProperty("imaginary").GetString()!,limits.RationalCharacters);return Degree(form);
   case "sum":Shape(p);Need(degrees.Length>0,"sum arity");var known=degrees.Where(x=>x>=0).Distinct().ToArray();Need(known.Length<=1,"sum degree mismatch");return known.Length==0?-1:known[0];
   case "scale":Shape(p,"real","imaginary");_=Rat(p.GetProperty("real").GetString()!,limits.RationalCharacters);_=Rat(p.GetProperty("imaginary").GetString()!,limits.RationalCharacters);return Unary();
   case "product":case "transpose":
    Binary();Shape(p,"kind");string kind=p.GetProperty("kind").GetString()!;Need(kind.Length==1&&(op=="product"?"WCA":"CA").Contains(kind,StringComparison.Ordinal),"product/transpose kind");
    if(degrees.Any(x=>x<0))return -1;int result=op=="product"?degrees[0]+degrees[1]:degrees[1]-degrees[0];return result is >=0 and <=14?result:-1;
   case "star":Shape(p);int degree=Unary();return degree<0?-1:14-degree;
   case "starAdjoint":Shape(p,"degree");int forward=p.GetProperty("degree").GetInt32();Need(forward is >=0 and <=14,"Hodge adjoint degree");int input=Unary();Need(input<0||input==14-forward,"Hodge adjoint input degree");return forward;
   case "motion":case "motionAdjoint":case "pullback":_=ReadMatrix(p,limits);return Unary();
   case "spin":Need(degrees.Length==0,"spin arity");_=ReadMatrix(p,limits);return 0;
   case "contract":Shape(p,"axis");Need(p.GetProperty("axis").GetInt32() is >=0 and <14,"contraction axis");int d=Unary();return d<=0?-1:d-1;
   case "component":Shape(p,"form");Need(p.GetProperty("form").GetInt32() is >=0 and <=16383,"component mask");_=Unary();return 0;
   case "raise":Shape(p);return Unary();
   default:throw new EvidenceFailure("unregistered operation: "+op);
  }
 }
 private sealed class HashStream(long limit,MixedProducerAdmission.Scope? admission=null,Action<int>? chargeBytes=null):Stream
 {
  private readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  public long Count{get;private set;}
  public override void Write(byte[] buffer,int offset,int count)=>Write(buffer.AsSpan(offset,count));
  public override void Write(ReadOnlySpan<byte> buffer)
  {
   admission?.EnsureActive();Need(Count<=limit-buffer.Length,"serialized byte ceiling");
   if(admission is not null){chargeBytes!(buffer.Length);admission.EnsureActive();}
   hash.AppendData(buffer);Count+=buffer.Length;
  }
  public string Finish(){admission?.EnsureActive();return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();}
  protected override void Dispose(bool disposing){if(disposing)hash.Dispose();base.Dispose(disposing);}
  public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;
  public override long Length=>Count;public override long Position{get=>Count;set=>throw new NotSupportedException();}
  public override void Flush(){}public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override long Seek(long o,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();
 }
 public static void WriteTensor(Utf8JsonWriter writer,FT tensor,int degree,Limits limits)
 {
  var records=PrepareTensor(tensor,degree,limits,false);WritePreparedTensor(writer,records,limits);
 }
 private static TensorRecord[] PrepareTensor(FT tensor,int degree,Limits limits,bool ownsWriter,MixedProducerAdmission.Scope? admission=null)
 {
#if A68_GUARDED_FOURIER_TENSOR
  admission??=MixedProducerAdmission.Current;
#endif
  try
  {
  Need(tensor.Count<=limits.TensorRecords,"tensor support ceiling");
  if(admission is not null)MixedProducerStages.TensorFingerprintAdmitted(tensor.Count,ownsWriter);
  long characters=0;
  // Validate the complete borrowed input before sort/writer allocation. No
  // decimal strings or coefficient arithmetic are needed for this scan.
  foreach(var item in tensor)
  {
   var (form,blade,k0,k1)=item.Key;
   Need(form is >=0 and <=16383&&blade is >=0 and <=16383&&k0==0&&k1==0&&!(item.Value.Real.Numerator.IsZero&&item.Value.Imaginary.Numerator.IsZero),"canonical local tensor record");
   Need(degree>=0&&Degree(form)==degree,"tensor form degree");
   if(admission is not null)
   {
    var real=item.Value.Real;var imaginary=item.Value.Imaginary;
    Need(real.Denominator.Sign>0&&imaginary.Denominator.Sign>0,"positive canonical tensor denominators");
    characters=admission.CountSum(characters,admission.FormattingCharacters(real.Numerator,real.Denominator,real.Denominator.IsOne));
    characters=admission.CountSum(characters,admission.FormattingCharacters(imaginary.Numerator,imaginary.Denominator,imaginary.Denominator.IsOne));
   }
  }
  // Prospective check, not a reusable credit: Rational.ToString still charges
  // every actual formatting call. Per-record exact character limits remain
  // post-format so a loose bit bound never rejects a valid decimal length.
  admission?.RequireFormattingCharacters(characters);
  var records=new TensorRecord[tensor.Count];int slot=0;foreach(var item in tensor)records[slot++]=item;
  SortTensorRecords(records);return records;
  }
  catch{admission?.Poison();throw;}
 }
 private static int KeyOrder((int Form,int Blade,int K0,int K1) a,(int Form,int Blade,int K0,int K1) b)
 {
  int order=a.Form.CompareTo(b.Form);if(order!=0)return order;
  order=a.Blade.CompareTo(b.Blade);if(order!=0)return order;
  order=a.K0.CompareTo(b.K0);return order!=0?order:a.K1.CompareTo(b.K1);
 }
 private static void SiftTensorRecords(TensorRecord[] records,int root,int length)
 {
  while(root<length/2)
  {
   int child=2*root+1;
   if(child+1<length&&KeyOrder(records[child].Key,records[child+1].Key)<0)child++;
   if(KeyOrder(records[root].Key,records[child].Key)>=0)return;
   var temporary=records[root];records[root]=records[child];records[child]=temporary;root=child;
  }
 }
 private static void SortTensorRecords(TensorRecord[] records)
 {
  for(int root=records.Length/2-1;root>=0;root--)SiftTensorRecords(records,root,records.Length);
  for(int end=records.Length-1;end>0;end--)
  {var temporary=records[0];records[0]=records[end];records[end]=temporary;SiftTensorRecords(records,0,end);}
 }
 private static void WritePreparedTensor(Utf8JsonWriter writer,TensorRecord[] records,Limits limits,MixedProducerAdmission.Scope? admission=null)
 {
  writer.WriteStartArray();foreach(var item in records)
  {
   admission?.EnsureActive();
   if(admission is not null)RequireCanonicalFingerprintCulture();
#if !A68_GUARDED_EXACT_ARITHMETIC
   // Guarded Rational.ToString already charges these exact calls. The explicit
   // admitted path must provide the same accounting in ordinary production.
   if(admission is not null)
   {
    admission.Format(item.Value.Real.Numerator,item.Value.Real.Denominator,item.Value.Real.Denominator.IsOne);
    admission.Format(item.Value.Imaginary.Numerator,item.Value.Imaginary.Denominator,item.Value.Imaginary.Denominator.IsOne);
   }
#endif
   var (form,blade,k0,k1)=item.Key;string real=item.Value.Real.ToString(),imaginary=item.Value.Imaginary.ToString();
   Need(real.Length<=limits.RationalCharacters&&imaginary.Length<=limits.RationalCharacters,"tensor rational ceiling");
   writer.WriteStartObject();writer.WriteNumber("form",form);writer.WriteNumber("blade",blade);writer.WriteNumber("k0",0);writer.WriteNumber("k1",0);writer.WriteString("real",real);writer.WriteString("imaginary",imaginary);writer.WriteEndObject();
  }writer.WriteEndArray();
 }
 public static (string Sha256,long Bytes) Fingerprint(FT tensor,int degree,Limits limits)
 {
  var records=PrepareTensor(tensor,degree,limits,true);
  using var stream=new HashStream(limits.TensorBytes);using(var writer=new Utf8JsonWriter(stream,new(){Encoder=JsonOptions.Encoder}))WritePreparedTensor(writer,records,limits);
  return(stream.Finish(),stream.Count);
 }
 // Explicit admission is independent of build symbols. This is a logical
 // primitive envelope, NOT a proof of writer/provider/VM buffers or peak RSS.
 // The caller's byte charge is additional to the scope's actual primitive
 // accounting; it runs before every hash append and cannot replace that scope.
 private static void RequireCanonicalFingerprintCulture()
 {
  // Historical fractional Rational.ToString uses the current negative sign.
  // Keep that legacy API unchanged, but refuse noncanonical signs here. Check
  // again at each record because a byte-charge callback can change culture.
  Need(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NegativeSign=="-","admitted tensor canonical negative sign required");
 }
 public static (string Sha256,long Bytes) FingerprintAdmitted(FT tensor,int degree,Limits limits,Action<int> chargeBytes)
 {
  var admission=MixedProducerAdmission.Current;
  try
  {
   ArgumentNullException.ThrowIfNull(tensor);ArgumentNullException.ThrowIfNull(limits);ArgumentNullException.ThrowIfNull(chargeBytes);
   CheckLimits(limits);Need(degree is >=0 and <=14,"admitted tensor degree0..14");RequireCanonicalFingerprintCulture();
   var records=PrepareTensor(tensor,degree,limits,true,admission);
   using var stream=new HashStream(limits.TensorBytes,admission,chargeBytes);
   using(var writer=new Utf8JsonWriter(stream,new(){Encoder=JsonOptions.Encoder}))WritePreparedTensor(writer,records,limits,admission);
   var result=(stream.Finish(),stream.Count);admission.EnsureActive();return result;
  }
  catch{admission.Poison();throw;}
 }
 public sealed class Session:IDisposable
 {
  private static readonly object ownershipGate=new();
  private static Session? activeSession,mutatingSession;
  private static SinkOwnership? sinkOwner;
  private readonly SinkOwnership? owningSink;
  private readonly CaptureBudget captures;
  private readonly long inspectionLimit;
  private long inspections;
  private long retainedInputSlots;
  private readonly int ownerThread=Environment.CurrentManagedThreadId;
#if A68_GUARDED_FOURIER_TENSOR
  private MixedProducerAdmission.Scope? ownerScope;
#endif
  private readonly LeafSpec[] leaves;private readonly Limits limits;private readonly Action<string,int,FT> expandedSink;private readonly Action<string,JsonElement> parameterValidator;
  private readonly ConditionalWeakTable<FT,Identity> identities=new();private readonly List<Node> nodes=[];private readonly List<MarkSpec> marks=[];private readonly HashSet<string> registered=new(StringComparer.Ordinal);private bool finished,busy,failed,reentered,disposed;private long metadataBytes=4096;
  public Session(CaptureLimits captures,LeafSpec[] declaredLeaves,Limits limits,Action<string,int,FT> expandedSink,Action<string,JsonElement> parameterValidator)
   :this(null,new CaptureBudget(captures),captures.Inspection.Graphs,declaredLeaves,limits,expandedSink,parameterValidator){}
  private Session(SinkOwnership? owner,CaptureBudget captures,long inspectionLimit,LeafSpec[] declaredLeaves,Limits limits,Action<string,int,FT> expandedSink,Action<string,JsonElement> parameterValidator)
  {
   lock(ownershipGate)
   {
    if(!ReferenceEquals(sinkOwner,owner))RejectInterference(null,"session construction requires the sink capability");
    owner?.RequireControl();Need(inspectionLimit>=0&&inspectionLimit<=MaximumSafeInteger,"explicit safe session inspection limit");
    owningSink=owner;this.captures=captures;this.inspectionLimit=inspectionLimit;
   }
   CheckLimits(limits);CheckLeaves(declaredLeaves);leaves=declaredLeaves.ToArray();this.limits=limits;this.expandedSink=expandedSink??throw new ArgumentNullException(nameof(expandedSink));this.parameterValidator=parameterValidator??throw new ArgumentNullException(nameof(parameterValidator));Reserve(leaves);
  }
  // The sink alone retains this capability. Public Active/Dispose cannot
  // temporarily evade it between Session operations. This is API ownership,
  // not isolation against reflection, arbitrary memory mutation or hostile code.
  public sealed class SinkOwnership:IDisposable
  {
   readonly int thread=Environment.CurrentManagedThreadId;
   readonly CaptureBudget captures;
   internal bool failed;
   bool callback,disposed;
   internal SinkOwnership(CaptureLimits limits){captures=new(limits);}
   public void CheckHealthy()
   {lock(ownershipGate){RequireIdentity();Need(!failed,"failed sink trace ownership");}}
   void RequireIdentity()
   {
    if(disposed||!ReferenceEquals(sinkOwner,this)||Environment.CurrentManagedThreadId!=thread)
    {failed=true;RejectInterference(null,"sink capability identity/thread mismatch");}
   }
   public void RequireControl()
   {
    lock(ownershipGate)
    {
     RequireIdentity();
     if(callback||mutatingSession is not null){failed=true;RejectInterference(null,"sink control forbidden during callback/session mutation");}
    }
   }
   internal void RequireComputation(Session? session)
   {
    RequireIdentity();
    if(callback||session is not null&&!ReferenceEquals(session.owningSink,this))
     RejectInterference(session,"sink callback or foreign session cannot produce trace operations");
    Need(!failed,"failed sink cannot produce trace operations");
   }
   public Session CreateSession(long inspectionCopies,LeafSpec[] leaves,Limits limits,Action<string,int,FT> expanded,Action<string,JsonElement> validator)
   {
    lock(ownershipGate){RequireControl();CheckHealthy();}
    try{return new Session(this,captures,inspectionCopies,leaves,limits,expanded,validator);}
    catch{lock(ownershipGate)failed=true;throw;}
   }
   public void Activate(Session? session)
   {
    lock(ownershipGate)
    {
     RequireControl();CheckHealthy();
     if(session is not null&&(!ReferenceEquals(session.owningSink,this)||session.failed||session.disposed))
      RejectInterference(null,"sink activation requires its healthy live session");
     activeSession=session; // Healthy finished points remain valid tripwires.
    }
   }
   public void Detach()
   {lock(ownershipGate){RequireControl();activeSession=null;}}
   public void DisposeSession(Session session)
   {
    lock(ownershipGate)
    {
     RequireControl();
     if(!ReferenceEquals(session.owningSink,this))RejectInterference(null,"cannot clean up another owner's session");
     session.DisposeCore();
    }
   }
   public CaptureUsage CaptureUsage()
   {lock(ownershipGate){RequireIdentity();return captures.Usage();}}
   public Graph FailureSnapshot(Session session)
   {
    lock(ownershipGate)
    {
     RequireControl();
     if(!ReferenceEquals(session.owningSink,this))RejectInterference(null,"failure capture requires its owning sink");
     // Explicit failure-only action: this cannot be used to resume science.
     failed=true;session.failed=true;return session.CaptureGraph(CaptureLane.Failure);
    }
   }
   public T Callback<T>(Func<T> action)
   {
    // Entry is outside finally: rejected nested entry must not lift the fence.
    lock(ownershipGate)
    {
     CheckHealthy();if(callback)RejectInterference(null,"nested prerequisite callback");callback=true;
    }
    try{T result=action();CheckHealthy();return result;}
    catch{lock(ownershipGate)failed=true;throw;}
    finally{lock(ownershipGate)callback=false;}
   }
   public void Callback(Action action)=>Callback(()=>{action();return true;});
   public void Dispose()
   {
    lock(ownershipGate)
    {
     if(disposed)return;RequireControl();activeSession=null;disposed=true;sinkOwner=null;
    }
   }
  }
  public static SinkOwnership AcquireSinkOwnership(CaptureLimits captures)
  {
   lock(ownershipGate)
   {
    if(sinkOwner is not null||activeSession is not null||mutatingSession is not null)
     RejectInterference(null,"exclusive sink trace ownership required");
    return sinkOwner=new SinkOwnership(captures);
   }
  }
  internal static void RecordActive(string op,FT result,FT[] inputs,object? parameters)
  {
   Session? session;
   lock(ownershipGate){sinkOwner?.RequireComputation(activeSession);session=activeSession;}
   session?.Record(op,result,inputs,parameters);
  }
  internal static Session? GetActive(){lock(ownershipGate)return activeSession;}
  internal static void SetActive(Session? value)
  {
   lock(ownershipGate)
   {
    if(sinkOwner is not null||value?.owningSink is not null)RejectInterference(null,"active trace requires its sink capability");
    if(mutatingSession is not null)RejectInterference(null,"active trace cannot change during session mutation");
    activeSession?.RequireOwnerThread();value?.RequireOwnerThread();
    Need(value is null||(!value.failed&&!value.disposed),"failed or disposed session cannot become active");
    // Finished, healthy points remain legal tripwires between child contexts.
    activeSession=value;
   }
  }
  // Called with the gate held. Rejection latches the in-flight owner even if
  // a callback catches the exception, including cross-session/thread calls.
  private static void RejectInterference(Session? target,string message)
  {
   if(sinkOwner is not null)sinkOwner.failed=true;
   if(mutatingSession is not null){mutatingSession.failed=true;mutatingSession.reentered=true;}
   if(target is not null)target.failed=true;
   throw new EvidenceFailure(message);
  }
  private void RequireOwnerThread()
  {if(Environment.CurrentManagedThreadId!=ownerThread)RejectInterference(this,"session requires its creating thread");}
#if A68_GUARDED_FOURIER_TENSOR
  private void RequireOwnerScope()
  {
   var scope=MixedProducerAdmission.Current;
   if(ownerScope is null)ownerScope=scope;
   else Need(ReferenceEquals(scope,ownerScope),"session arithmetic scope cannot be replaced");
  }
#endif
  // The gate protects ownership/state, NOT callbacks: no lock spans external
  // getters/validators/sinks. A foreign call can refuse without deadlocking a
  // callback waiting for it. Begin is outside each public try/finally so a
  // rejected nested call cannot release the outer mutation lease.
  private void BeginMutation()
  {
   lock(ownershipGate)try
   {
    if(mutatingSession is not null)RejectInterference(this,"reentrant or cross-session mutation");
    RequireOwnerThread();activeSession?.RequireOwnerThread();
    if(owningSink is not null)owningSink.RequireComputation(this);
    else sinkOwner?.RequireComputation(this);
    Need(!finished&&!failed&&!disposed,"closed or failed session cannot mutate");
#if A68_GUARDED_FOURIER_TENSOR
    RequireOwnerScope();
#endif
    busy=true;mutatingSession=this;
   }
   catch{failed=true;if(owningSink is not null)owningSink.failed=true;throw;}
  }
  private void CheckMutation()
  {
   lock(ownershipGate)
   {
    RequireOwnerThread();Need(ReferenceEquals(mutatingSession,this)&&busy&&!finished&&!failed&&!reentered,"failed or reentered session mutation");
    owningSink?.RequireComputation(this);
#if A68_GUARDED_FOURIER_TENSOR
    // Includes swallowed quota failures and replacement with a fresh scope.
    RequireOwnerScope();
#endif
   }
  }
  private void FailMutation(){lock(ownershipGate){failed=true;if(owningSink is not null)owningSink.failed=true;}}
  private void EndMutation()
  {lock(ownershipGate){Need(ReferenceEquals(mutatingSession,this)&&busy,"mutation ownership lost");busy=false;mutatingSession=null;}}
  private void Reserve<T>(T value)
  {Need(metadataBytes<limits.GraphBytes,"graph metadata pre-retention ceiling");using var counter=new HashStream(limits.GraphBytes-metadataBytes);JsonSerializer.Serialize(counter,value,JsonOptions);metadataBytes=checked(metadataBytes+counter.Count+1);Need(metadataBytes<=limits.GraphBytes,"graph metadata pre-retention ceiling");}
  private void CheckNodeCapacity(long required)
  {Need(!finished&&required>=0&&required<=(long)limits.Nodes-nodes.Count,"closed session or prospective node ceiling");}
  private void CheckRecordOperands(FT[] inputs)
  {
   CheckNodeCapacity(1);Need(inputs is not null,"record operands required");
#if A68_GUARDED_FOURIER_TENSOR
   MixedProducerStages.RecorderOperandScan(inputs.Length);
#endif
   long required=1;
   for(int i=0;i<inputs.Length;i++)
   {
    var value=inputs[i];Need(value is not null,"record operand required");
    if(identities.TryGetValue(value,out _))continue;
    Need(value.Count==0,"unregistered NONZERO operand; implicit leaves are forbidden");
    bool repeated=false;for(int j=0;j<i;j++)if(ReferenceEquals(inputs[j],value)){repeated=true;break;}
    if(!repeated)CheckNodeCapacity(++required);
   }
   // All capacity/domain checks precede allocation and mutating Operand calls.
#if A68_GUARDED_FOURIER_TENSOR
   MixedProducerStages.RecorderOperandArrays(inputs.Length);
#endif
  }
  private void CheckMarkOperand(FT value)
  {
   Need(value is not null,"mark operand required");
   if(identities.TryGetValue(value,out _))return;
   Need(value.Count==0,"unregistered NONZERO operand; implicit leaves are forbidden");CheckNodeCapacity(1);
  }
  private int Operand(FT value)
  {
   if(identities.TryGetValue(value,out var identity))return identity.Id;
   Need(value.Count==0,"unregistered NONZERO operand; implicit leaves are forbidden");return Append("zero",value,[],Empty,-1);
  }
  private int Append(string op,FT value,int[] inputs,JsonElement parameters,int degree)
  {
   CheckMutation();
   Need(!finished&&nodes.Count<limits.Nodes,"closed session or node ceiling");var fingerprint=Fingerprint(value,degree,limits);int id=nodes.Count;
   var node=new Node(id,op,degree,inputs,parameters,value.Count,fingerprint.Bytes,fingerprint.Sha256);Reserve(node);
   lock(ownershipGate){CheckMutation();long nextInputs=checked(retainedInputSlots+inputs.Length);nodes.Add(node);retainedInputSlots=nextInputs;identities.Remove(value);identities.Add(value,new(id));return id;}
  }
  public void RegisterLeaf(string id,int degree,FT value)
  {
   BeginMutation();try
   {
   Need(!finished,"closed session cannot register leaves");
   CheckNodeCapacity(1);
   var spec=leaves.SingleOrDefault(x=>x.Id==id);Need(spec is not null&&spec.Degree==degree&&registered.Add(id),"undeclared/repeated leaf or wrong degree");
   Need(!identities.TryGetValue(value,out _),"leaf cannot relabel a recorded intermediate");var hash=Fingerprint(value,degree,limits);Need(hash.Sha256==spec!.Sha256,"declared leaf hash mismatch");
   Append("leaf",value,[],JsonSerializer.SerializeToElement(new{id},JsonOptions),degree);
   }
   catch{FailMutation();throw;}finally{EndMutation();}
  }
  public FT Record(string op,FT result,FT[] inputs,object? parameters=null)
  {
   BeginMutation();try
   {
   // A sealed point remains the active tripwire between child contexts.
   // Reject before operand traversal, parameter getters/serialization or
   // provenance callbacks; it cannot be reopened for point computation.
   Need(!finished,"closed session cannot record operations");
   Need(op!="leaf","leaf operation requires explicit RegisterLeaf");CheckRecordOperands(inputs);
   var source=new int[inputs.Length];for(int i=0;i<inputs.Length;i++)source[i]=Operand(inputs[i]);
   var p=parameters is null?Empty:JsonSerializer.SerializeToElement(parameters,JsonOptions);
   CheckMutation(); // A parameter getter may have swallowed a reentry failure.
   // Validator MUST bind scale/unit/spin/matrix parameters to the declared
   // constant or independently reconstructed geometry/recipe menu, not their
   // shapes or values copied from the recorded nodes. Computed coefficients
   // cannot be smuggled into this graph as opaque unit/scale constants.
   var degrees=new int[source.Length];for(int i=0;i<source.Length;i++)degrees[i]=nodes[source[i]].Degree;
   int degree=DegreeOf(op,degrees,p,limits);parameterValidator(op,p);CheckMutation();Append(op,result,source,p,degree);return result;
   }
   catch{FailMutation();throw;}finally{EndMutation();}
  }
  public void Mark(string name,int degree,FT value,bool expanded)
  {
   BeginMutation();try
   {
   Need(!finished&&marks.Count<limits.Marks&&!string.IsNullOrWhiteSpace(name)&&PrintableAscii(name)&&marks.All(x=>x.Name!=name),"closed session, duplicate mark or mark ceiling");
   CheckMarkOperand(value);
   int id=Operand(value);Need(degree is >=0 and <=14&&(nodes[id].Degree<0||nodes[id].Degree==degree),"mark degree");var hash=Fingerprint(value,degree,limits);Need(hash.Sha256==nodes[id].Sha256,"recorded tensor mutated before mark");
   var mark=new MarkSpec(name,degree,id,expanded,hash.Sha256);Reserve(mark);if(expanded)expandedSink(name,degree,value);
   lock(ownershipGate){CheckMutation();marks.Add(mark);}
   }
   catch{FailMutation();throw;}finally{EndMutation();}
  }
  public Graph Finish()
  {
   BeginMutation();try
   {
    Need(registered.Count==leaves.Length,"all and only declared leaves must be registered");
    Graph graph;lock(ownershipGate){CheckMutation();graph=CaptureGraph(CaptureLane.Normal);}using var counter=new HashStream(limits.GraphBytes);JsonSerializer.Serialize(counter,graph,JsonOptions);counter.WriteByte(10);
    lock(ownershipGate){CheckMutation();finished=true;}return graph;
   }
   catch{FailMutation();throw;}finally{EndMutation();}
  }
  private Graph CaptureGraph(CaptureLane lane)
  {
   // O(1) admission precedes traversal/allocation; input slots are maintained
   // at node commit, not counted by an unadmitted diagnostic pre-scan.
   long slots=checked((long)nodes.Count+leaves.Length+marks.Count+retainedInputSlots);
   if(lane==CaptureLane.Inspection&&owningSink is not null)Need(inspections<inspectionLimit,"context inspection policy exhausted before copying");
   captures.Admit(lane,new(1,nodes.Count,checked(3L+nodes.Count),slots,slots));
   if(lane==CaptureLane.Inspection)inspections++;
   // Each admitted element copy has one source read and one destination write;
   // zero initialization adds at most Slots logical writes. Node record field
   // copies, header initialization and allocator/library internals are separate.
   var copy=new Node[nodes.Count];for(int i=0;i<copy.Length;i++)
   {
    var node=nodes[i];var inputs=new int[node.Inputs.Length];
    for(int j=0;j<inputs.Length;j++)inputs[j]=node.Inputs[j];
    copy[i]=node with{Inputs=inputs};
   }
   var leafCopy=new LeafSpec[leaves.Length];for(int i=0;i<leafCopy.Length;i++)leafCopy[i]=leaves[i];
   var markCopy=new MarkSpec[marks.Count];for(int i=0;i<markCopy.Length;i++)markCopy[i]=marks[i];
   return new(Schema,leafCopy,copy,markCopy);
  }
  // Failure preservation only, NOT Finish: incomplete leaves/marks remain
  // visible. The failure record must label this incomplete; normal scientific
  // replay must still reject missing closure. Inspection cannot consume the
  // separately frozen failure allowance. No arithmetic scope is needed here.
  public Graph Snapshot()
  {
   lock(ownershipGate)try{RequireOwnerThread();return CaptureGraph(CaptureLane.Inspection);}
   catch{FailMutation();throw;}
  }
  public CaptureUsage CaptureUsage(){lock(ownershipGate){RequireOwnerThread();return captures.Usage();}}
  public void Dispose()
  {
   lock(ownershipGate)
   {
    if(owningSink is not null||sinkOwner is not null)RejectInterference(this,"session disposal requires its sink capability");
    if(mutatingSession is not null)RejectInterference(this,"reentrant or cross-session disposal");
    DisposeCore();
   }
  }
  private void DisposeCore()
  {
   RequireOwnerThread();
   // Cleanup needs no healthy arithmetic scope and cannot clear another owner.
   if(ReferenceEquals(activeSession,this))activeSession=null;finished=true;disposed=true;
  }
 }
 public static void WriteGraph(Stream destination,Graph graph,Limits limits)
 {
  using(var counter=new HashStream(limits.GraphBytes)){JsonSerializer.Serialize(counter,graph,JsonOptions);counter.WriteByte(10);}
  JsonSerializer.Serialize(destination,graph,JsonOptions);destination.WriteByte(10);
 }
 public static Graph ReadGraph(ReadOnlyMemory<byte> bytes,Limits limits,string expectedSha256)
 {
  CheckLimits(limits);Need(bytes.Length>1&&bytes.Length<=limits.GraphBytes&&bytes.Span[^1]==10&&bytes.Span[^2]!=10,"graph bytes/LF ceiling");
  Need(ValidHash(expectedSha256)&&Convert.ToHexString(SHA256.HashData(bytes.Span)).ToLowerInvariant()==expectedSha256,"independently bound wire hash mismatch");
  // The byte ceiling precedes BOTH parsing passes. Depth16 exceeds the closed
  // schema's nesting, not an arbitrary recursive-JSON allowance. This bounds
  // input/cardinality, NOT RSS: caller bytes, DOM, DTO arrays, parameter clones
  // and serializer buffers coexist and need a separate frozen memory budget.
  const int depth=16;
  var reader=new Utf8JsonReader(bytes.Span,new JsonReaderOptions{MaxDepth=depth,CommentHandling=JsonCommentHandling.Disallow,AllowTrailingCommas=false});
  while(reader.Read())
  {
   if(reader.TokenType is not (JsonTokenType.PropertyName or JsonTokenType.EndObject or JsonTokenType.EndArray))
    Need(reader.CurrentDepth<depth,"graph value depth16");
   if(reader.TokenType==JsonTokenType.Number)
   {
    Need(reader.TryGetInt64(out var number)&&number>=-MaximumSafeInteger&&number<=MaximumSafeInteger,"graph cross-runtime safe integer range/type");
    var canonical=System.Text.Encoding.UTF8.GetBytes(number.ToString(System.Globalization.CultureInfo.InvariantCulture));
    Need(reader.ValueSpan.SequenceEqual(canonical),"noncanonical graph integer");
   }
   else if(reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName)
   {
    string decoded=reader.GetString()!;Need(PrintableAscii(decoded),"graph decoded printable ASCII string domain");
    var canonical=JsonSerializer.SerializeToUtf8Bytes(decoded,JsonOptions);
    Need(reader.ValueSpan.SequenceEqual(canonical.AsSpan(1,canonical.Length-2)),"noncanonical graph string escape");
   }
  }
  using var document=JsonDocument.Parse(bytes,new JsonDocumentOptions{MaxDepth=depth,CommentHandling=JsonCommentHandling.Disallow,AllowTrailingCommas=false});
  var root=document.RootElement;Shape(root,"schemaVersion","leaves","nodes","marks");
  string Text(JsonElement value){Need(value.ValueKind==JsonValueKind.String,"required graph string");return value.GetString()!;}
  int Integer(JsonElement value){Need(value.ValueKind==JsonValueKind.Number&&value.TryGetInt32(out _),"required graph Int32");return value.GetInt32();}
  int ArrayCount(JsonElement value,long ceiling){Need(value.ValueKind==JsonValueKind.Array&&value.GetArrayLength()<=ceiling,"graph array ceiling/type");return value.GetArrayLength();}
  Need(Text(root.GetProperty("schemaVersion"))==Schema,"graph schema version");
  var leafArray=root.GetProperty("leaves");var nodeArray=root.GetProperty("nodes");var markArray=root.GetProperty("marks");
  int nodeCount=ArrayCount(nodeArray,limits.Nodes);_=ArrayCount(leafArray,nodeCount);_=ArrayCount(markArray,limits.Marks);
  var leafDegrees=new Dictionary<string,int>(StringComparer.Ordinal);
  foreach(var leaf in leafArray.EnumerateArray())
  {
   Shape(leaf,"id","degree","source","sha256");string id=Text(leaf.GetProperty("id"));int degree=Integer(leaf.GetProperty("degree"));
   Need(!string.IsNullOrWhiteSpace(id)&&degree is >=0 and <=14&&!string.IsNullOrWhiteSpace(Text(leaf.GetProperty("source")))&&ValidHash(Text(leaf.GetProperty("sha256")))&&leafDegrees.TryAdd(id,degree),"graph declared leaf identity/type/provenance");
  }
  var degrees=new int[nodeCount];var hashes=new string[nodeCount];var seenLeaves=new HashSet<string>(StringComparer.Ordinal);int index=0;
  foreach(var node in nodeArray.EnumerateArray())
  {
   Shape(node,"id","op","degree","inputs","parameters","records","bytes","sha256");
   Need(Integer(node.GetProperty("id"))==index,"graph node sequence");string op=Text(node.GetProperty("op"));int degree=Integer(node.GetProperty("degree"));
   int records=Integer(node.GetProperty("records"));long tensorBytes=node.GetProperty("bytes").GetInt64();string hash=Text(node.GetProperty("sha256"));
   Need(degree is >=-1 and <=14&&records>=0&&records<=limits.TensorRecords&&tensorBytes>=2&&tensorBytes<=limits.TensorBytes&&ValidHash(hash),"graph node metadata bounds");
   var sources=node.GetProperty("inputs");_=ArrayCount(sources,limits.GraphBytes/2);var inputDegrees=new int[sources.GetArrayLength()];int slot=0;
   foreach(var source in sources.EnumerateArray()){int id=Integer(source);Need(id>=0&&id<index,"graph input topology");inputDegrees[slot++]=degrees[id];}
   var parameters=node.GetProperty("parameters");int inferred=DegreeOf(op,inputDegrees,parameters,limits);
   if(op=="leaf")
   {string id=Text(parameters.GetProperty("id"));Need(leafDegrees.TryGetValue(id,out inferred)&&seenLeaves.Add(id),"graph undeclared/repeated leaf node");}
   Need(degree==inferred&&(degree>=0||records==0),"graph node degree inference");degrees[index]=degree;hashes[index]=hash;index++;
  }
  Need(seenLeaves.Count==leafDegrees.Count,"graph declared leaf census");var markNames=new HashSet<string>(StringComparer.Ordinal);
  foreach(var mark in markArray.EnumerateArray())
  {
   Shape(mark,"name","degree","node","expanded","sha256");string name=Text(mark.GetProperty("name"));int degree=Integer(mark.GetProperty("degree")),id=Integer(mark.GetProperty("node"));
   Need(!string.IsNullOrWhiteSpace(name)&&markNames.Add(name)&&degree is >=0 and <=14&&id>=0&&id<nodeCount,"graph mark identity/type");
   Need((degrees[id]<0||degree==degrees[id])&&Text(mark.GetProperty("sha256"))==hashes[id]&&mark.GetProperty("expanded").ValueKind is JsonValueKind.True or JsonValueKind.False,"graph mark metadata");
  }
  // Every object above, including each closed operation/matrix-entry object
  // in DegreeOf, passed exact ordered Shape: duplicate, missing and unknown
  // keys are rejected before deserialization can apply defaults/last-wins.
  var graph=JsonSerializer.Deserialize<Graph>(document.RootElement,JsonOptions)??throw new EvidenceFailure("missing graph");
  using var stream=new HashStream(limits.GraphBytes);JsonSerializer.Serialize(stream,graph,JsonOptions);stream.WriteByte(10);
  Need(stream.Count==bytes.Length&&stream.Finish()==Convert.ToHexString(SHA256.HashData(bytes.Span)).ToLowerInvariant(),"noncanonical graph wire format");return graph;
 }
 // Independent finite interpreter. No Mixed/Caa operators or recorded result
 // tensors are inputs. Every node is recomputed, checked, then last-use released.
 // Arithmetic replay alone does NOT establish physical expression semantics:
 // the caller must independently validate the expected recipe/input topology
 // or reconstruct the named physical marks, plus the exact leaf/mark/path
 // census. A parameter whitelist and matching recorded hashes cannot do so.
 public static ReplayReport Replay(Graph graph,LeafSpec[] expectedLeaves,Func<LeafSpec,FT> resolveLeaf,Limits limits,Action<string,JsonElement> validateParameters,Action<MarkSpec,FT> compareMark)
 {
  CheckLimits(limits);CheckLeaves(expectedLeaves);Need(graph.SchemaVersion==Schema&&graph.Leaves.SequenceEqual(expectedLeaves),"graph/declared input provenance mismatch");
  Need(graph.Nodes.Length<=limits.Nodes&&graph.Marks.Length<=limits.Marks,"graph census ceilings");Need(compareMark is not null&&validateParameters is not null&&resolveLeaf is not null,"explicit replay prerequisites required");
  var marks=new Dictionary<int,List<MarkSpec>>();var names=new HashSet<string>(StringComparer.Ordinal);var last=Enumerable.Range(0,graph.Nodes.Length).ToArray();var seenLeaves=new HashSet<string>(StringComparer.Ordinal);
  for(int i=0;i<graph.Nodes.Length;i++){var n=graph.Nodes[i];Need(n.Id==i&&n.Inputs.All(s=>s>=0&&s<i),"strict DAG topology");foreach(int input in n.Inputs)last[input]=i;}
  foreach(var mark in graph.Marks){Need(!string.IsNullOrWhiteSpace(mark.Name)&&mark.Node>=0&&mark.Node<graph.Nodes.Length&&mark.Degree is >=0 and <=14&&names.Add(mark.Name)&&ValidHash(mark.Sha256),"mark identity/type");if(!marks.TryGetValue(mark.Node,out var list))marks.Add(mark.Node,list=[]);list.Add(mark);}
  var live=new Dictionary<int,FT>();var scratch=new HashSet<FT>(ReferenceEqualityComparer.Instance);long liveRecords=0,scratchRecords=0,maxRecords=0,pairVisits=0,slotVisits=0;int maxTensors=0,markCount=0;
  void Peak(){Need(liveRecords+scratchRecords<=limits.LiveRecords,"replay live plus primitive scratch record ceiling");maxRecords=Math.Max(maxRecords,liveRecords+scratchRecords);maxTensors=Math.Max(maxTensors,live.Count+scratch.Count);}
  FT Create(){var t=new FT();scratch.Add(t);Peak();return t;}
  void Drop(FT t){Need(scratch.Remove(t),"untracked/repeated primitive scratch release");scratchRecords-=t.Count;}
  void Pairs(FT a,FT b){long n=checked((long)a.Count*b.Count);Need(pairVisits<=limits.PairVisits-n,"replay raw pair ceiling BEFORE loop");pairVisits+=n;}
  void Slots(long count){Need(count>=0&&slotVisits<=limits.SlotVisits-count,"replay slot ceiling BEFORE loop");slotVisits+=count;}
  void Height(Rational value)
  {
   long numerator=System.Numerics.BigInteger.Abs(value.Numerator).GetBitLength(),denominator=value.Denominator.GetBitLength();
   Need(numerator<=4L*limits.RationalCharacters&&denominator<=4L*limits.RationalCharacters,"replay transient rational bit ceiling");
   // log10(2)<1/3. This sufficient fast path avoids decimal allocation at
   // every accumulation; the fallback enforces the EXACT character ceiling.
   if(numerator+denominator>3L*(limits.RationalCharacters-4))Need(value.ToString().Length<=limits.RationalCharacters,"replay transient rational ceiling");
  }
  void Put(FT t,(int Form,int Blade,int K0,int K1) key,Scalar value)
  {
   Need(scratch.Contains(t),"interpreter mutation of an input/live tensor");if(value.IsZero)return;
   Height(value.Real);Height(value.Imaginary);bool existed=t.TryGetValue(key,out var old);var sum=old+value;Height(sum.Real);Height(sum.Imaginary);
   if(sum.IsZero){if(t.Remove(key))scratchRecords--;}
   else{if(!existed){Need(t.Count<limits.TensorRecords,"replay support ceiling BEFORE entry insertion");Need(liveRecords+scratchRecords<limits.LiveRecords,"replay record ceiling BEFORE entry insertion");}t[key]=sum;if(!existed)scratchRecords++;}Peak();
  }
  int Wedge(int a,int b){int inversions=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)for(int j=0;j<i;j++)if((b&(1<<j))!=0)inversions++;return inversions%2==0?1:-1;}
  int Metric(int mask)=>Degree(mask&0x3f80)%2==0?1:-1;
  FT Scale(FT a,Scalar factor){Slots(a.Count);var r=Create();foreach(var x in a)Put(r,x.Key,x.Value*factor);return r;}
  FT Product(FT a,FT b,char kind)
  {
   Pairs(a,b);var r=Create();foreach(var x in a)foreach(var y in b)
   {if((x.Key.Form&y.Key.Form)!=0)continue;int ab=WordSign(x.Key.Blade,y.Key.Blade),ba=WordSign(y.Key.Blade,x.Key.Blade);var c=x.Value*y.Value*(Wedge(x.Key.Form,y.Key.Form)*(kind=='W'?ab:kind=='C'?ab-ba:ab+ba));if(kind=='A')c*=Scalar.I;Put(r,(x.Key.Form|y.Key.Form,x.Key.Blade^y.Key.Blade,0,0),c);}return r;
  }
  FT Transpose(FT a,FT y,char kind)
  {
   Pairs(a,y);var r=Create();foreach(var x in a)foreach(var z in y)
   {if((x.Key.Form&z.Key.Form)!=x.Key.Form)continue;int input=z.Key.Form^x.Key.Form;int xz=WordSign(x.Key.Blade,z.Key.Blade),zx=WordSign(z.Key.Blade,x.Key.Blade);var c=x.Value*z.Value*(Metric(x.Key.Form)*Wedge(x.Key.Form,input)*(kind=='C'?zx-xz:xz+zx));if(kind=='A')c*=Scalar.I;Put(r,(input,x.Key.Blade^z.Key.Blade,0,0),c);}return r;
  }
  FT Motion(Matrix matrix,FT a,bool adjoint)
  {
   long visits=0;foreach(var q in a)visits=checked(visits+14L*Degree(q.Key.Form));Slots(visits);var r=Create();foreach(var q in a)
   {int[] axes=Enumerable.Range(0,14).Where(i=>(q.Key.Form&(1<<i))!=0).ToArray();for(int slot=0;slot<axes.Length;slot++)for(int replacement=0;replacement<14;replacement++)
    {int old=axes[slot];var c=adjoint?matrix[replacement,old]*Sigma(old)*Sigma(replacement):matrix[old,replacement];if(c==0)continue;int mask=0,sign=1;for(int j=0;j<axes.Length;j++){int axis=j==slot?replacement:axes[j];if((mask&(1<<axis))!=0){sign=0;break;}sign*=Wedge(mask,1<<axis);mask|=1<<axis;}if(sign!=0)Put(r,(mask,q.Key.Blade,0,0),q.Value*new Scalar(c*sign,0));}}
   return r;
  }
  FT Pullback(Matrix matrix,FT a)
  {
   var r=Create();var formCache=new Dictionary<int,FT>();foreach(var q in a)
   {
    if(!formCache.TryGetValue(q.Key.Form,out var form))
    {form=Create();Put(form,(0,0,0,0),1);Slots(checked(14L*Degree(q.Key.Form)));for(int axis=0;axis<14;axis++)if((q.Key.Form&(1<<axis))!=0){var row=Create();for(int j=0;j<14;j++)if(matrix[axis,j]!=0)Put(row,(1<<j,0,0,0),new Scalar(matrix[axis,j],0));var next=Product(form,row,'W');Drop(form);Drop(row);form=next;}formCache.Add(q.Key.Form,form);}
    Slots(form.Count);foreach(var x in form)Put(r,(x.Key.Form,q.Key.Blade,0,0),x.Value*q.Value);
   }foreach(var form in formCache.Values)Drop(form);return r;
  }
  for(int i=0;i<graph.Nodes.Length;i++)
  {
   var n=graph.Nodes[i];int degree=DegreeOf(n.Op,n.Inputs.Select(s=>graph.Nodes[s].Degree).ToArray(),n.Parameters,limits);validateParameters(n.Op,n.Parameters);FT result;
   FT Input(int index)=>live[n.Inputs[index]];
   if(n.Op=="leaf")
   {string id=n.Parameters.GetProperty("id").GetString()!;var spec=expectedLeaves.SingleOrDefault(x=>x.Id==id);Need(spec is not null&&seenLeaves.Add(id),"undeclared/repeated replay leaf");degree=spec!.Degree;result=resolveLeaf(spec);Need(result.Count<=limits.TensorRecords&&liveRecords<=limits.LiveRecords-result.Count,"replay leaf support/liveness ceiling BEFORE admission");Need(Fingerprint(result,degree,limits).Sha256==spec.Sha256,"reconstructed leaf mismatch");}
   else switch(n.Op)
   {
    case "zero":result=Create();break;
    case "unit":result=Create();Put(result,(n.Parameters.GetProperty("form").GetInt32(),n.Parameters.GetProperty("blade").GetInt32(),0,0),new(Rat(n.Parameters.GetProperty("real").GetString()!,limits.RationalCharacters),Rat(n.Parameters.GetProperty("imaginary").GetString()!,limits.RationalCharacters)));break;
    case "sum":Slots(n.Inputs.Aggregate(0L,(count,source)=>checked(count+live[source].Count)));result=Create();foreach(int source in n.Inputs)foreach(var q in live[source])Put(result,q.Key,q.Value);break;
    case "scale":result=Scale(Input(0),new(Rat(n.Parameters.GetProperty("real").GetString()!,limits.RationalCharacters),Rat(n.Parameters.GetProperty("imaginary").GetString()!,limits.RationalCharacters)));break;
    case "product":result=Product(Input(0),Input(1),n.Parameters.GetProperty("kind").GetString()![0]);break;
    case "transpose":result=Transpose(Input(0),Input(1),n.Parameters.GetProperty("kind").GetString()![0]);break;
    case "motion":case "motionAdjoint":result=Motion(ReadMatrix(n.Parameters,limits),Input(0),n.Op=="motionAdjoint");break;
    case "pullback":result=Pullback(ReadMatrix(n.Parameters,limits),Input(0));break;
    case "spin":
     var matrix=ReadMatrix(n.Parameters,limits);Slots(91);result=Create();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(result,(0,(1<<a)|(1<<b),0,0),new Scalar(matrix[b,a]*new Rational(-Sigma(a),2),0));break;
    default:
     Slots(Input(0).Count);result=Create();foreach(var q in Input(0))
     {
      int form=q.Key.Form;var c=q.Value;
      if(n.Op is "star" or "starAdjoint"){c*=Wedge(form,16383^form)*Metric(form);form^=16383;if(n.Op=="starAdjoint"){int d=n.Parameters.GetProperty("degree").GetInt32();if(d*(14-d)%2!=0)c*=-1;}}
      else if(n.Op=="raise")c*=Metric(form);
      else if(n.Op=="component"){if(form!=n.Parameters.GetProperty("form").GetInt32())continue;form=0;}
      else if(n.Op=="contract"){int axis=n.Parameters.GetProperty("axis").GetInt32();if((form&(1<<axis))==0)continue;c*=Degree(form&((1<<axis)-1))%2==0?1:-1;form^=1<<axis;}
      else throw new EvidenceFailure("closed interpreter menu");Put(result,(form,q.Key.Blade,0,0),c);
     }break;
   }
   Need(n.Degree==degree&&n.Records==result.Count&&n.Records<=limits.TensorRecords&&ValidHash(n.Sha256),"node type/support metadata");var fingerprint=Fingerprint(result,degree,limits);Need(fingerprint.Sha256==n.Sha256&&fingerprint.Bytes==n.Bytes,"independently reconstructed intermediate mismatch");
   if(scratch.Contains(result))Drop(result);Need(scratch.Count==0&&scratchRecords==0,"all primitive temporaries must be released");
   live.Add(i,result);liveRecords=checked(liveRecords+result.Count);Peak();
   if(marks.TryGetValue(i,out var current))foreach(var mark in current){Need(degree<0||mark.Degree==degree,"replay mark type");Need(mark.Sha256==n.Sha256,"mark fingerprint mismatch");compareMark(mark,result);markCount++;}
   foreach(int source in n.Inputs.Distinct())if(last[source]==i){liveRecords-=live[source].Count;live.Remove(source);}
   if(last[i]==i){liveRecords-=result.Count;live.Remove(i);}
  }
  Need(seenLeaves.Count==expectedLeaves.Length&&live.Count==0&&liveRecords==0&&markCount==graph.Marks.Length,"complete replay leaf/mark/liveness census");return new(graph.Nodes.Length,markCount,pairVisits,slotVisits,maxRecords,maxTensors);
 }
}
