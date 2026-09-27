using System.Security.Cryptography;
using System.Text.Json;

internal sealed record MixedGeometryWireLimits(int RationalCharacters,int PayloadBytes,long TotalPayloadBytes,long Snapshots);
internal readonly record struct MixedGeometryWireReport(long Snapshots,long PayloadBytes,bool Failed);

// Owned canonical GEOMETRY METADATA bytes, not a live prerequisite capability.
// No source object, caller-supplied digest, or mutable byte array is exposed.
// The digest includes one LF; embedding writes only the JSON body. Transport
// framing, quotas and later independent comparisons remain the caller's duty.
internal sealed class MixedGeometryWireValue:IDisposable
{
 readonly object gate=new();byte[]? buffer;readonly int count;
 bool busy,failed,disposed;MixedProducerAdmission.Scope? writingScope;
 public long Bytes=>count;
 public string Sha256{get;}
 internal MixedGeometryWireValue(MixedGeometryWire owner)
 {var output=owner.ClaimOutput();buffer=output.Buffer;count=output.Count;Sha256=output.Sha256;}
 public void WriteTo(Utf8JsonWriter writer)
 {
  MixedProducerAdmission.Scope? scope=null;bool entered=false;
  try
  {
   lock(gate)
   {
    if(busy||failed||disposed){failed=true;writingScope?.Poison();throw new InvalidOperationException("A68 geometry wire value: failed reentrant or disposed");}
    busy=true;entered=true;
   }
   scope=MixedProducerAdmission.Current;byte[] owned;
   lock(gate)
   {
    writingScope=scope;
    if(failed||disposed||buffer is null)throw new InvalidOperationException("A68 geometry wire value: invalidated before embedding");
    owned=buffer;
   }
   ArgumentNullException.ThrowIfNull(writer);
   // Logical raw-byte input read plus output copy. Utf8JsonWriter's own
   // buffers/provider/runtime work are separate, not claimed as an RSS bound.
   scope.MatrixVisits(scope.CountProduct(2,count-1));scope.EnsureActive();
   lock(gate)if(failed||disposed)throw new InvalidOperationException("A68 geometry wire value: invalidated before raw write");
   writer.WriteRawValue(owned.AsSpan(0,count-1),skipInputValidation:true);
   scope.EnsureActive();lock(gate)if(failed||disposed)throw new InvalidOperationException("A68 geometry wire value: failed during embedding");
  }
  catch{scope?.Poison();lock(gate){failed=true;writingScope?.Poison();}throw;}
  finally{if(entered)lock(gate){busy=false;writingScope=null;}}
 }
 public void Dispose()
 {lock(gate){if(disposed)return;if(busy){failed=true;writingScope?.Poison();}disposed=true;buffer=null;}}
}

// Explicit primitive admission in BOTH ordinary and guarded builds. Fixed
// schemas contain129556/330260 rational coordinates including every zero.
// Flat snapshots avoid the legacy9965/25404-array jagged output trees. Limits
// and charges are cumulative logical work/storage, NOT allocator/native RSS,
// formatter/hash/encoder internals, static bootstrap, or later sink DTO costs.
internal sealed class MixedGeometryWire
{
 public const int BackgroundCoordinates=129556,GermCoordinates=330260;
 const long MaximumSafeInteger=9007199254740991;
 readonly MixedGeometryWireLimits limits;readonly Action<string,int> admitSnapshot;
 readonly object gate=new();long snapshots,payloadBytes;bool busy,failed;
 MixedProducerAdmission.Scope? activeScope;
 byte[]? pendingOutput;int pendingCount;string? pendingSha;
 static void Need(bool condition,string why){if(!condition)throw new InvalidOperationException("A68 geometry wire: "+why);}
 public MixedGeometryWire(MixedGeometryWireLimits limits,Action<string,int> admitSnapshot)
 {
  this.limits=limits??throw new ArgumentNullException(nameof(limits));
  this.admitSnapshot=admitSnapshot??throw new ArgumentNullException(nameof(admitSnapshot));
  Need(limits.RationalCharacters is >0 and <=16384&&limits.PayloadBytes>0&&
   limits.TotalPayloadBytes is >0 and <=MaximumSafeInteger&&limits.Snapshots is >0 and <=MaximumSafeInteger,"explicit positive bounded wire limits");
 }
 public MixedGeometryWireReport Snapshot(){lock(gate)return new(snapshots,payloadBytes,failed);}
 public MixedGeometryWireValue Background(MixedBackground actual)=>Build(actual,null,false);
 public MixedGeometryWireValue Germ(MixedMetricGerm actual)=>Build(null,actual,true);
 // A value can take bytes only during the producing writer's synchronous
 // mint step, after successful canonical construction. No raw-buffer/hash
 // constructor or reusable byte-export operation is available to callers.
 internal (byte[] Buffer,int Count,string Sha256) ClaimOutput()
 {
  lock(gate)try
  {
   Need(activeScope is not null,"active output mint");Healthy(activeScope!);
   Need(pendingOutput is not null&&pendingSha is not null&&pendingCount>1,"one ready canonical output");
   var result=(pendingOutput!,pendingCount,pendingSha!);pendingOutput=null;pendingCount=0;pendingSha=null;return result;
  }
  catch{failed=true;activeScope?.Poison();throw;}
 }
 void Healthy(MixedProducerAdmission.Scope scope)
 {scope.EnsureActive();lock(gate)Need(busy&&!failed&&ReferenceEquals(activeScope,scope),"healthy same-scope snapshot");}
 void ChargeBytes(MixedProducerAdmission.Scope scope,int count)
 {
  Healthy(scope);lock(gate)
  {Need(count>=0&&count<=limits.TotalPayloadBytes-payloadBytes,"cumulative payload bytes before write");payloadBytes+=count;}
 }
 static void CanonicalCulture()=>Need(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NegativeSign=="-","canonical negative sign required");
 readonly record struct Header(int Point,int MetricBasis,int JetIndex,int M0,int M1,int M2,int M3,int Order);
 static Header BackgroundHeader(MixedBackground? actual)
 {Need(actual is not null&&actual.Point is 0 or 1,"background point0/1");return new(actual!.Point,0,0,0,0,0,0,0);}
 static Header GermHeader(MixedMetricGerm? actual)
 {
  Need(actual is not null&&actual.MetricBasis is >=0 and <10&&actual.JetIndex is >=0 and <35,"fixed germ basis and jet domain");
  int[]? multi=actual!.Multiindex;Need(multi is {Length:4},"four-component multiindex");
  int x=multi![0],y=multi[1],z=multi[2],w=multi[3];
  Need(x is >=0 and <=3&&y is >=0 and <=3&&z is >=0 and <=3&&w is >=0 and <=3&&x+y+z+w<=3,"bounded divided-monomial multiindex");
  int at=0;bool matched=false;
  // The closed35-entry ordering, without constructing the source's list.
  for(int degree=0;degree<=3;degree++)for(int a=0;a<=degree;a++)for(int b=0;b<=degree-a;b++)for(int c=0;c<=degree-a-b;c++)
  {if(at==actual.JetIndex)matched=x==a&&y==b&&z==c&&w==degree-a-b-c;at++;}
  Need(matched&&at==35,"exact multiindex for jet index");
  return new(0,actual.MetricBasis,actual.JetIndex,x,y,z,w,x+y+z+w);
 }
 MixedGeometryWireValue Build(MixedBackground? background,MixedMetricGerm? germ,bool isGerm)
 {
  MixedProducerAdmission.Scope? scope=null;bool entered=false;
  try
  {
   lock(gate)
   {
    if(busy||failed){failed=true;activeScope?.Poison();throw new InvalidOperationException("A68 geometry wire: failed or reentrant snapshot");}
    busy=true;entered=true;
   }
   scope=MixedProducerAdmission.Current;lock(gate)activeScope=scope;
   Need(isGerm?germ is not null:background is not null,"actual geometry required");
   int coordinates=isGerm?GermCoordinates:BackgroundCoordinates;
   lock(gate){Need(snapshots<limits.Snapshots,"cumulative snapshot count before admission");snapshots++;}
   // Callback has no authority to replace these primitive charges. It runs
   // before full shape/coordinate scans and before any large allocation.
   Healthy(scope);admitSnapshot(isGerm?"germ":"background",coordinates);Healthy(scope);CanonicalCulture();
   // Known logical allocations: flat rational buffer, fixed byte buffer,
   // digest32, hash/stream/writer/value wrappers, value gate, and two64-char
   // hex strings. Wrapper count includes strings; array headers use RawArray.
   scope.RawArray(coordinates);scope.RawArray(limits.PayloadBytes);scope.RawArray(32);scope.MetadataObjects(7);scope.TextCharacters(128);
   // Coordinate loop bound: two complete scans, source/copy/output accesses,
   // scalar identity checks and all vector/grid/matrix structural accesses.
   // Each matrix has196 coordinates: fewer than20 logical array/data touches
   // per coordinate cover the scans, flat zero/store/read and shape loads.
   // Add1024 fixed identity/header visits, byte zeroing+copy2*capacity, and
   // digest/hex conversion slots. This is not a uniform VM instruction count.
   scope.MatrixVisits(scope.CountSum(scope.CountSum(scope.CountProduct(20,coordinates),scope.CountProduct(2,limits.PayloadBytes)),1152));
   Header header=isGerm?GermHeader(germ):BackgroundHeader(background);
   var preview=new Walker(scope,null);
   if(isGerm)WalkGerm(ref preview,germ!);else WalkBackground(ref preview,background!);
   Need(preview.Position==coordinates,"complete preview coordinate census");scope.RequireFormattingCharacters(preview.Characters);Healthy(scope);
   var values=new Rational[coordinates];var copy=new Walker(scope,values);
   if(isGerm)WalkGerm(ref copy,germ!);else WalkBackground(ref copy,background!);
   Header captured=isGerm?GermHeader(germ):BackgroundHeader(background);
   Need(captured==header&&copy.Position==coordinates,"stable complete copied geometry identity");
   // Recheck copied values: no formatting credit can be reused, and neither
   // a caller mutation nor a changed scope can bypass the aggregate preview.
   scope.RequireFormattingCharacters(copy.Characters);
   // The ASCII-domain scan below visits the final formatted characters. The
   // conservative formatting bound also bounds this additional logical pass.
   scope.MatrixVisits(copy.Characters);Healthy(scope);
   // Fixed destination capacity removes MemoryStream growth/copy ambiguity.
   // Utf8JsonWriter can buffer the ENTIRE body before disposal flushes to this
   // stream. This is NOT a strict streaming-memory bound: these logical
   // charges do NOT bound library buffers or their peak.
   var bytes=new byte[limits.PayloadBytes];
   using var stream=new PayloadStream(this,scope,bytes);
   using(var writer=new Utf8JsonWriter(stream,new(){Encoder=MixedTrace.JsonOptions.Encoder}))
   {
    var output=new Emitter(writer,scope,values,limits.RationalCharacters);
    if(isGerm)WriteGerm(ref output,header);else WriteBackground(ref output,header);
    Need(output.Position==coordinates,"complete output coordinate census");Healthy(scope);
   }
   stream.WriteByte(10);Healthy(scope);string sha=stream.Finish();Healthy(scope);
   lock(gate){pendingOutput=bytes;pendingCount=stream.Count;pendingSha=sha;}
   return new MixedGeometryWireValue(this);
  }
  catch{scope?.Poison();lock(gate){failed=true;activeScope?.Poison();}throw;}
  finally{if(entered)lock(gate){busy=false;activeScope=null;pendingOutput=null;pendingCount=0;pendingSha=null;}}
 }
 struct Walker(MixedProducerAdmission.Scope scope,Rational[]? destination)
 {
  public int Position{get;private set;}
  public long Characters{get;private set;}
  void Value(Rational value)
  {
   Need(value.Denominator.Sign>0,"positive rational denominator");
   Characters=scope.CountSum(Characters,scope.FormattingCharacters(value.Numerator,value.Denominator,value.Denominator.IsOne));
   if(destination is not null){Need(Position<destination.Length,"flat snapshot capacity");destination[Position]=value;}Position++;
  }
  public void Matrix(Matrix? matrix)
  {
   Need(matrix is not null&&matrix.N==14&&matrix.Data is not null,"complete fourteen-dimensional matrix");var data=matrix!.Data!;
   Need(data.GetLength(0)==14&&data.GetLength(1)==14&&data.GetLowerBound(0)==0&&data.GetLowerBound(1)==0,"actual matrix storage dimensions");
   // Direct data reads avoid conditional indexer accounting; every rational
   // is explicitly checked by this same primitive in BOTH build modes.
   for(int row=0;row<14;row++)for(int column=0;column<14;column++)Value(data[row,column]);
  }
  public void Vector(Matrix[]? vector)
  {Need(vector is {Length:14},"complete matrix vector");for(int i=0;i<14;i++)Matrix(vector![i]);}
  public void Grid(Matrix[,]? grid)
  {
   Need(grid is not null&&grid.GetLength(0)==14&&grid.GetLength(1)==14&&grid.GetLowerBound(0)==0&&grid.GetLowerBound(1)==0,"complete matrix grid");
   for(int i=0;i<14;i++)for(int j=0;j<14;j++)Matrix(grid![i,j]);
  }
  public void Metric(MetricData? metric)
  {Need(metric is not null,"metric container");Matrix(metric!.G);Vector(metric.D);Grid(metric.DD);}
  public void Connection(ConnectionData? connection)
  {
   Need(connection is not null,"connection container");Vector(connection!.Gamma);Grid(connection.DGamma);var r=connection.R;
   Need(r is not null,"curvature container");for(int axis=0;axis<4;axis++)Need(r!.GetLength(axis)==14&&r.GetLowerBound(axis)==0,"complete curvature axis");
   for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)Value(r![a,b,c,d]);
  }
 }
 static void WalkBackground(ref Walker walk,MixedBackground value)
 {
  walk.Matrix(value.Frame);walk.Matrix(value.InverseFrame);walk.Metric(value.Metric);walk.Connection(value.Connection);
  walk.Vector(value.FrameLift);walk.Vector(value.FramePartial);walk.Vector(value.Omega);
 }
 static void WalkGerm(ref Walker walk,MixedMetricGerm value)
 {
  walk.Metric(value.Shear);walk.Metric(value.DeltaMetric);walk.Metric(value.BlockMetric);walk.Connection(value.DeltaConnection);walk.Connection(value.Palatini);
  walk.Matrix(value.Motion);walk.Vector(value.MotionPartial);walk.Vector(value.MotionCovariant);walk.Vector(value.DeltaOmega);walk.Grid(value.DeltaOmegaPartial);walk.Matrix(value.DeltaFrame);
 }
 struct Emitter(Utf8JsonWriter writer,MixedProducerAdmission.Scope scope,Rational[] values,int rationalCharacters)
 {
  public int Position{get;private set;}
  public Utf8JsonWriter Writer=>writer;
  void Value()
  {
   scope.EnsureActive();CanonicalCulture();Need(Position<values.Length,"output flat snapshot capacity");var value=values[Position++];
#if !A68_GUARDED_EXACT_ARITHMETIC
   scope.Format(value.Numerator,value.Denominator,value.Denominator.IsOne);
#endif
   string text=value.ToString();Need(text.Length<=rationalCharacters,"exact rational character ceiling");
   // Rational constructors already supply reduced values. This output-domain
   // check rejects all non-ASCII signs/encodings without parsing or new GCDs.
   foreach(char c in text)Need(c is >= '0' and <= '9' or '-' or '/',"canonical ASCII rational characters");
   writer.WriteStringValue(text);
  }
  public void Matrix()
  {writer.WriteStartArray();for(int row=0;row<14;row++){writer.WriteStartArray();for(int column=0;column<14;column++)Value();writer.WriteEndArray();}writer.WriteEndArray();}
  public void Vector()
  {writer.WriteStartArray();for(int i=0;i<14;i++)Matrix();writer.WriteEndArray();}
  public void Grid()
  {writer.WriteStartArray();for(int i=0;i<14;i++){writer.WriteStartArray();for(int j=0;j<14;j++)Matrix();writer.WriteEndArray();}writer.WriteEndArray();}
  public void Metric()
  {writer.WriteStartObject();writer.WritePropertyName("g");Matrix();writer.WritePropertyName("d");Vector();writer.WritePropertyName("dd");Grid();writer.WriteEndObject();}
  public void Connection()
  {
   writer.WriteStartObject();writer.WritePropertyName("gamma");Vector();writer.WritePropertyName("dGamma");Grid();writer.WritePropertyName("curvature");
   writer.WriteStartArray();for(int a=0;a<14;a++){writer.WriteStartArray();for(int b=0;b<14;b++){writer.WriteStartArray();for(int c=0;c<14;c++){writer.WriteStartArray();for(int d=0;d<14;d++)Value();writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
   writer.WriteEndObject();
  }
 }
 static void WriteBackground(ref Emitter output,Header header)
 {
  var writer=output.Writer;writer.WriteStartObject();writer.WriteNumber("point",header.Point);
  writer.WritePropertyName("frame");output.Matrix();writer.WritePropertyName("inverseFrame");output.Matrix();
  writer.WritePropertyName("metric");output.Metric();writer.WritePropertyName("connection");output.Connection();
  writer.WritePropertyName("frameLift");output.Vector();writer.WritePropertyName("framePartial");output.Vector();writer.WritePropertyName("omega");output.Vector();writer.WriteEndObject();
 }
 static void WriteGerm(ref Emitter output,Header header)
 {
  var writer=output.Writer;writer.WriteStartObject();writer.WriteNumber("metricBasis",header.MetricBasis);writer.WriteNumber("jetIndex",header.JetIndex);
  writer.WritePropertyName("multiindex");writer.WriteStartArray();writer.WriteNumberValue(header.M0);writer.WriteNumberValue(header.M1);writer.WriteNumberValue(header.M2);writer.WriteNumberValue(header.M3);writer.WriteEndArray();writer.WriteNumber("order",header.Order);
  writer.WritePropertyName("shear");output.Metric();writer.WritePropertyName("deltaMetric");output.Metric();writer.WritePropertyName("blockMetric");output.Metric();
  writer.WritePropertyName("deltaConnection");output.Connection();writer.WritePropertyName("palatini");output.Connection();writer.WritePropertyName("motion");output.Matrix();
  writer.WritePropertyName("motionPartial");output.Vector();writer.WritePropertyName("motionCovariant");output.Vector();writer.WritePropertyName("deltaOmega");output.Vector();
  writer.WritePropertyName("deltaOmegaPartial");output.Grid();writer.WritePropertyName("deltaFrame");output.Matrix();writer.WriteEndObject();
 }
 sealed class PayloadStream(MixedGeometryWire owner,MixedProducerAdmission.Scope scope,byte[] buffer):Stream
 {
  readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  public int Count{get;private set;}
  public override void Write(ReadOnlySpan<byte> bytes)
  {
   owner.Healthy(scope);Need(bytes.Length<=buffer.Length-Count,"payload capacity before write");owner.ChargeBytes(scope,bytes.Length);
   bytes.CopyTo(buffer.AsSpan(Count));hash.AppendData(bytes);Count+=bytes.Length;owner.Healthy(scope);
  }
  public override void Write(byte[] bytes,int offset,int count)=>Write(bytes.AsSpan(offset,count));
  public override void WriteByte(byte value){Span<byte> one=stackalloc byte[1];one[0]=value;Write(one);}
  public string Finish(){owner.Healthy(scope);return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();}
  public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;
  public override long Length=>Count;public override long Position{get=>Count;set=>throw new NotSupportedException();}
  public override void Flush(){}public override int Read(byte[] bytes,int offset,int count)=>throw new NotSupportedException();
  public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();
  protected override void Dispose(bool disposing){if(disposing)hash.Dispose();base.Dispose(disposing);}
 }
}
