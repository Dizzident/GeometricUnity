using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Full-shape MANUFACTURED metadata only. Every background, germ, metric,
// connection and matrix is allocated WITHOUT running its constructor. These
// zero/sentinel containers cannot stand in for source geometry or a GU run.
static partial class Program
{
 sealed class GeometryWireFixture(Assembly assembly)
 {
  public readonly Assembly Assembly=assembly;
  public Type Type(string name)=>Assembly.GetType(name,true)!;
  public object Bare(string name)=>RuntimeHelpers.GetUninitializedObject(Type(name));
  public object New(string name,params object?[] args)
  {try{return Activator.CreateInstance(Type(name),args)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
  public object Static(string name,string method,params object?[] args)
  {try{return Type(name).GetMethod(method)!.Invoke(null,args)!;}catch(TargetInvocationException e)when(e.InnerException is not null){ExceptionDispatchInfo.Capture(e.InnerException).Throw();throw;}}
  public static void Set(object target,string property,object? value)=>target.GetType().GetField("<"+property+">k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(target,value);
  public object Rational(BigInteger numerator,BigInteger denominator)
  {
   object value=Bare("Rational");Type("Rational").GetField("n",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(value,numerator);
   Type("Rational").GetField("d",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(value,denominator);return value;
  }
  public object Matrix(int n=14)
  {var result=Bare("Matrix");Set(result,"N",n);Set(result,"Data",Array.CreateInstance(Type("Rational"),n,n));return result;}
  public Array Vector(object matrix)
  {var result=Array.CreateInstance(Type("Matrix"),14);for(int i=0;i<14;i++)result.SetValue(matrix,i);return result;}
  public Array Grid(object matrix)
  {var result=Array.CreateInstance(Type("Matrix"),14,14);for(int i=0;i<14;i++)for(int j=0;j<14;j++)result.SetValue(matrix,i,j);return result;}
  public object Metric(object matrix)
  {var result=Bare("MetricData");Set(result,"G",matrix);Set(result,"D",Vector(matrix));Set(result,"DD",Grid(matrix));return result;}
  public object Connection(object matrix)
  {var result=Bare("ConnectionData");Set(result,"Gamma",Vector(matrix));Set(result,"DGamma",Grid(matrix));Set(result,"R",Array.CreateInstance(Type("Rational"),14,14,14,14));return result;}
  public object Background()
  {
   object result=Bare("MixedBackground"),zero=Matrix();Set(result,"Point",0);Set(result,"Frame",zero);Set(result,"InverseFrame",zero);
   Set(result,"Metric",Metric(zero));Set(result,"Connection",Connection(zero));
   foreach(string field in new[]{"FrameLift","FramePartial","Omega"})Set(result,field,Vector(zero));return result;
  }
  public object Germ()
  {
   object result=Bare("MixedMetricGerm"),zero=Matrix();Set(result,"MetricBasis",0);Set(result,"JetIndex",0);Set(result,"Multiindex",new int[4]);
   foreach(string field in new[]{"Shear","DeltaMetric","BlockMetric"})Set(result,field,Metric(zero));
   foreach(string field in new[]{"DeltaConnection","Palatini"})Set(result,field,Connection(zero));
   Set(result,"Motion",zero);Set(result,"DeltaFrame",zero);
   foreach(string field in new[]{"MotionPartial","MotionCovariant","DeltaOmega"})Set(result,field,Vector(zero));
   Set(result,"DeltaOmegaPartial",Grid(zero));return result;
  }
  object Record(string name,IReadOnlyDictionary<string,long>? overrides=null)
  {
   var constructor=Type(name).GetConstructors().Single();return constructor.Invoke(constructor.GetParameters().Select(p=>
    p.ParameterType==typeof(long)?(object)(overrides?.GetValueOrDefault(p.Name!,1000000000L)??1000000000L):
    Record(p.ParameterType.FullName!,overrides)).ToArray());
  }
  public IDisposable Scope(IReadOnlyDictionary<string,long>? overrides=null)=>(IDisposable)Static("MixedProducerAdmission","Enter",Record("MixedProducerAdmission+Limits",overrides));
  public object Writer(int rational=128,int payload=2000000,long total=10000000,long snapshots=10,Action<string,int>? admission=null)
   =>New("MixedGeometryWire",New("MixedGeometryWireLimits",rational,payload,total,snapshots),admission??((_,_)=>{}));
 }
 static void GeometryRefused(Action action)
 {
  try{action();}catch(Exception e)when(e is InvalidOperationException or ArgumentException||e.GetType().FullName=="MixedProducerAdmission+Refused"){return;}
  throw new Exception("expected geometry wire refusal");
 }
 static byte[] GeometryBody(object value)
 {
  using var output=new MemoryStream();using(var writer=new Utf8JsonWriter(output,new(){Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}))
  {Call(value,"WriteTo",writer);writer.Flush();}return output.ToArray();
 }
 static string GeometryRat(object value)
 {
  BigInteger numerator=(BigInteger)Get(value,"Numerator"),denominator=(BigInteger)Get(value,"Denominator");
  return numerator.ToString(CultureInfo.InvariantCulture)+(denominator.IsOne?"":"/"+denominator.ToString(CultureInfo.InvariantCulture));
 }
 // Independent schema projection: reflection reads the raw manufactured Data
 // arrays, never Matrix.Text, the production writer, or its capture helpers.
 static object GeometryExpected(object input,bool germ)
 {
  string[][] MatrixText(object matrix)
  {var data=(Array)Get(matrix,"Data");return Enumerable.Range(0,14).Select(i=>Enumerable.Range(0,14).Select(j=>GeometryRat(data.GetValue(i,j)!)).ToArray()).ToArray();}
  object Vector(object value)=>((Array)value).Cast<object>().Select(MatrixText).ToArray();
  object Grid(object value)
  {var a=(Array)value;return Enumerable.Range(0,14).Select(i=>Enumerable.Range(0,14).Select(j=>MatrixText(a.GetValue(i,j)!)).ToArray()).ToArray();}
  object Metric(object value)=>new{g=MatrixText(Get(value,"G")),d=Vector(Get(value,"D")),dd=Grid(Get(value,"DD"))};
  object Connection(object value)
  {var r=(Array)Get(value,"R");return new{gamma=Vector(Get(value,"Gamma")),dGamma=Grid(Get(value,"DGamma")),
   curvature=Enumerable.Range(0,14).Select(a=>Enumerable.Range(0,14).Select(b=>Enumerable.Range(0,14).Select(c=>Enumerable.Range(0,14).Select(d=>GeometryRat(r.GetValue(a,b,c,d)!)).ToArray()).ToArray()).ToArray()).ToArray()};}
  if(!germ)return new{point=(int)Get(input,"Point"),frame=MatrixText(Get(input,"Frame")),inverseFrame=MatrixText(Get(input,"InverseFrame")),
   metric=Metric(Get(input,"Metric")),connection=Connection(Get(input,"Connection")),frameLift=Vector(Get(input,"FrameLift")),framePartial=Vector(Get(input,"FramePartial")),omega=Vector(Get(input,"Omega"))};
  return new{metricBasis=(int)Get(input,"MetricBasis"),jetIndex=(int)Get(input,"JetIndex"),multiindex=(int[])Get(input,"Multiindex"),order=((int[])Get(input,"Multiindex")).Sum(),
   shear=Metric(Get(input,"Shear")),deltaMetric=Metric(Get(input,"DeltaMetric")),blockMetric=Metric(Get(input,"BlockMetric")),
   deltaConnection=Connection(Get(input,"DeltaConnection")),palatini=Connection(Get(input,"Palatini")),motion=MatrixText(Get(input,"Motion")),
   motionPartial=Vector(Get(input,"MotionPartial")),motionCovariant=Vector(Get(input,"MotionCovariant")),deltaOmega=Vector(Get(input,"DeltaOmega")),
   deltaOmegaPartial=Grid(Get(input,"DeltaOmegaPartial")),deltaFrame=MatrixText(Get(input,"DeltaFrame"))};
 }
 static void GeometryCheckBody(object value,object input,bool germ)
 {
  var actual=GeometryBody(value);var expected=JsonSerializer.SerializeToUtf8Bytes(GeometryExpected(input,germ),Json);
  Need(actual.SequenceEqual(expected),"full ordered independent geometry schema");
  var wire=new byte[actual.Length+1];actual.CopyTo(wire,0);wire[^1]=10;
  Need((long)Get(value,"Bytes")==wire.Length,"exact JSON plus LF bytes");
  Need((string)Get(value,"Sha256")==Convert.ToHexString(SHA256.HashData(wire)).ToLowerInvariant(),"exact full canonical JSON plus LF hash");
 }
 static void GeometryWireFixtures()
 {
  var f=new GeometryWireFixture(Source);using var scope=f.Scope();var owner=f.Writer();
  foreach(bool germ in new[]{false,true})
  {
   object input=germ?f.Germ():f.Background();using var value=(IDisposable)Call(owner,germ?"Germ":"Background",input)!;
   var body=GeometryBody(value);using var parsed=JsonDocument.Parse(body);
   Console.WriteLine(JsonSerializer.Serialize(new{kind=germ?"germ":"background",value=parsed.RootElement,bytes=(long)Get(value,"Bytes"),sha256=(string)Get(value,"Sha256")},Json));
  }
 }
 static void GeometryWireGuarded(string path)
 {
  var context=new AssemblyLoadContext("manufactured-geometry-wire-guarded",isCollectible:true);
  try{GeometryWireTests(context.LoadFromAssemblyPath(Path.GetFullPath(path)));Console.WriteLine("PASS guarded geometry wire tests: "+tests);}
  finally{context.Unload();}
 }
 static void GeometryWireTests(Assembly? assembly=null)
 {
  var f=new GeometryWireFixture(assembly??Source);
  Test("geometry writer full zero bodies preserve all ranks and exact independent JSON plus LF",()=>
  {
   using var scope=f.Scope();var calls=new List<(string,int)>();var owner=f.Writer(admission:(kind,count)=>calls.Add((kind,count)));
   foreach(bool germ in new[]{false,true})
   {
    object input=germ?f.Germ():f.Background();using var value=(IDisposable)Call(owner,germ?"Germ":"Background",input)!;
    GeometryCheckBody(value,input,germ);Need((long)Get(value,"Bytes")== (germ?1372171L:538292L),"fixed all-zero full14 body census");
   }
   Need(calls.SequenceEqual(new[]{("background",129556),("germ",330260)}),"complete coordinate census before snapshot");
   var report=Call(owner,"Snapshot")!;Need((long)Get(report,"Snapshots")==2&&(long)Get(report,"PayloadBytes")==1910463L&&!(bool)Get(report,"Failed"),"cumulative writer counters");
  });
  Test("geometry writer includes asymmetric curvature and final coordinate without transposition or truncation",()=>
  {
   using var scope=f.Scope();var owner=f.Writer();object b=f.Background(),g=f.Germ();
   ((Array)Get(Get(b,"Connection"),"R")).SetValue(f.Rational(-7,3),13,12,11,10);
   var frame=f.Matrix();((Array)Get(frame,"Data")).SetValue(f.Rational(5,2),13,12);GeometryWireFixture.Set(b,"Frame",frame);
   var last=f.Matrix();((Array)Get(last,"Data")).SetValue(f.Rational(-11,7),13,13);GeometryWireFixture.Set(g,"DeltaFrame",last);
   ((Array)Get(Get(g,"Palatini"),"R")).SetValue(f.Rational(13,5),12,13,10,11);
   using var bv=(IDisposable)Call(owner,"Background",b)!;using var gv=(IDisposable)Call(owner,"Germ",g)!;
   GeometryCheckBody(bv,b,false);GeometryCheckBody(gv,g,true);
  });
  Test("geometry writer preserves nonzero divided-monomial headers at j4 j10 and final j34",()=>
  {
   // Independently enumerated degree/a/b/c ordering gives these exact rows.
   using var scope=f.Scope();var owner=f.Writer();
   foreach(var row in new[]{(Jet:4,Multi:new[]{1,0,0,0}),(Jet:10,Multi:new[]{0,2,0,0}),(Jet:34,Multi:new[]{3,0,0,0})})
   {
    object g=f.Germ();GeometryWireFixture.Set(g,"JetIndex",row.Jet);GeometryWireFixture.Set(g,"Multiindex",row.Multi);
    using var value=(IDisposable)Call(owner,"Germ",g)!;GeometryCheckBody(value,g,true);
    Need((long)Get(value,"Bytes")== (row.Jet<10?1372171L:1372172L),"header decimal width changes exact body count");
   }
  });
  Test("geometry writer owns immutable bytes detached from later source mutation and repeated export",()=>
  {
   using var scope=f.Scope();object b=f.Background();using var value=(IDisposable)Call(f.Writer(),"Background",b)!;var first=GeometryBody(value);
   ((Array)Get(Get(b,"Frame"),"Data")).SetValue(f.Rational(99,1),13,13);GeometryWireFixture.Set(b,"Point",1);
   Need(GeometryBody(value).SequenceEqual(first),"caller source mutation cannot change owned body");first[0]=0;
   Need(GeometryBody(value)[0]==(byte)'{',"exported bytes cannot mutate private backing");value.Dispose();GeometryRefused(()=>GeometryBody(value));
  });
  Test("geometry writer mandatory scope is enforced by ordinary and guarded assemblies",()=>
  {
   var owner=f.Writer();object b=f.Background();GeometryRefused(()=>Call(owner,"Background",b));
   Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"missing scope poisons writer");
  });
  Test("geometry value export requires a fresh healthy scope even after successful capture",()=>
  {
   IDisposable value;using(var scope=f.Scope())value=(IDisposable)Call(f.Writer(),"Background",f.Background())!;
   try{GeometryRefused(()=>GeometryBody(value));}finally{value.Dispose();}
  });
  Test("geometry writer invalid dimensions null slots and malformed fixed identities fail closed",()=>
  {
   Action<object>[] edits=[b=>GeometryWireFixture.Set(b,"Point",2),b=>GeometryWireFixture.Set(b,"Frame",null),b=>GeometryWireFixture.Set(b,"Frame",f.Matrix(13)),
    b=>GeometryWireFixture.Set(Get(b,"Frame"),"Data",Array.CreateInstance(f.Type("Rational"),13,14)),
    b=>((Array)Get(b,"Omega")).SetValue(null,13),b=>GeometryWireFixture.Set(Get(b,"Metric"),"DD",Array.CreateInstance(f.Type("Matrix"),14,13)),
    b=>GeometryWireFixture.Set(Get(b,"Connection"),"R",Array.CreateInstance(f.Type("Rational"),14,14,14,13))];
   foreach(var edit in edits){using var scope=f.Scope();var owner=f.Writer();object input=f.Background();edit(input);GeometryRefused(()=>Call(owner,"Background",input));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"shape failure sticky");}
   foreach(string field in new[]{"MetricBasis","JetIndex","Multiindex"})
   {using var scope=f.Scope();var owner=f.Writer();object input=f.Germ();GeometryWireFixture.Set(input,field,field=="Multiindex"?new[]{0,0,0,4}:field=="MetricBasis"?10:35);GeometryRefused(()=>Call(owner,"Germ",input));}
   using(var scope=f.Scope())
   {var owner=f.Writer();object input=f.Germ();GeometryWireFixture.Set(input,"Multiindex",new[]{1,0,0,0});GeometryRefused(()=>Call(owner,"Germ",input));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"valid multiindex with wrong jet identity rejected");}
  });
  Test("geometry writer late rational bit and formatting ceilings fail without returning a partial body",()=>
  {
   foreach(bool bits in new[]{false,true})
   {
    using var scope=f.Scope(bits?new Dictionary<string,long>{{"IntegerBits",64L},{"IntermediateBits",128L}}:new Dictionary<string,long>{{"FormatCharacters",1L}});
    var owner=f.Writer();object g=f.Germ();var last=f.Matrix();((Array)Get(last,"Data")).SetValue(f.Rational(bits?BigInteger.One<<128:1,1),13,13);GeometryWireFixture.Set(g,"DeltaFrame",last);
    GeometryRefused(()=>Call(owner,"Germ",g));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"late rational/format failure sticky");
   }
   using(var scope=f.Scope())
   {var owner=f.Writer();object g=f.Germ();var last=f.Matrix();((Array)Get(last,"Data")).SetValue(f.Rational(1,-2),13,13);GeometryWireFixture.Set(g,"DeltaFrame",last);
    GeometryRefused(()=>Call(owner,"Germ",g));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"negative raw denominator at final coordinate rejected");}
  });
  Test("geometry writer explicit array and wire budgets cannot reset across snapshots",()=>
  {
   using(var scope=f.Scope(new Dictionary<string,long>{{"ArraySlots",1L}})){var owner=f.Writer();GeometryRefused(()=>Call(owner,"Background",f.Background()));}
   foreach(int bytes in new[]{1,538291}){using var scope=f.Scope();var owner=f.Writer(payload:bytes);GeometryRefused(()=>Call(owner,"Background",f.Background()));}
   using(var scope=f.Scope())
   {var owner=f.Writer(total:538292);using var first=(IDisposable)Call(owner,"Background",f.Background())!;GeometryRefused(()=>Call(owner,"Background",f.Background()));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"cumulative wire limit sticky");}
   using(var scope=f.Scope())
   {var owner=f.Writer(snapshots:1);using var first=(IDisposable)Call(owner,"Background",f.Background())!;GeometryRefused(()=>Call(owner,"Background",f.Background()));}
  });
  Test("geometry writer exact payload limit succeeds and per-rational text limit rejects oversized coordinates",()=>
  {
   using(var scope=f.Scope())
   {using var value=(IDisposable)Call(f.Writer(payload:538292,total:538292),"Background",f.Background())!;Need((long)Get(value,"Bytes")==538292L,"exact whole-payload ceiling succeeds");}
   using(var scope=f.Scope())
   {object b=f.Background();var matrix=f.Matrix();((Array)Get(matrix,"Data")).SetValue(f.Rational(123,1),13,13);GeometryWireFixture.Set(b,"Frame",matrix);
    var owner=f.Writer(rational:2);GeometryRefused(()=>Call(owner,"Background",b));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"rational text ceiling poisons writer");}
  });
  Test("geometry writer admission precedes capture and source callback mutation cannot split body and hash",()=>
  {
   using var scope=f.Scope();object b=f.Background();int calls=0;var owner=f.Writer(admission:(kind,count)=>
   {calls++;Need(kind=="background"&&count==129556,"known prospective callback");GeometryWireFixture.Set(b,"Point",1);});
   using var value=(IDisposable)Call(owner,"Background",b)!;Need(calls==1,"one prospective admission");GeometryCheckBody(value,b,false);
  });
  Test("geometry writer swallowed admission reentry and callback exception poison writer permanently",()=>
  {
   foreach(bool reenter in new[]{false,true})
   {using var scope=f.Scope();object? owner=null;object b=f.Background();owner=f.Writer(admission:(_,_)=>
    {if(reenter)GeometryRefused(()=>Call(owner!,"Background",b));else throw new InvalidOperationException("manufactured admission failure");});
    GeometryRefused(()=>Call(owner,"Background",b));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"callback failure sticky");GeometryRefused(()=>Call(owner,"Background",b));}
  });
  Test("geometry writer rejects noncanonical ambient negative-sign culture before formatting",()=>
  {
   using var scope=f.Scope();var saved=CultureInfo.CurrentCulture;var changed=(CultureInfo)CultureInfo.InvariantCulture.Clone();changed.NumberFormat.NegativeSign="NEG";
   try{CultureInfo.CurrentCulture=changed;object b=f.Background();var frame=f.Matrix();((Array)Get(frame,"Data")).SetValue(f.Rational(-7,3),0,1);GeometryWireFixture.Set(b,"Frame",frame);
    var owner=f.Writer();GeometryRefused(()=>Call(owner,"Background",b));Need((bool)Get(Call(owner,"Snapshot")!,"Failed"),"noncanonical culture poisons writer");}
   finally{CultureInfo.CurrentCulture=saved;}
  });
 }
}
