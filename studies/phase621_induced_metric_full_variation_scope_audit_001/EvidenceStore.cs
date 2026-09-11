using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Lossless computation-graph evidence. The replay interpreter uses the separate
// WordSign/NaiveProduct kernel, not the forward audit's grouped product kernel.
internal static class EvidenceStore
{
 public const string ShardDirectory="studies/phase621_induced_metric_full_variation_scope_audit_001/output/shards";
 public const long AggregateByteCeiling=2147483648;
 public static long WrittenBytes{get;private set;}
 public static int MaximumShardBytes{get;private set;}
 public sealed class ResourceFailure(string message):Exception(message);
 public const string GraphJson="""
 {"schema":"caa-jet-dag-v1","inputs":{"f":2,"p1":1,"p2":2},"nodes":[
 {"id":"sf","op":"star","a":"f","degree":12},
 {"id":"one","op":"product","a":"p1","b":"sf","kind":"C","degree":13},
 {"id":"inner","op":"product","a":"p2","b":"sf","kind":"A","degree":14},
 {"id":"zero","op":"star","a":"inner","degree":0},
 {"id":"outer","op":"product","a":"p1","b":"zero","kind":"A","degree":1},
 {"id":"outer-star","op":"star","a":"outer","degree":13},
 {"id":"scaled","op":"scale","a":"outer-star","scalar":"-1/2","degree":13},
 {"id":"upper","op":"add","a":"one","b":"scaled","degree":13},
 {"id":"lower","op":"star","a":"upper","degree":1}],
 "stages":["f","sf","one","inner","zero","outer","upper","lower"]}
 """;
 public static readonly JsonSerializerOptions Compact=new(){WriteIndented=false};
 public static string Digest(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
 public static string TensorHash(FT tensor)
 {foreach(var z in tensor.Values)if(z.Real.ToString().Length>128||z.Imaginary.ToString().Length>128)throw new ResourceFailure("prospective tensor rational text ceiling");return Digest(JsonSerializer.SerializeToUtf8Bytes(Terms(tensor),Compact));}
 public static object[] Fingerprints(Jet[] stages)=>stages.Select(s=>(object)new{value=TensorHash(s.Value),delta=TensorHash(s.Delta)}).ToArray();
 public static object[] Fingerprints(FT[] values,FT[] deltas)=>values.Select((v,i)=>(object)new{value=TensorHash(v),delta=TensorHash(deltas[i])}).ToArray();
 public static string ShardPath(string root,int point,int metric,int jet)=>$"{root}/output/shards/jet_p{point}_m{metric:00}_j{jet:00}.json";
 public static string[] ExpectedPaths(string root)=>(from p in Enumerable.Range(0,2) from m in Enumerable.Range(0,10) from j in Enumerable.Range(0,35) select ShardPath(root,p,m,j)).Order(StringComparer.Ordinal).ToArray();
 public static bool ExactPathSet(string root)=>Directory.Exists(ShardDirectory)&&ExpectedPaths(root).SequenceEqual(Directory.EnumerateFiles(ShardDirectory,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal));
 public static object WriteShard(string root,int point,int metric,int jet,object evidence)
 {
  var document=new{schemaVersion=1,phase=621,evidenceKind="full-induced-metric-jet-v1",point,metricBasis=metric,jetIndex=jet,evidence};var bytes=JsonSerializer.SerializeToUtf8Bytes(document,Compact).Concat(new byte[]{10}).ToArray();
  if(bytes.Length>67108864)throw new ResourceFailure("prospective 64MiB per-shard byte ceiling");if(WrittenBytes+bytes.Length>AggregateByteCeiling)throw new ResourceFailure("prospective 2GiB aggregate shard byte ceiling; existing evidence preserved");using var parsed=JsonDocument.Parse(bytes);CheckRationalText(parsed.RootElement);
  var path=ShardPath(root,point,metric,jet);Directory.CreateDirectory(ShardDirectory);File.WriteAllBytes(path,bytes);WrittenBytes+=bytes.Length;MaximumShardBytes=Math.Max(MaximumShardBytes,bytes.Length);return new{point,metricBasis=metric,jetIndex=jet,path,sha256=Digest(bytes),byteCount=bytes.Length};
 }
 private static void CheckRationalText(JsonElement value)
 {
  if(value.ValueKind==JsonValueKind.Object){foreach(var p in value.EnumerateObject())CheckRationalText(p.Value);return;}
  if(value.ValueKind==JsonValueKind.Array){foreach(var q in value.EnumerateArray())CheckRationalText(q);return;}
  if(value.ValueKind!=JsonValueKind.String)return;string s=value.GetString()!;if(s.Length>128&&s.All(c=>char.IsAsciiDigit(c)||c is '-' or '/'))throw new ResourceFailure("prospective rational text ceiling");
 }
 private static FT ReadTensor(JsonElement element)
 {
  var t=new FT();(int,int,int,int)? previous=null;foreach(var row in element.EnumerateArray())
  {var key=(row.GetProperty("form").GetInt32(),row.GetProperty("blade").GetInt32(),row.GetProperty("k0").GetInt32(),row.GetProperty("k1").GetInt32());string real=row.GetProperty("real").GetString()!,imaginary=row.GetProperty("imaginary").GetString()!;var z=new Scalar(SpinGeometry.Parse(real),SpinGeometry.Parse(imaginary));if(key.Item1<0||key.Item1>Full||key.Item2<0||key.Item2>Full||key.Item3!=0||key.Item4!=0||z.IsZero||t.ContainsKey(key)||real!=z.Real.ToString()||imaginary!=z.Imaginary.ToString()||(previous.HasValue&&Comparer<(int,int,int,int)>.Default.Compare(previous.Value,key)>=0))throw new InvalidOperationException("primitive tensor record");t.Add(key,z);previous=key;}return t;
 }
 private static FT Motion(Matrix a,FT t)
 {
  // Independent covector derivation: wedge the replacements in their original
  // ordered positions, rather than deleting/reinserting by a closed sign law.
  var result=new FT();foreach(var term in t)
  {
   var axes=Enumerable.Range(0,14).Where(i=>(term.Key.Form&(1<<i))!=0).ToArray();for(int slot=0;slot<axes.Length;slot++)for(int replacement=0;replacement<14;replacement++)if(a[axes[slot],replacement]!=0)
   {int mask=0,sign=1;for(int k=0;k<axes.Length;k++){int axis=k==slot?replacement:axes[k];if((mask&(1<<axis))!=0){sign=0;break;}sign*=Shuffle(mask,1<<axis);mask|=1<<axis;}if(sign!=0)Put(result,(mask,term.Key.Blade,0,0),term.Value*new Scalar(a[axes[slot],replacement]*sign,0));}
  }return result;
 }
 private static Jet Prod(Jet a,Jet b,char kind='W')=>new(NaiveProduct(a.Value,b.Value,kind),Add(NaiveProduct(a.Delta,b.Value,kind),NaiveProduct(a.Value,b.Delta,kind)));
 private static Jet StarJ(Jet input,Matrix motion,bool moving)
 {var delta=Star(input.Delta);if(moving)delta=Add(delta,Add(Star(Motion(motion,input.Value)),Scale(Motion(motion,Star(input.Value)),-1)));return new(Star(input.Value),delta);}
 private static FT Cov(FT b,FT t)=>Add(NaiveProduct(b,t,'W'),NaiveProduct(t,b,'W'));
 private static Jet[] Interpret(Jet f,Matrix motion,bool moving)
 {
  var p1=Caa.Gamma1;var p2=Caa.Gamma2;var nodes=new Dictionary<string,Jet>{["f"]=f,["p1"]=new(p1,moving?Scale(Motion(motion,p1),-1):new()),["p2"]=new(p2,moving?Scale(Motion(motion,p2),-1):new())};using var spec=JsonDocument.Parse(GraphJson);
  foreach(var n in spec.RootElement.GetProperty("nodes").EnumerateArray())
  {
   string op=n.GetProperty("op").GetString()!;var a=nodes[n.GetProperty("a").GetString()!];Jet result=op switch
   {"star"=>StarJ(a,motion,moving),"product"=>Prod(a,nodes[n.GetProperty("b").GetString()!],n.GetProperty("kind").GetString()![0]),"scale"=>Jet.Scale(a,new Scalar(SpinGeometry.Parse(n.GetProperty("scalar").GetString()!),0)),"add"=>Jet.Add(a,nodes[n.GetProperty("b").GetString()!]),_=>throw new InvalidOperationException("DAG operation")};int degree=n.GetProperty("degree").GetInt32();if(!Typed(result.Value,degree)||!Typed(result.Delta,degree)||!nodes.TryAdd(n.GetProperty("id").GetString()!,result))throw new InvalidOperationException("DAG topology/type");
  }return spec.RootElement.GetProperty("stages").EnumerateArray().Select(q=>nodes[q.GetString()!]).ToArray();
 }
 private static void VerifyFingerprints(Jet[] stages,JsonElement fingerprints)
 {if(fingerprints.GetArrayLength()!=8)throw new InvalidOperationException("DAG stage count");for(int k=0;k<8;k++)if(TensorHash(stages[k].Value)!=fingerprints[k].GetProperty("value").GetString()||TensorHash(stages[k].Delta)!=fingerprints[k].GetProperty("delta").GetString())throw new InvalidOperationException("expanded DAG fingerprint");}
 private static void SparseShape(JsonElement array,params string[] names)
 {int previous=-1;foreach(var q in array.EnumerateArray()){int index=0;foreach(string name in names){int axis=q.GetProperty(name).GetInt32();if(axis<0||axis>=14)throw new InvalidOperationException("sparse geometry index");index=index*14+axis;}string printed=q.GetProperty("value").GetString()!;var value=SpinGeometry.Parse(printed);if(index<=previous||value==0||printed!=value.ToString())throw new InvalidOperationException("sparse geometry canonicality");previous=index;}}
 private static void DocumentCanonical(byte[] bytes,JsonElement root)
 {if(bytes.Length>67108864||bytes.Length<2||bytes[^1]!=10||bytes[^2]==10||!bytes.SequenceEqual(JsonSerializer.SerializeToUtf8Bytes(root,Compact).Concat(new byte[]{10})))throw new InvalidOperationException("canonical document/size");CheckRationalText(root);}
 public static (int Shards,int Fields,int StagePairs,int ScalarEntries) Verify(string root,string expectedContractHash,JsonElement fixtures)
 {
  string full=root+"/output/induced_metric_full_variation_scope_audit.json",summary=root+"/output/induced_metric_full_variation_scope_audit_summary.json";
  if(!File.ReadAllBytes(full).SequenceEqual(File.ReadAllBytes(summary)))throw new InvalidOperationException("full/summary byte parity");byte[] summaryBytes=File.ReadAllBytes(summary);using var doc=JsonDocument.Parse(summaryBytes);var result=doc.RootElement;DocumentCanonical(summaryBytes,result);if(!result.GetProperty("auditPassed").GetBoolean()||result.GetProperty("contractSha256").GetString()!=expectedContractHash)throw new InvalidOperationException("summary/contract");var evidence=result.GetProperty("evidence");var bases=evidence.GetProperty("pointRows");var manifests=evidence.GetProperty("rows");
  foreach(string key in new[]{"evidenceStorage","evidenceDag","dagSemantics"})if(!JsonNode.DeepEquals(JsonNode.Parse(evidence.GetProperty(key).GetRawText()),JsonNode.Parse(fixtures.GetProperty(key).GetRawText())))throw new InvalidOperationException("retained graph/semantics provenance");
  if(!JsonNode.DeepEquals(JsonNode.Parse(GraphJson),JsonNode.Parse(fixtures.GetProperty("evidenceDag").GetRawText())))throw new InvalidOperationException("replay graph/contract parity");
  var expectedPaths=ExpectedPaths(root);var actual=Directory.EnumerateFiles(ShardDirectory,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
  if(manifests.GetArrayLength()!=700||!expectedPaths.SequenceEqual(actual)||!expectedPaths.SequenceEqual(manifests.EnumerateArray().Select(q=>q.GetProperty("path").GetString()!).Order(StringComparer.Ordinal)))throw new InvalidOperationException("complete shard pathset");
  int shards=0,fields=0,stagePairs=0,scalarEntries=0,maximumBytes=0;long aggregateBytes=0;
  foreach(var manifest in manifests.EnumerateArray())
  {
   string path=manifest.GetProperty("path").GetString()!;byte[] bytes=File.ReadAllBytes(path);if(bytes.Length!=manifest.GetProperty("byteCount").GetInt32()||Digest(bytes)!=manifest.GetProperty("sha256").GetString())throw new InvalidOperationException("shard hash/bytes");aggregateBytes+=bytes.Length;maximumBytes=Math.Max(maximumBytes,bytes.Length);if(aggregateBytes>AggregateByteCeiling)throw new InvalidOperationException("aggregate shard resource ceiling");using var sd=JsonDocument.Parse(bytes);var s=sd.RootElement;DocumentCanonical(bytes,s);int point=s.GetProperty("point").GetInt32(),metric=s.GetProperty("metricBasis").GetInt32(),jet=s.GetProperty("jetIndex").GetInt32();
   if(s.GetProperty("schemaVersion").GetInt32()!=1||s.GetProperty("phase").GetInt32()!=621||s.GetProperty("evidenceKind").GetString()!="full-induced-metric-jet-v1"||path!=ShardPath(root,point,metric,jet)||point!=manifest.GetProperty("point").GetInt32()||metric!=manifest.GetProperty("metricBasis").GetInt32()||jet!=manifest.GetProperty("jetIndex").GetInt32())throw new InvalidOperationException("shard identity");
   var e=s.GetProperty("evidence");if(!e.GetProperty("multiindex").EnumerateArray().Select(q=>q.GetInt32()).SequenceEqual(MetricVariation.Multiindices()[jet])||e.GetProperty("baselinePointId").GetInt32()!=point)throw new InvalidOperationException("jet/baseline identity");var pointData=bases[point];if(pointData.GetProperty("point").GetInt32()!=point)throw new InvalidOperationException("baseline order");
   var eta4=Matrix.Diagonal(-1,1,1,1).Text();var y4=(point==0?Matrix.Diagonal(-1,1,1,1):Matrix.Diagonal(-1,4,9,16)).Text();if(!JsonNode.DeepEquals(JsonNode.Parse(pointData.GetProperty("h0").GetRawText()),JsonSerializer.SerializeToNode(eta4))||!JsonNode.DeepEquals(JsonNode.Parse(pointData.GetProperty("fibreMetricY").GetRawText()),JsonSerializer.SerializeToNode(y4)))throw new InvalidOperationException("fixed-h versus fibre point");
   foreach(string key in new[]{"shearVariation","metricVariation","inverseVariation","frameMotion"})_ =SpinGeometry.ReadMatrix(e.GetProperty(key));SparseShape(e.GetProperty("metricFirstJets"),"z","i","j");SparseShape(e.GetProperty("metricSecondJets"),"z","w","i","j");SparseShape(e.GetProperty("connectionVariation"),"z","i","j");SparseShape(e.GetProperty("connectionDerivative"),"z","w","i","j");SparseShape(e.GetProperty("curvatureVariation"),"a","b","c","d");
   var b=ReadTensor(pointData.GetProperty("spinReference"));var fb=ReadTensor(pointData.GetProperty("spinCurvature"));var db=ReadTensor(e.GetProperty("spinConnectionVariation"));var df=ReadTensor(e.GetProperty("spinCurvatureVariation"));var motion=SpinGeometry.ReadMatrix(e.GetProperty("frameMotion"));var fieldRows=e.GetProperty("fieldRows");if(fieldRows.GetArrayLength()!=5)throw new InvalidOperationException("field menu");
   for(int field=0;field<5;field++)
   {
    var row=fieldRows[field];if(row.GetProperty("field").GetInt32()!=field||row.GetProperty("baselinePointId").GetInt32()!=point||row.GetProperty("referenceInputVariationId").GetString()!="spinCurvatureVariation")throw new InvalidOperationException("field identity");var t=ReadTensor(pointData.GetProperty("fields")[field]);var dt=Cov(b,t);var q=NaiveProduct(t,t,'W');var tdot=Motion(motion,t);var bdot=Add(db,Motion(motion,b));var ddt=Add(Cov(bdot,t),Cov(b,tdot));var dq=Add(NaiveProduct(tdot,t,'W'),NaiveProduct(t,tdot,'W'));
    if(!Equal(ddt,ReadTensor(row.GetProperty("kineticInputVariation")))||!Equal(dq,ReadTensor(row.GetProperty("cubicInputVariation"))))throw new InvalidOperationException("primitive derived input");
    Jet[] fixedInputs=[new(fb,Add(df,Scale(Motion(motion,fb),-1))),new(dt,Cov(db,t)),Jet.Fixed(q)];Jet[] adaptedInputs=[new(fb,df),new(dt,ddt),new(q,dq)];var fixedStages=new Jet[3][];var adaptedStages=new Jet[3][];
    for(int piece=0;piece<3;piece++)
    {fixedStages[piece]=Interpret(fixedInputs[piece],motion,true);adaptedStages[piece]=Interpret(adaptedInputs[piece],motion,false);VerifyFingerprints(fixedStages[piece],row.GetProperty("fixedStageFingerprints")[piece]);VerifyFingerprints(adaptedStages[piece],row.GetProperty("adaptedStageFingerprints")[piece]);for(int stage=0;stage<8;stage++){if(!Equal(fixedStages[piece][stage].Value,adaptedStages[piece][stage].Value)||!Equal(Add(fixedStages[piece][stage].Delta,Motion(motion,fixedStages[piece][stage].Value)),adaptedStages[piece][stage].Delta))throw new InvalidOperationException("replayed frame-stage equality");stagePairs++;}}
    Rational[] weights=[1,new(1,2),new(1,3),new(1,2)];
    for(int piece=0;piece<4;piece++)
    {
     var original=Prod(Jet.Fixed(t),StarJ(piece<3?fixedStages[piece][7]:Jet.Fixed(t),motion,true));var value=Top(original.Value)*weights[piece];var delta=Top(original.Delta)*weights[piece];Rational expanded=piece<3?weights[piece]*(Pair(tdot,adaptedStages[piece][7].Value)+Pair(t,adaptedStages[piece][7].Delta)):Pair(t,tdot);
     if(value!=ActionVariation.Forecast(point,field)[piece]||delta!=expanded||delta.ToString()!=row.GetProperty("originalVariation")[piece].GetString()||expanded.ToString()!=row.GetProperty("independentExpansion")[piece].GetString())throw new InvalidOperationException("replayed action scalar");scalarEntries++;
    }fields++;
   }shards++;
  }
  if(shards!=700||fields!=3500||stagePairs!=84000||scalarEntries!=14000||aggregateBytes!=evidence.GetProperty("totalShardBytes").GetInt64()||maximumBytes!=evidence.GetProperty("maximumShardBytes").GetInt32())throw new InvalidOperationException("replay census/byte totals");return(shards,fields,stagePairs,scalarEntries);
 }
}
