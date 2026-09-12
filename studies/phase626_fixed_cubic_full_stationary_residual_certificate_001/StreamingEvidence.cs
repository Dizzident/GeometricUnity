using System.Security.Cryptography;
using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// The plan is constructed exclusively from the frozen finite menu, before any
// scientific operator is evaluated. Empty chunks are mandatory, not elided.
internal sealed record ChunkPlan(string Path,string Collection,int Degree,int[] Forms,string[] TensorIds);
internal sealed record ChunkPin(string path,string sha256,long bytes,int records,string collection,int degree,int[] forms,string[] tensorIds);
internal sealed class StreamingEvidence
{
 public static readonly JsonSerializerOptions JsonOptions=new(){Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
 public const string DirectoryPath="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output/chunks";
 public const long MaximumFileBytes=64L*1024*1024;
 public const long MaximumAggregateBytes=2L*1024*1024*1024;
 public const int MaximumRecords=100000;
 public const int MaximumTensorRationalCharacters=256;
 public const int MaximumMetadataBytes=1024*1024;
 private readonly Dictionary<string,ChunkPlan> expected;
 private readonly List<ChunkPin> pins=[];
 public long TotalBytes{get;private set;}
 public long LargestFileBytes{get;private set;}
 public long RetainedRecords{get;private set;}
 public int LargestTensor{get;private set;}
 public int LargestRationalCharacters{get;private set;}
 public IReadOnlyList<ChunkPin> Pins=>pins;
 public StreamingEvidence(IEnumerable<ChunkPlan> plans)
 {
  expected=plans.ToDictionary(p=>p.Path,StringComparer.Ordinal);
  foreach(var p in expected.Values)
  {
   if(!p.Path.StartsWith(DirectoryPath+"/",StringComparison.Ordinal)||p.Path.Contains("..",StringComparison.Ordinal)||!p.Path.EndsWith(".json",StringComparison.Ordinal))throw new ArgumentException("literal chunk path");
   if(p.Degree<0||p.Degree>14||p.Forms.Length==0||!p.Forms.SequenceEqual(p.Forms.Distinct().Order())||p.Forms.Any(f=>f<0||f>=16384||Degree(f)!=p.Degree))throw new ArgumentException("fixed chunk forms");
   if(p.TensorIds.Length==0||p.TensorIds.Distinct(StringComparer.Ordinal).Count()!=p.TensorIds.Length)throw new ArgumentException("fixed tensor identities");
  }
 }
 public static IEnumerable<ChunkPlan> Split(string collection,int degree,int formsPerChunk,params string[] ids)
 {
  if(formsPerChunk<1)throw new ArgumentException("positive fixed group size");
  var masks=Enumerable.Range(0,16384).Where(f=>Degree(f)==degree).ToArray();
  for(int offset=0,index=0;offset<masks.Length;offset+=formsPerChunk,index++)yield return new(DirectoryPath+"/"+collection+"_g"+index.ToString("D3")+".json",collection,degree,masks.Skip(offset).Take(formsPerChunk).ToArray(),ids);
 }
 public void WriteCollection(string collection,IReadOnlyDictionary<string,FT> tensors,object metadata)
 {
  var plans=expected.Values.Where(p=>p.Collection==collection).OrderBy(p=>p.Path,StringComparer.Ordinal).ToArray();
  if(plans.Length==0||!plans[0].TensorIds.ToHashSet(StringComparer.Ordinal).SetEquals(tensors.Keys)||plans.Any(p=>!p.TensorIds.SequenceEqual(plans[0].TensorIds)))throw new ArgumentException("exact fixed collection identities");
  foreach(var pair in tensors)
  {
   var t=pair.Value;LargestTensor=Math.Max(LargestTensor,t.Count);
   if(t.Keys.Any(k=>k.Form<0||k.Form>=16384||k.Blade<0||k.Blade>=16384||Degree(k.Form)!=plans[0].Degree||k.K0!=0||k.K1!=0))throw new ArgumentException("complete tensor mask/degree/frequency");
   foreach(var z in t.Values)if(z.IsZero)throw new ArgumentException("explicit zero sparse coefficient");
  }
  var covered=plans.SelectMany(p=>p.Forms).ToArray();
  if(!covered.SequenceEqual(Enumerable.Range(0,16384).Where(f=>Degree(f)==plans[0].Degree)))throw new ArgumentException("complete fixed form partition");
  byte[] header=JsonSerializer.SerializeToUtf8Bytes(metadata,JsonOptions);
  if(header.Length>MaximumMetadataBytes)throw new ResourceLimitException("chunk metadata byte ceiling");
  foreach(var plan in plans)Write(plan,tensors,header);
 }
 private void Write(ChunkPlan plan,IReadOnlyDictionary<string,FT> tensors,byte[] metadata)
 {
  if(pins.Any(p=>p.path==plan.Path))throw new ArgumentException("duplicate emitted chunk");
  var forms=plan.Forms.ToHashSet();int records=0;
  using var stream=new MemoryStream();
  using(var writer=new Utf8JsonWriter(stream,new JsonWriterOptions{Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}))
  {
   writer.WriteStartObject();writer.WriteNumber("schemaVersion",1);writer.WriteNumber("phase",626);writer.WriteString("encoding","expanded-rational-tensor-chunks-v1");writer.WriteString("collection",plan.Collection);writer.WriteNumber("degree",plan.Degree);writer.WritePropertyName("forms");JsonSerializer.Serialize(writer,plan.Forms);writer.WritePropertyName("metadata");writer.WriteRawValue(metadata,true);writer.WriteStartObject("tensors");
   foreach(string id in plan.TensorIds)
   {
    writer.WriteStartArray(id);
    foreach(var q in tensors[id].Where(q=>forms.Contains(q.Key.Form)).OrderBy(q=>q.Key))
    {
     if(++records>MaximumRecords)throw new ResourceLimitException("chunk sparse-record ceiling");
     string re=q.Value.Real.ToString(),im=q.Value.Imaginary.ToString();LargestRationalCharacters=Math.Max(LargestRationalCharacters,Math.Max(re.Length,im.Length));
     if(re.Length>MaximumTensorRationalCharacters||im.Length>MaximumTensorRationalCharacters)throw new ResourceLimitException("tensor rational-character ceiling");
     writer.WriteStartObject();writer.WriteNumber("form",q.Key.Form);writer.WriteNumber("blade",q.Key.Blade);writer.WriteNumber("k0",q.Key.K0);writer.WriteNumber("k1",q.Key.K1);writer.WriteString("real",re);writer.WriteString("imaginary",im);writer.WriteEndObject();
    }
    writer.WriteEndArray();
   }
   writer.WriteEndObject();writer.WriteEndObject();writer.Flush();
  }
  stream.WriteByte(10);
  if(stream.Length>MaximumFileBytes)throw new ResourceLimitException("individual chunk byte ceiling");
  if(TotalBytes+stream.Length>MaximumAggregateBytes)throw new ResourceLimitException("aggregate chunk byte ceiling before next write");
  byte[] bytes=stream.ToArray();System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(plan.Path)!);File.WriteAllBytes(plan.Path,bytes);
  pins.Add(new(plan.Path,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),bytes.LongLength,records,plan.Collection,plan.Degree,plan.Forms,plan.TensorIds));
  TotalBytes+=bytes.LongLength;LargestFileBytes=Math.Max(LargestFileBytes,bytes.LongLength);RetainedRecords+=records;
 }
 public bool Complete()
 {
  var required=expected.Keys.Order(StringComparer.Ordinal).ToArray();
  var written=pins.Select(p=>p.path).Order(StringComparer.Ordinal).ToArray();
  var actual=System.IO.Directory.Exists(DirectoryPath)?System.IO.Directory.EnumerateFiles(DirectoryPath,"*",SearchOption.AllDirectories).Select(p=>p.Replace('\\','/')).Order(StringComparer.Ordinal).ToArray():[];
  return required.SequenceEqual(written)&&required.SequenceEqual(actual)&&pins.All(p=>new FileInfo(p.path).Length==p.bytes&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p.path))).ToLowerInvariant()==p.sha256);
 }
}

internal static class CertificateLayout
{
 public static readonly string[] Fields=["S1","S2","S3","S4","S5","X","PHGamma","PTGamma","PtrGamma","J","B","C","W5","central"];
 public static readonly int[] ActiveDirections=[0,7,8,9];
 public static readonly int[] StageDegrees=[2,12,13,14,0,1,13,1];
 private static readonly int[] GeneralGroups=[8,8,7,1,1,7,7,7];
 private static readonly int[] KineticGroups=[91,91,14,1,1,7,7,7];
 public static string Prefix(int point)=>"p"+point;
 public static IEnumerable<ChunkPlan> Stages(string name,bool kinetic)
 {for(int s=0;s<8;s++)foreach(var p in StreamingEvidence.Split(name+"_stage"+s,StageDegrees[s],(kinetic?KineticGroups:GeneralGroups)[s],"actual","oracle"))yield return p;}
 public static IEnumerable<ChunkPlan> Kinetic(string name)
 {
  string[] inputIds=["input",..Enumerable.Range(0,14).Select(a=>"derivative"+a),..Enumerable.Range(0,14).Select(a=>"oracle"+a)];
  foreach(var p in StreamingEvidence.Split(name+"_inputs",1,14,inputIds))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_exterior",2,91,"actual","oracle"))yield return p;
  foreach(var p in Stages(name,true))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_adjointLegs",2,91,"first","second"))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_adjoint",2,16,"actual","simplified"))yield return p;
  foreach(int a in ActiveDirections)foreach(var p in StreamingEvidence.Split(name+"_reverse"+a,2,10,"actual","parallel","simplified"))yield return p;
  string[] zeroIds=Enumerable.Range(0,14).Except(ActiveDirections).SelectMany(a=>new[]{"actual"+a,"parallel"+a,"simplified"+a}).ToArray();
  foreach(var p in StreamingEvidence.Split(name+"_reverseZero",2,91,zeroIds))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_result",1,4,"reverse","reverseOracle","full"))yield return p;
 }
 public static IEnumerable<ChunkPlan> Feedback(string name,bool residual)
 {
  foreach(var p in Stages(name,false))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_transpose",1,7,"actual","word"))yield return p;
  foreach(var p in StreamingEvidence.Split(name+"_result",1,residual?4:7,residual?["nonlinear","residual","scaledDefect"]:["nonlinear","oracle"]))yield return p;
 }
 public static ChunkPlan[] Plan()
 {
  var plan=new List<ChunkPlan>();
  for(int point=0;point<2;point++)
  {
   string prefix=Prefix(point);
   foreach(string id in Fields)plan.AddRange(Kinetic(prefix+"_kinetic_"+id));
   for(int n=2;n<=10;n++)plan.AddRange(Feedback(prefix+"_polynomial"+n,true));
   plan.AddRange(Feedback(prefix+"_direct",true));
   foreach(string id in Fields.Skip(6)){plan.AddRange(Feedback(prefix+"_self_"+id,false));plan.AddRange(Feedback(prefix+"_cross_"+id,false));}
   plan.AddRange(Feedback(prefix+"_offRoot",false));
   plan.AddRange(StreamingEvidence.Split(prefix+"_lowResidual",1,14,"G0","G1","defect0","defect1","defect2"));
  }
  return plan.ToArray();
 }
 public static void WriteStages(StreamingEvidence store,string name,FT[] actual,FT[] oracle,object metadata)
 {for(int s=0;s<8;s++)store.WriteCollection(name+"_stage"+s,new Dictionary<string,FT>{{"actual",actual[s]},{"oracle",oracle[s]}},metadata);}
 public static void WriteKinetic(StreamingEvidence store,string name,CertifiedKinetic result,object metadata)
 {
  var inputs=new Dictionary<string,FT>{{"input",result.Input}};for(int a=0;a<14;a++){inputs.Add("derivative"+a,result.Derivatives[a]);inputs.Add("oracle"+a,result.DerivativeOracles[a]);}
  store.WriteCollection(name+"_inputs",inputs,metadata);
  store.WriteCollection(name+"_exterior",new Dictionary<string,FT>{{"actual",result.Exterior},{"oracle",result.ExteriorOracle}},metadata);
  WriteStages(store,name,result.Stages,result.NaiveStages,metadata);
  store.WriteCollection(name+"_adjointLegs",new Dictionary<string,FT>{{"first",result.AdjointFirst},{"second",result.AdjointSecond}},metadata);
  store.WriteCollection(name+"_adjoint",new Dictionary<string,FT>{{"actual",result.Adjoint},{"simplified",result.SimplifiedAdjoint}},metadata);
  foreach(int a in ActiveDirections)store.WriteCollection(name+"_reverse"+a,new Dictionary<string,FT>{{"actual",result.ReverseDerivatives[a]},{"parallel",result.ParallelReverse[a]},{"simplified",result.ParallelSimplified[a]}},metadata);
  var zeros=new Dictionary<string,FT>();foreach(int a in Enumerable.Range(0,14).Except(ActiveDirections)){zeros.Add("actual"+a,result.ReverseDerivatives[a]);zeros.Add("parallel"+a,result.ParallelReverse[a]);zeros.Add("simplified"+a,result.ParallelSimplified[a]);}store.WriteCollection(name+"_reverseZero",zeros,metadata);
  store.WriteCollection(name+"_result",new Dictionary<string,FT>{{"reverse",result.Reverse},{"reverseOracle",result.ReverseOracle},{"full",result.Full}},metadata);
 }
}
