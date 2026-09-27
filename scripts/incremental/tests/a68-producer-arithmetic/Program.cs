using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;
using System.Text.Json;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Manufactured exact arithmetic ONLY. No source geometry, GU tensors, FIRST,
// production caps, science output paths, or production project references.
static class Program
{
 static readonly MixedProducerAdmission.Limits Generous=new(4096,16384,1000000,100000000,1000000,1000000,1000000,1000000,1000000,128,1000000,new(1000000,1000000,1000000,1000000,1000000,1000000,10000000,1000000,10000000,1000000),new(64,100000,1000000,1000000,100000,1000000));
 static int tests;
 static void Need(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Test(string name,Action test){test();tests++;Console.WriteLine("PASS "+name);}
 static void Refuses(Action test)
 {try{test();}catch(MixedProducerAdmission.Refused){return;}throw new Exception("expected admission refusal");}
 static void EvidenceRefuses(Action test)
 {try{test();}catch(MixedTrace.EvidenceFailure){return;}throw new Exception("expected evidence refusal");}
 static void Foreign(Action test)
 {
  Exception? failure=null;var thread=new Thread(()=>{try{test();}catch(Exception ex){failure=ex;}}){IsBackground=true};
  thread.Start();Need(thread.Join(5000),"foreign ownership check must not deadlock");if(failure is not null)throw new Exception("foreign check failed",failure);
 }
 static void Value(Rational value,BigInteger numerator,BigInteger denominator)
 {Need(value.Numerator==numerator&&value.Denominator==denominator,"exact rational value");}
 sealed class ParameterProbe(Action touch){public string Value{get{touch();return "unexpected";}}}
 sealed class UnitParameterProbe(Action touch)
 {public int Form{get{touch();return 0;}}public int Blade=>0;public string Real=>"0";public string Imaginary=>"0";}
 static readonly MixedTrace.Limits WireLimits=new(100,100,1000,1000,1000000,1000000,1000000,1000000,1000000);
 // Manufactured fixture allowances only; the scientific sink has no defaults.
 static readonly MixedTrace.CopyCounts CopyCaps=new(1000,100000,100000,1000000,1000000);
 static readonly MixedTrace.CaptureLimits CaptureCaps=new(CopyCaps,CopyCaps,CopyCaps);
 static byte[] TensorWire(FT tensor,int degree,bool historical=false,MixedTrace.Limits? limits=null)
 {
  using var stream=new MemoryStream();
  using(var writer=new Utf8JsonWriter(stream,new(){Encoder=MixedTrace.JsonOptions.Encoder}))
  {
   if(!historical)MixedTrace.WriteTensor(writer,tensor,degree,limits??WireLimits);
   else
   {
    writer.WriteStartArray();foreach(var item in tensor.OrderBy(x=>x.Key))
    {
     writer.WriteStartObject();writer.WriteNumber("form",item.Key.Form);writer.WriteNumber("blade",item.Key.Blade);
     writer.WriteNumber("k0",0);writer.WriteNumber("k1",0);writer.WriteString("real",item.Value.Real.ToString());writer.WriteString("imaginary",item.Value.Imaginary.ToString());writer.WriteEndObject();
    }writer.WriteEndArray();
   }
  }
  return stream.ToArray();
 }
 static long EnumerateMetricJetBody(int basisLength)
 {
  long visits=0;
  for(int i=0;i<basisLength;i++)visits+=6; // two input/result/zeroing triples
  for(int a=0;a<10;a++)for(int b=0;b<10;b++)
  {
   visits+=2; // pair and t2 zeroes
   visits+=3+2+4; // pair product, t2 trace, Gram expression
   for(int c=0;c<10;c++)
   {visits+=1+3;for(int d=0;d<10;d++)visits+=1+3;} // t3/t4 zeroes and body
  }
  for(int x=0;x<10;x++)for(int a=0;a<10;a++)for(int b=0;b<10;b++)
  {visits+=7;for(int z=0;z<10;z++)visits+=17;}
  return visits;
 }
 static void Main()
 {
  Test("mandatory scope",()=>{Refuses(()=>_=new Rational(1));Refuses(()=>_=new Scalar(default(Rational),default(Rational)));Refuses(()=>_=default(Rational).ToString());Refuses(()=>_=Algebra.WordSign(1,2));});
  Test("default zero, reduction and signs",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);Value(default,0,1);Value(default(Rational)+new Rational(2,3),2,3);Value(new Rational(6,-9),-2,3);Value(new Rational(0,-17),0,1);Need(default(Rational)==new Rational(0),"default equality");});
  Test("manufactured rational operations",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);for(int a=-6;a<=6;a++)for(int b=1;b<=7;b++)for(int c=-3;c<=3;c++)for(int d=1;d<=4;d++)
   {var x=new Rational(a,b);var y=new Rational(c,d);Check(x+y,a*d+c*b,b*d);Check(x-y,a*d-c*b,b*d);Check(x*y,a*c,b*d);}
   void Check(Rational value,int n,int d){int g=(int)BigInteger.GreatestCommonDivisor(BigInteger.Abs(n),d);Value(value,n/g,d/g);}});
  Test("complex multiplication and conjugation",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=new Scalar(new Rational(2,3),new Rational(-3,5));var b=new Scalar(new Rational(-7,2),new Rational(5,4));var c=a*b;Value(c.Real,-19,12);Value(c.Imaginary,44,15);Value(a.Conjugate().Imaginary,3,5);});
  Test("word product retains Clifford sign",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{WordVisits=100000000});foreach(int a in new[]{0,1,127,128,8192,16383,2184})foreach(int b in new[]{0,2,255,512,16383,1092})Need(Algebra.WordSign(a,b)==Algebra.BladeSign(a,b),"word sign");});
  Test("oversized constructor refused before GCD",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{IntermediateBits=4096});var huge=BigInteger.One<<4096;Refuses(()=>_=new Rational(huge,1));Need(s.Snapshot().GcdCalls==0&&s.Snapshot().Poisoned,"no GCD before refusal");Refuses(()=>_=new Rational(1));});
  Test("prospective product refused before integer work",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=128,IntermediateBits=128});var a=new Rational(BigInteger.One<<80,1);var before=s.Snapshot();Refuses(()=>_=a*a);var after=s.Snapshot();Need(after.IntegerOperations==before.IntegerOperations&&after.IntegerObjects==before.IntegerObjects,"product was not admitted");});
  Test("prospective addition refused before integer work",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=128,IntermediateBits=128});var a=BigInteger.One<<127;Refuses(()=>s.Binary("add",a,a));Need(s.Snapshot().IntegerOperations==0,"sum was not admitted");});
  Test("GCD call ceiling and poison",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{GcdCalls=1});_=new Rational(2,3);Refuses(()=>_=new Rational(3,4));Need(s.Snapshot().GcdCalls==1,"GCD ceiling");Refuses(()=>_=default(Rational).ToString());});
  Test("zero denominator retains exception and poisons",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);bool caught=false;try{_=new Rational(1,0);}catch(DivideByZeroException){caught=true;}Need(caught&&s.Snapshot().Poisoned,"domain failure poison");Refuses(()=>_=new Rational(1));});
  Test("formatting admission precedes formatting",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{FormatCharacters=1});var a=new Rational(-12,7);Refuses(()=>_=a.ToString());Need(s.Snapshot().FormatCharacters==0,"no formatted characters admitted");});
  Test("format preserves source culture behavior",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var previous=CultureInfo.CurrentCulture;try{var culture=(CultureInfo)CultureInfo.InvariantCulture.Clone();culture.NumberFormat.NegativeSign="MINUS";CultureInfo.CurrentCulture=culture;Need(new Rational(-2).ToString()=="-2","integer invariant");Need(new Rational(-2,3).ToString()=="MINUS2/3","fraction original interpolation culture");}finally{CultureInfo.CurrentCulture=previous;}});
  Test("word refuses before allocation",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{WordVisits=1});Refuses(()=>_=Algebra.WordSign(16383,16383));Need(s.Snapshot().WordVisits==0,"no word visits admitted");});
  Test("scratch and object ceilings poison",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous with{PrimitiveScratchSlots=1})){Refuses(()=>_=new Rational(1));Need(s.Snapshot().Poisoned,"scratch poison");}using(var s=MixedProducerAdmission.Enter(Generous with{IntegerObjects=1})){Refuses(()=>_=new Rational(1));Need(s.Snapshot().IntegerObjects==1,"object ceiling");}});
  Test("invalid limits and nested scopes rejected",()=>
  {Refuses(()=>MixedProducerAdmission.Enter(Generous with{GcdCalls=0}));using var s=MixedProducerAdmission.Enter(Generous);Refuses(()=>MixedProducerAdmission.Enter(Generous));Need(s.Snapshot().Poisoned,"nested scope poison");});
  Test("scope is thread-local and disposal removes admission",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){Exception? error=null;var t=new Thread(()=>{try{_=new Rational(1);}catch(Exception e){error=e;}});t.Start();t.Join();Need(error is MixedProducerAdmission.Refused,"child thread has no inherited authorization");}Refuses(()=>_=new Rational(1));});
  Test("binding same opened bytes and pre-read admission",()=>
  {string path=Path.GetTempFileName();try{byte[] bytes=[1,2,3,4];File.WriteAllBytes(path,bytes);string hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();using(var s=MixedProducerAdmission.Enter(Generous)){Need(new Binding("synthetic",path,hash).hashMatches,"exact hash");Need(s.Snapshot().InputBytes==5,"charged same bytes plus sentinel");}using(var s=MixedProducerAdmission.Enter(Generous with{InputBytes=4})){Refuses(()=>_=new Binding("synthetic",path,hash).hashMatches);Need(s.Snapshot().InputBytes==0,"sentinel also admitted before read");Refuses(()=>_=new Binding("synthetic",path+".absent",hash).hashMatches);}}finally{File.Delete(path);}});
  Test("tensor operations require scope even on empty input",()=>
  {var empty=new FT();Refuses(()=>_=Fourier.Empty());Refuses(()=>_=Fourier.Typed(empty,0));Refuses(()=>_=Fourier.Terms(empty));Refuses(()=>_=Fourier.PConstant([],empty));});
  Test("Put sums and removes without mutating sources",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,1,2);var b=Fourier.One(2,2,3);var sum=Fourier.Add(a,b);Need(sum.Count==2&&a.Count==1&&b.Count==1,"copy isolation");Fourier.Put(sum,(1,1,0,0),-2);Need(sum.Count==1&&!sum.ContainsKey((1,1,0,0)),"exact cancellation removes");var before=s.Snapshot();Fourier.Put(sum,(8,8,0,0),0);Need(s.Snapshot().Tensors.Mutations==before.Tensors.Mutations,"zero Put no mutation");});
  Test("manufactured grouped and word products agree",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.Empty();Fourier.Put(a,(1,1,1,0),new Scalar(new Rational(2,3),new Rational(1,5)));Fourier.Put(a,(2,128,-1,2),-3);var b=Fourier.Empty();Fourier.Put(b,(4,3,0,-2),2);Fourier.Put(b,(1,128,1,0),new Scalar(0,new Rational(2,7)));foreach(char kind in new[]{'W','C','A'})Need(Fourier.Equal(Fourier.Product(a,b,kind),Fourier.NaiveProduct(a,b,kind)),"literal/word product");});
  Test("product numerical signs, modes and unrestricted blades",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,1023,2);var b=Fourier.One(2,4096,3);var p=Fourier.Product(a,b);Need(p.Count==1&&p.ContainsKey((3,5119,0,0)),"grade11 preserved");Value(p[(3,5119,0,0)].Real,6*Algebra.BladeSign(1023,4096),1);var t=Fourier.Trig(1,true,1,1);var derivative=Fourier.Partial(t,1);Need(Fourier.Equal(derivative,Fourier.Trig(1,false,1,1)),"sine derivative and Fourier modes");});
  Test("Hodge Omega adjoint Pair and Top",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,1,2);Need(Fourier.Typed(Fourier.Star(a),13),"star degree");Need(Fourier.Equal(Fourier.HAdjoint(Fourier.HAdjoint(a)),a),"adjoint involution");Need(Fourier.HAnti(a),"anti-Hermitian vector");Need(Fourier.Omega(a).ContainsKey((1,Fourier.Full^1,0,0)),"omega full grade");Value(Fourier.Pair(a,a),-4,1);Value(Fourier.Top(Fourier.One(Fourier.Full,0,7)),-7,1);});
  Test("polynomial and Jet operations retain Leibniz order",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(0,0,2);var b=Fourier.One(0,0,3);var c=Fourier.One(0,0,5);var d=Fourier.One(0,0,7);var j=Jet.Product(new(a,b),new(c,d));Value(j.Value[(0,0,0,0)].Real,10,1);Value(j.Delta[(0,0,0,0)].Real,29,1);var p=Fourier.PMul([a,b],[c,d]);Need(p.Length==3,"polynomial length");Value(p[1][(0,0,0,0)].Real,29,1);Need(Fourier.PAdd([a],[a,b]).Length==2,"ragged add");Need(Fourier.PConstant(Fourier.PScale([a,Fourier.Empty()],2),Fourier.Scale(a,2)),"constant polynomial");Need(Fourier.REqual(Fourier.RAdd([new Rational(2)],[new Rational(3),new Rational(4)]),[new Rational(5),new Rational(4)]),"scalar polynomials");});
  Test("empty chains and derivative helpers remain complete",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var z=Fourier.Empty();Need(Fourier.Chain(z,z,z,false).Count==0,"empty omitted first leg");Need(Jet.Chain(Jet.Fixed(z),Jet.Fixed(z),Jet.Fixed(z),false).Value.Count==0,"dual empty chain");Need(Fourier.Covariant(z,z).Count==0&&Fourier.Conjugate(z,z,z).Count==0,"composites");Need(Fourier.PD([z]).Length==1&&Fourier.PChain([z],z,z,true).Length==1,"polynomial composites");Need(Fourier.PPair([z],[z]).Length==1,"pair polynomial");Need(Fourier.RScale([new Rational(3)],new Rational(2))[0]==new Rational(6),"scalar scale");});
  Test("full Cartesian charge precedes overlap pruning",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{PairVisits=1}});var a=Fourier.Empty();Fourier.Put(a,(1,1,0,0),1);Fourier.Put(a,(1,2,0,0),1);var b=Fourier.One(1,4,1);long dictionaries=s.Snapshot().Tensors.Dictionaries;Refuses(()=>_=Fourier.Product(a,b));Need(s.Snapshot().Tensors.PairVisits==0&&s.Snapshot().Tensors.Dictionaries==dictionaries,"refused before product allocation despite all pairs zero");});
  Test("zero algebra coefficients still charge Cartesian pairs",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,0,1);var b=Fourier.One(2,0,1);var before=s.Snapshot();Need(Fourier.Product(a,b,'C').Count==0,"commuting scalar bracket");Need(s.Snapshot().Tensors.PairVisits-before.Tensors.PairVisits==1,"unpruned pair charge");});
  Test("grouped left records reread for each right group are charged",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,0,1);var b=Fourier.Empty();Fourier.Put(b,(2,0,0,0),1);Fourier.Put(b,(4,0,0,0),1);var before=s.Snapshot();Need(Fourier.Product(a,b,'C').Count==0,"all scalar brackets vanish");var after=s.Snapshot();Need(after.Tensors.PairVisits-before.Tensors.PairVisits==2,"two full Cartesian pairs");Need(after.Tensors.RecordVisits-before.Tensors.RecordVisits==10,"seven record reads plus three group iterations, no Put accounting substitution");});
  Test("support refusal preserves all existing records",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{RecordsPerTensor=1}});var a=Fourier.One(1,1,2);Refuses(()=>Fourier.Put(a,(2,2,0,0),3));Need(a.Count==1&&a.ContainsKey((1,1,0,0))&&!a.ContainsKey((2,2,0,0)),"no entry discarded or partially inserted");});
  Test("clone admission precedes cloned dictionary",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{Dictionaries=1}});var a=Fourier.One(1,1,2);Refuses(()=>_=Fourier.Add(a,new FT()));Need(s.Snapshot().Tensors.Dictionaries==1&&a.Count==1,"clone refused before creation");});
  Test("mutation cap precedes cancellation removal",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{Mutations=1}});var a=Fourier.One(1,1,2);var negative=new Scalar(-2);Refuses(()=>Fourier.Put(a,(1,1,0,0),negative));Need(a.Count==1&&a[(1,1,0,0)].Real.Numerator==2,"removal not performed");});
  Test("Terms sorting and format retain full wire fields",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.Empty();Fourier.Put(a,(2,128,3,-4),new Scalar(new Rational(2,3),new Rational(-1,5)));Fourier.Put(a,(1,1,0,0),7);var terms=Fourier.Terms(a);string json=System.Text.Json.JsonSerializer.Serialize(terms);Need(json=="[{\"form\":1,\"blade\":1,\"k0\":0,\"k1\":0,\"real\":\"7\",\"imaginary\":\"0\"},{\"form\":2,\"blade\":128,\"k0\":3,\"k1\":-4,\"real\":\"2/3\",\"imaginary\":\"-1/5\"}]","exact terms order and fields");});
  Test("sort admission occurs before formatting",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{SortWork=1}});var a=Fourier.One(1,1,2);Refuses(()=>_=Fourier.Terms(a));Need(s.Snapshot().FormatCharacters==0,"no formatting before sort admission");});
  Test("sequence admission precedes polynomial work",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{SequenceSlots=1}});var a=Fourier.One(0,0,1);long count=s.Snapshot().Tensors.Dictionaries;Refuses(()=>_=Fourier.PAdd([a,a],[a]));Need(s.Snapshot().Tensors.Dictionaries==count,"no polynomial outputs allocated");});
  Test("domain failure poisons full tensor context",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Fourier.One(1,1,1);bool threw=false;try{_=Fourier.Top(a);}catch(InvalidOperationException){threw=true;}Need(threw&&s.Snapshot().Poisoned,"Top type failure poison");Refuses(()=>_=Fourier.Empty());});
  Test("oversized scale parameters cannot pass empty paths",()=>
  {Rational r;Scalar c;using(var s=MixedProducerAdmission.Enter(Generous)){r=new Rational(BigInteger.One<<100,1);c=new Scalar(default,r);}var low=Generous with{IntegerBits=64};using(var s=MixedProducerAdmission.Enter(low)){Refuses(()=>_=Fourier.Scale(new FT(),c));Need(s.Snapshot().Tensors.Dictionaries==0,"scale refused before output allocation");}using(var s=MixedProducerAdmission.Enter(low))Refuses(()=>_=Fourier.PScale([],c));using(var s=MixedProducerAdmission.Enter(low))Refuses(()=>_=Fourier.RScale([],r));});
  Test("matrix construction requires scope before backing allocation",()=>
  {Refuses(()=>_=new Matrix(2));using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{Cells=3}});Refuses(()=>_=new Matrix(2));Need(s.Snapshot().Matrices.Cells==0,"matrix backing cells not admitted");});
  Test("small manufactured matrix arithmetic and cloning",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=new Matrix(2);a[0,0]=1;a[0,1]=2;a[1,0]=3;a[1,1]=4;var b=Matrix.Diagonal(5,6);var p=a*b;Value(p[0,0],5,1);Value(p[0,1],12,1);Value(p[1,0],15,1);Value(p[1,1],24,1);Need((p-b).Same(p+b.Scale(-1)),"subtraction order");var copy=a.Copy();copy[0,0]=9;Value(a[0,0],1,1);Value(a.Trace(),5,1);Value(Matrix.TraceProduct(a,b),29,1);Need(!a.Symmetric&&a.Nonzero==4&&!a.Zero,"complete scans");});
  Test("small pivoting inverse and determinant",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=new Matrix(2);a[0,1]=2;a[1,0]=3;a[1,1]=4;var inverse=a.Inverse();Need((a*inverse).Same(Matrix.Identity(2))&&(inverse*a).Same(Matrix.Identity(2)),"two-sided inverse");Value(a.Determinant(),-6,1);Value(inverse[0,0],new System.Numerics.BigInteger(-2),new System.Numerics.BigInteger(3));Value(inverse[0,1],1,3);Value(inverse[1,0],1,2);});
  Test("small inertia diagonal and off-diagonal pivots",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);Need(Matrix.Diagonal(2,-3,0).Inertia()==(1,1,1),"signed diagonal inertia");var a=new Matrix(2);a[0,1]=a[1,0]=2;Need(a.Inertia()==(1,1,0),"off-diagonal pivot");var zero=new Matrix(2);Need(zero.Inertia()==(0,0,2)&&zero.Determinant()==new Rational(0),"singular zero controls");});
  Test("empty matrices retain zero-dimensional semantics",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var z=new Matrix(0);Need(z.Inverse().N==0&&z.Inertia()==(0,0,0)&&z.Text().Length==0,"empty matrix methods");Value(z.Determinant(),1,1);Value(z.Trace(),0,1);Need((z*z).Same(z),"empty multiplication");});
  Test("matrix product full visits precede zero pruning",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=new Matrix(2);var b=new Matrix(2);var before=s.Snapshot();var product=a*b;var after=s.Snapshot();Need(after.Matrices.SlotVisits-before.Matrices.SlotVisits==56,"two n2 validation scans, n2 outer tests, five touches per product and n2 result zeroes");Need(product.Zero,"all arithmetic skipped still visits charged");});
  Test("matrix product cap refuses before result allocation",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=59}});var a=new Matrix(2);var b=new Matrix(2);Refuses(()=>_=a*b);Need(s.Snapshot().Matrices.Objects==2,"eight input zeroes plus52 pre-output accesses need60 before result construction");});
  Test("inverse dense envelope refuses before scratch matrices",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=10}});var a=new Matrix(2);Refuses(()=>_=a.Inverse());Need(s.Snapshot().Matrices.Objects==1,"no inverse copies allocated");});
  Test("matrix singularity and dimension failures poison",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){bool threw=false;try{_=new Matrix(2).Inverse();}catch(InvalidOperationException){threw=true;}Need(threw&&s.Snapshot().Poisoned,"singular inverse poisons");Refuses(()=>_=new Matrix(1));}using(var s=MixedProducerAdmission.Enter(Generous)){var a=new Matrix(1);var b=new Matrix(2);bool threw=false;try{_=a+b;}catch(ArgumentException){threw=true;}Need(threw&&s.Snapshot().Poisoned,"shape failure poisons");}});
  Test("matrix text arrays are admitted before formatting",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){var a=Matrix.Diagonal(2,3);var text=a.Text();Need(text.Length==2&&text[0][0]=="2"&&text[0][1]=="0"&&text[1][1]=="3","complete text");Need(s.Snapshot().Matrices.ArraySlots==6&&s.Snapshot().Matrices.ArrayObjects==3,"jagged shape charge");}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=5}})){var a=new Matrix(2);Refuses(()=>_=a.Text());Need(s.Snapshot().FormatCharacters==0,"no format before raw-array admission");}});
  Test("matrix clone validates borrowed coefficients before copying",()=>
  {Matrix a;Rational large;using(var s=MixedProducerAdmission.Enter(Generous)){a=new Matrix(1);large=new Rational(BigInteger.One<<100,1);a.Data[0,0]=large;}using(var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64})){Refuses(()=>_=a.Copy());Need(s.Snapshot().Matrices.Objects==0,"no clone before borrowed input admission");}using(var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64})){var z=new Matrix(0);Refuses(()=>_=z.Scale(large));}});
  Test("small generic solve and power preserve numeric expressions",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Matrix.Diagonal(2,3);var v=Geometry.Solve(a,[new Rational(5),new Rational(7)]);Value(v[0],10,1);Value(v[1],21,1);Value(Geometry.Pow(new Rational(2,3),3),8,27);Value(Geometry.Pow(new Rational(7),-2),1,1);Need(Geometry.Commutator(a,Matrix.Identity(2)).Zero,"generic commutator");});
  Test("invalid matrix access and short solve poison before access",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){var a=new Matrix(2);bool failed=false;try{_=a[-1,0];}catch(IndexOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"bad getter poison");}using(var s=MixedProducerAdmission.Enter(Generous)){var a=new Matrix(2);bool failed=false;try{a[0,2]=default;}catch(IndexOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"bad setter poison");}using(var s=MixedProducerAdmission.Enter(Generous)){var a=new Matrix(2);bool failed=false;try{_=Geometry.Solve(a,[]);}catch(IndexOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArrayObjects==0,"short vector refused before output array");}});
  Test("geometry scalar parameters admitted before matrix work",()=>
  {Rational large;Matrix empty;using(var s=MixedProducerAdmission.Enter(Generous)){large=new Rational(BigInteger.One<<100,1);empty=new Matrix(0);}var low=Generous with{IntegerBits=64};using(var s=MixedProducerAdmission.Enter(low)){Refuses(()=>_=Geometry.Metric(empty,empty,empty,default,large));Need(s.Snapshot().Matrices.Objects==0,"Metric parameters before products");}using(var s=MixedProducerAdmission.Enter(low)){Refuses(()=>_=Geometry.DMetric(empty,empty,empty,empty,large,default));Need(s.Snapshot().Matrices.Objects==0,"DMetric parameters before products");}using(var s=MixedProducerAdmission.Enter(low)){Refuses(()=>_=Geometry.DDMetric(empty,empty,empty,empty,empty,default,large));Need(s.Snapshot().Matrices.Objects==0,"DDMetric parameters before products");}});
  Test("Mixed raw empty requires scope and stays untraced",()=>
  {Refuses(()=>_=MixedProducerStages.Empty());using var s=MixedProducerAdmission.Enter(Generous);var z=Mixed.ExteriorComponent(new FT(),2,2);Need(z.Count==0&&s.Snapshot().Tensors.Dictionaries==1,"one raw lazy zero");});
  Test("Mixed elementary tensor operations retain exact coefficients",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Mixed.Unit(3,1,new Scalar(2,3));var b=Mixed.Times(a,new Rational(-1,2));var sum=Mixed.Sum(a,b);Need(sum.Count==1,"sum support");Value(sum[(3,1,0,0)].Real,1,1);Value(sum[(3,1,0,0)].Imaginary,3,2);var component=Mixed.Component(sum,3);Need(component.ContainsKey((0,1,0,0)),"component strips form");var contract=Mixed.Contract(sum,1);Value(contract[(1,1,0,0)].Real,-1,1);Need(Mixed.Equal(a,Mixed.Raise(a)),"positive metric raise");Value(Mixed.Norm(sum),5,2);});
  Test("Mixed transpose literal and word routes agree on manufactured tensors",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var a=Mixed.Unit(1,2,new Scalar(2,1));var y=Mixed.Unit(3,1,new Scalar(3,-1));foreach(char kind in new[]{'C','A'})Need(Mixed.Equal(Mixed.Transpose(a,y,kind),Mixed.Transpose(a,y,kind,true)),"full transpose equality");});
  Test("Mixed transpose admits Cartesian pairs before form rejection",()=>
  {FT a,b;using(var s=MixedProducerAdmission.Enter(Generous)){a=Fourier.One(3,0,1);b=Fourier.Add(Fourier.One(1,0,1),Fourier.One(2,0,1));}foreach(bool oracle in new[]{false,true}){using var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{PairVisits=1}});Refuses(()=>_=Mixed.Transpose(a,b,'C',oracle));Need(s.Snapshot().Tensors.Dictionaries==0&&s.Snapshot().Poisoned,"no transpose allocation before pair refusal");}});
  Test("Mixed oracle transpose reserves repeated Bits arrays before pruning",()=>
  {FT a,b;using(var s=MixedProducerAdmission.Enter(Generous)){a=Fourier.One(1,0,1);b=Fourier.One(3,0,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=839}})){Refuses(()=>_=Mixed.Transpose(a,b,'C',true));Need(s.Snapshot().Tensors.Dictionaries==0,"full15 Bits calls reserved before output");}});
  Test("Mixed motion preserves heterogeneous-degree manufactured input",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(14);m[0,1]=2;m[1,0]=3;var t=Mixed.Sum(Mixed.Unit(0,4,7),Mixed.Unit(1,4,5),Mixed.Unit(3,4,11));var a=Mixed.Motion(m,t);var b=Mixed.Motion(m,t,true);Need(Mixed.Equal(a,b)&&a.Count==1,"no homogeneous-degree narrowing");Value(a[(2,4,0,0)].Real,10,1);});
  Test("manufactured off-shell mass keeps residual pairing motion without double counting",()=>
  {
   // Two signed form coordinates and ONE real H-anti Clifford blade only.
   // This is a finite-dimensional algebra regression, not GU geometry, a
   // stationary background, a joint Hessian, propagation or a mass prediction.
   using var s=MixedProducerAdmission.Enter(Generous);
   var a=new Matrix(14);a[0,7]=1;Value(a.Trace(),0,1);
   var e0=Mixed.Unit(1,1,1);var e7=Mixed.Unit(1<<7,1,1);
   Value(Fourier.Pair(e0,e0),-1,1);Value(Fourier.Pair(e7,e7),1,1);Value(Fourier.Pair(e0,e7),0,1);
   var x=Mixed.Sum(e0,Mixed.Times(e7,2));var u=Mixed.Sum(Mixed.Times(e0,3),e7);
   Rational kappa=3;var gradient=Mixed.Times(x,kappa);
   Need(Fourier.HAnti(x)&&Fourier.HAnti(u)&&gradient.Count==2,"full manufactured carrier and nonzero off-shell gradient");
   Value(Fourier.Pair(u,gradient),-3,1);

   // Independent direct bivariate action. The native-to-adapted shear is
   // (z0,z7) -> (z0,z7+h*z0), with determinant EXACTLY1. At native X+u*U,
   // z0=1+3u, z7=2+u+h+3hu, so L(h,u)=3/2*(-z0^2+z7^2).
   // Compute all four coefficients by literal scalar convolution, not by
   // Motion/Adjoint/PairingMotion, a gradient identity or finite differences.
   Rational[,] z0={{1,3},{0,0}},z7={{2,1},{1,3}},density=new Rational[2,2];
   for(int h=0;h<2;h++)for(int v=0;v<2;v++)
    for(int i=0;i<=h;i++)for(int j=0;j<=v;j++)
     density[h,v]+=new Rational(3,2)*(z7[i,j]*z7[h-i,v-j]-z0[i,j]*z0[h-i,v-j]);
   Value(density[0,0],9,2);Value(density[0,1],-3,1);Value(density[1,0],6,1);Value(density[1,1],21,1);
   foreach(bool oracle in new[]{false,true})
   {
    var adaptedGradientDelta=Mixed.Times(Mixed.Motion(a,x,oracle),kappa);
    var residualCorrection=Mixed.MotionAdjoint(a,gradient,oracle);
    var correct=Mixed.Sum(adaptedGradientDelta,residualCorrection);
    var doubled=Mixed.Sum(adaptedGradientDelta,Mixed.PairingMotion(a,gradient,oracle));
    Need(Mixed.Equal(correct,Mixed.Times(Mixed.PairingMotion(a,x,oracle),kappa)),"adapted derivative plus signed adjoint residual equals fixed-native pairing motion");
    Need(Fourier.Pair(u,correct)==density[1,1],"full mixed mass coefficient equals independent bivariate action");
    Value(Fourier.Pair(u,adaptedGradientDelta),3,1);Value(Fourier.Pair(u,residualCorrection),18,1);Value(Fourier.Pair(u,doubled),24,1);
    Need(Fourier.Pair(u,adaptedGradientDelta)!=density[1,1],"deleting nonzero residual correction is a detectable error");
    Need(Fourier.Pair(u,doubled)!=density[1,1],"adding V1G to an already adapted derivative double-counts T1G");
   }
   Need(!s.Snapshot().Poisoned&&MixedTrace.Active is null,"manufactured algebra only; no recorder or scientific sink");
  });
  Test("Mixed motion reserves full slots even zero map",()=>
  {FT t;Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);t=Fourier.One(1,0,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=3555}})){Refuses(()=>_=Mixed.Motion(m,t));Need(s.Snapshot().Tensors.Dictionaries==0,"196 map validation plus28 Bits output touches plus17*196 replacement touches before result");}});
  Test("Mixed motion oracle clone envelope precedes zero pruning",()=>
  {FT t;Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);t=Fourier.One(1,0,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=2799}})){Refuses(()=>_=Mixed.Motion(m,t,true));Need(s.Snapshot().Tensors.Dictionaries==0,"56 Bits plus2744 clone slots reserved");}});
  Test("Mixed two-form slices preserve105 lazy objects and reversed aliases",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var t=Fourier.One(5,8,7);var before=s.Snapshot();var slices=Mixed.TwoFormSlices(t);Need(s.Snapshot().Tensors.Dictionaries-before.Tensors.Dictionaries==105,"exact105 slice dictionaries");Need(ReferenceEquals(slices[0,2],slices[2,0])&&!ReferenceEquals(slices[0,0],slices[1,1]),"slice object identities");Value(slices[0,2][(0,8,0,0)].Real,7,1);Need(slices[0,0].Count==0,"diagonal stays empty");});
  Test("Mixed slice arrays admitted before any dictionary",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=286}});Refuses(()=>_=Mixed.TwoFormSlices(new FT()));Need(s.Snapshot().Tensors.Dictionaries==0,"196 grid plus91 trace input slots admitted first");});
  Test("Mixed malformed domains poison",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){bool failed=false;try{_=Mixed.TwoFormSlices(Fourier.One(1,0,1));}catch(ArgumentException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"slice degree");}using(var s=MixedProducerAdmission.Enter(Generous)){bool failed=false;try{_=Mixed.HodgeAdjoint(Fourier.One(1,0,1),2);}catch(ArgumentException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"Hodge domain");}using(var s=MixedProducerAdmission.Enter(Generous)){bool failed=false;try{_=Mixed.Contract(new FT(),14);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"axis domain");}});
  Test("Mixed recorded matrices reject dimensions before tensor allocation",()=>
  {foreach(int n in new[]{13,15}){using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(n);bool failed=false;try{_=Mixed.Motion(m,new FT());}catch(ArgumentException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Tensors.Dictionaries==0,"recorded matrix dimension");}});
  Test("Mixed pullback atomic form cache preserves manufactured forms",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(14);m[0,1]=2;m[1,0]=3;var t=Mixed.Sum(Mixed.Unit(0,8,5),Mixed.Unit(1,8,7),Mixed.Unit(3,8,11));var a=Mixed.Pullback(m,t);var b=Mixed.Pullback(m,t,true);Need(Mixed.Equal(a,b)&&a.Count==3,"complete pullback routes");Value(a[(0,8,0,0)].Real,5,1);Value(a[(2,8,0,0)].Real,14,1);Value(a[(3,8,0,0)].Real,-66,1);});
  Test("Mixed pullback sorting is admitted before cache work",()=>
  {FT t;Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);t=Fourier.One(1,0,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Tensors=Generous.Tensors with{SortWork=15}})){Refuses(()=>_=Mixed.Pullback(m,t));Need(s.Snapshot().Tensors.Dictionaries==0,"sorting uppercharge before cache seeds");}});
  Test("Mixed composite arrays are reserved before chain operators",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=9}});Refuses(()=>_=Mixed.FixedForward(new FT()));Need(s.Snapshot().Tensors.Dictionaries==0,"forward result and params arrays before any operators or Phi construction");});
  Test("Mixed descriptive counter overflow poisons",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);bool failed=false;try{_=MixedProducerStages.Add(long.MaxValue,1);}catch(OverflowException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"checked metadata counter");});
  Test("Mixed positive absolute value validates borrowed height",()=>
  {Rational large;using(var s=MixedProducerAdmission.Enter(Generous)){large=new Rational(BigInteger.One<<100,1);}using(var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64})){Refuses(()=>_=Mixed.Abs(large));Need(s.Snapshot().Poisoned,"positive branch still checks input");}});
  Test("Mixed product admits borrowed coefficients before overlap pruning",()=>
  {FT a,b;using(var s=MixedProducerAdmission.Enter(Generous)){a=Fourier.One(1,0,new Scalar(default,new Rational(BigInteger.One<<100,1)));b=Fourier.One(1,0,1);}foreach(bool oracle in new[]{false,true}){using var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64});Refuses(()=>_=Mixed.P(a,b,'W',oracle));Need(s.Snapshot().Tensors.Dictionaries==0&&s.Snapshot().Poisoned,"oversized skipped coefficient refused before result");}});
  Test("Mixed stage guards preserve lazy-zero and atomic pullback node order",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],new(100,100,1000,1000,1000000,1000000,1000000,1000000,1000000),(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=trace;var m=new Matrix(14);m[0,1]=2;m[1,0]=3;var input=Mixed.Unit(3,8,11);var lazy=Mixed.ExteriorComponent(input,1,1);Need(trace.Snapshot().Nodes.Length==1,"raw zero not recorded eagerly");var moved=Mixed.Pullback(m,input);var sum=Mixed.Sum(lazy,moved);var graph=trace.Finish();Need(graph.Nodes.Select(n=>n.Op).SequenceEqual(new[]{"unit","pullback","zero","sum"}),"no internal seed or guard nodes");Need(graph.Nodes[1].Inputs.SequenceEqual(new[]{0})&&graph.Nodes[3].Inputs.SequenceEqual(new[]{2,1}),"unchanged recorded input order");Value(sum[(3,8,0,0)].Real,-66,1);});
  Test("Mixed matrix transpose keeps larger unrecorded input domain",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(15);m[0,7]=2;m[14,14]=9;var r=Mixed.MetricTranspose(m);Need(r.N==14,"fixed output shape");Value(r[7,0],-2,1);});
  Test("Mixed checked admission products refuse arithmetic overflow",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);Refuses(()=>_=s.CountProduct(long.MaxValue,2));Need(s.Snapshot().Poisoned,"admission count overflow");});
  Test("sealed recorder rejects late operations before serialization or callbacks",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);int callbacks=0,getters=0;
   using var trace=new MixedTrace.Session(CaptureCaps,[],new(100,100,1000,1000,1000000,1000000,1000000,1000000,1000000),(_,_,_)=>{},(_,_)=>callbacks++);
   MixedTrace.Active=trace;_=Mixed.Unit(1,0,1);var graph=trace.Finish();int before=callbacks;
   void Closed(Action action){try{action();}catch(MixedTrace.EvidenceFailure){return;}throw new Exception("sealed recorder accepted operation");}
   Closed(()=>trace.Record("unit",new FT(),[],new ParameterProbe(()=>getters++)));
   Closed(()=>trace.RegisterLeaf("late",1,null!));Closed(()=>trace.Mark("late",1,null!,false));Closed(()=>trace.Finish());
   Need(getters==0&&callbacks==before,"no parameter work after seal");
   Need(trace.Snapshot().Nodes.Length==graph.Nodes.Length&&trace.Snapshot().Marks.Length==0,"sealed graph unchanged");
  });
  Test("logical source-touch model preserves exact tiny matrix kernel envelopes",()=>
  {
   for(int n=0;n<=3;n++)
   {
    using var s=MixedProducerAdmission.Enter(Generous);
    void Cost(Action run,long expected,string name){long before=s.Snapshot().Matrices.SlotVisits;run();Need(s.Snapshot().Matrices.SlotVisits-before==expected,name+" n="+n);}
    long n2=(long)n*n,n3=n2*n;Matrix? a=null,b=null;
    Cost(()=>a=new Matrix(n),n2,"constructor zeroing");Cost(()=>b=new Matrix(n),n2,"second constructor");
    var left=a!;var right=b!;
    Cost(()=>_=Matrix.Identity(n),n2+n,"identity");Cost(()=>_=Matrix.Diagonal(new long[n]),n2+2*n,"diagonal input+store");
    Cost(()=>_=left.Copy(),4*n2,"input scan, bulk read/write, result zeroing");
    Cost(()=>_=left.Scale(default),5*n2,"scale full branches");
    Cost(()=>_=left+right,6*n2,"addition");Cost(()=>_=left*right,5*n3+4*n2,"multiplication");
    Cost(()=>_=left.Trace(),n2+n,"trace");Cost(()=>_=Matrix.TraceProduct(left,right),6*n2,"trace product full branches");
    Cost(()=>_=left.Same(right),4*n2,"equality");Cost(()=>_=left.Symmetric,3*n2,"symmetry");Cost(()=>_=left.Nonzero,2*n2,"nonzero");
    Cost(()=>_=left.Text(),4*n2+2*n,"text input, result slots and references");
    Cost(()=>_=Geometry.Solve(left,new Rational[n]),2*n2+2*n,"solve input pairs and result slots");
   }
  });
  Test("matrix output zero-initialization budget refuses before backing array",()=>
  {using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=3}});Refuses(()=>_=new Matrix(2));Need(s.Snapshot().Poisoned&&s.Snapshot().RationalConstructions==0,"zeroing admitted before Data allocation, not before CLR header");});
  Test("MetricJet body metadata agrees with independent integer-only element census",()=>
  {foreach(int n in new[]{10,11,14,32,100})Need(MetricJetAdmission.BodyVisits(n)==EnumerateMetricJetBody(n),"all basis entries counted n="+n);Need(MetricJetAdmission.BodyVisits(10)==222160&&MetricJetAdmission.BodyVisits(11)==222166,"no truncation to first ten for selector arrays");Need(MetricJetAdmission.BodyVisits(int.MaxValue)==222100+6L*int.MaxValue,"bounded long arithmetic, no int wrap");});
  Test("MetricJet body reservation performs no coefficients matrices or arrays",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);MetricJetAdmission.ReserveBody(10);var r=s.Snapshot();Need(r.Matrices.SlotVisits==222160&&r.Matrices.Objects==0&&r.Matrices.ArrayObjects==0&&r.RationalConstructions==0&&r.IntegerOperations==0,"metadata reservation only, excludes property initializer charges");});
  Test("MetricJet old loop-unit budget and one-short source budget fail closed",()=>
  {foreach(long cap in new[]{22120L,222159L}){using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=cap}});Refuses(()=>MetricJetAdmission.ReserveBody(10));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"before a body allocation or coefficient");}});
  Test("MetricJet body domain failure poisons and exact budgets cannot be reused",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){bool failed=false;try{MetricJetAdmission.ReserveBody(9);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.SlotVisits==0,"invalid generic basis domain");}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=222160}})){MetricJetAdmission.ReserveBody(10);Refuses(()=>MetricJetAdmission.ReserveBody(10));Need(s.Snapshot().Poisoned,"cumulative visits");}});
  Test("declared array reservations cover zeroing and initial stores before allocation",()=>
  {using(var s=MixedProducerAdmission.Enter(Generous)){MixedProducerStages.Arrays(3);Need(s.Snapshot().Matrices.ArraySlots==3&&s.Snapshot().Matrices.SlotVisits==6&&s.Snapshot().Matrices.Objects==0,"metadata-only array envelope");}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=5}})){bool reached=false;Refuses(()=>{MixedProducerStages.Arrays(3);reached=true;_=new int[3];});Need(s.Snapshot().Poisoned&&!reached,"before caller allocation");}});
  Test("Bits reservations admit final result initialization without running the library",()=>
  {using var s=MixedProducerAdmission.Enter(Generous);MixedProducerStages.Bits(2);var r=s.Snapshot();Need(r.Matrices.SlotVisits==56&&r.Matrices.ArraySlots==112&&r.Matrices.ArrayObjects==10,"known results versus separate legacy growth slack");Need(r.RationalConstructions==0&&r.Matrices.Objects==0,"no source work");});
  Test("Motion source-access reservation includes full index membership and repeated matrix reads",()=>
  {
   Matrix m;FT one,two;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);one=Fourier.One(1,0,1);two=Fourier.Add(one,Fourier.One(2,0,1));}
   foreach(var t in new[]{new FT(),one,two})foreach(bool oracle in new[]{false,true})
   {using var s=MixedProducerAdmission.Enter(Generous);MixedProducerStages.Motion(m,t,oracle);long expected=196+28L*t.Count+(17+(oracle?239:0))*196L*t.Count;var r=s.Snapshot();Need(r.Matrices.SlotVisits==expected&&r.Matrices.Objects==0&&r.Tensors.Dictionaries==0&&r.RationalConstructions==0,"reservation before output and no coefficient evaluation");}
  });
  Test("oracle clone census includes initialization copy replacement inversion and aggregation",()=>
  {long touches=0;for(int i=0;i<14;i++)touches+=3;touches++;for(int i=0;i<14;i++)for(int j=i+1;j<14;j++)touches+=2;for(int i=0;i<14;i++)touches++;Need(touches==239,"independent full fourteen-slot loop census");Matrix m;FT t;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);t=Fourier.One(1,0,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=196+28+(17+touches)*196-1}})){Refuses(()=>MixedProducerStages.Motion(m,t,true));Need(s.Snapshot().Poisoned&&s.Snapshot().Tensors.Dictionaries==0,"oracle work refused before output");}});
  Test("Mixed metric transpose counts read write and output zeroes with larger borrowed inputs",()=>
  {foreach(int n in new[]{14,15}){Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(n);m[0,7]=2;}using(var s=MixedProducerAdmission.Enter(Generous)){var r=Mixed.MetricTranspose(m);Need(s.Snapshot().Matrices.SlotVisits==n*n+392+196&&r.N==14,"full borrowed validation and fixed output");Value(r[7,0],-2,1);}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=n*n+391}})){Refuses(()=>_=Mixed.MetricTranspose(m));Need(s.Snapshot().Matrices.Objects==0,"read/write quota before result");}}});
  Test("WedgeCoordinate reserves matrix rereads derivative reads and declared params before output",()=>
  {
   Matrix zero,nonzero;FT[] derivative;using(var s=MixedProducerAdmission.Enter(Generous)){zero=new Matrix(14);nonzero=new Matrix(14);nonzero[0,0]=2;derivative=Enumerable.Range(0,14).Select(_=>new FT()).ToArray();derivative[0]=Fourier.One(2,0,3);}
   using(var s=MixedProducerAdmission.Enter(Generous)){var r=Mixed.WedgeCoordinate(derivative,zero);Need(r.Count==0&&s.Snapshot().Matrices.SlotVisits==196+588+784,"all branches reserved despite zero map");}
   using(var s=MixedProducerAdmission.Enter(Generous)){var r=Mixed.WedgeCoordinate(derivative,nonzero);Need(r.Count==1,"manufactured one wedge term");Value(r[(3,0,0,0)].Real,6,1);}
   using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=1567}})){Refuses(()=>_=Mixed.WedgeCoordinate(derivative,zero));Need(s.Snapshot().Tensors.Dictionaries==0,"before even empty result");}
  });
  Test("Pullback row reserves Bits read and both conditional matrix reads before row allocation",()=>
  {Matrix m;FT t;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);t=Fourier.One(1,0,1);}foreach(bool oracle in new[]{false,true}){using(var s=MixedProducerAdmission.Enter(Generous)){var r=Mixed.Pullback(m,t,oracle);Need(r.Count==0&&s.Snapshot().Matrices.SlotVisits==196+28+2+29+1568,"source body plus bounded MatrixArg; serializer remains separate");}using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=254}})){Refuses(()=>_=Mixed.Pullback(m,t,oracle));Need(s.Snapshot().Tensors.Dictionaries==1,"only existing constant-form seed, no row allocated");}}});
  Test("TwoFormSlices complete grid trace and record accesses agree with integer-only census",()=>
  {
   FT one,two;using(var s=MixedProducerAdmission.Enter(Generous)){one=Fourier.One(3,0,1);two=Fourier.Add(one,Fourier.One(5,0,2));}
   foreach(var t in new[]{new FT(),one,two})
   {
    long expected=0;for(int i=0;i<14;i++)for(int j=0;j<14;j++)expected++; // zero grid
    for(int i=0;i<14;i++)for(int j=i;j<14;j++)expected+=3; // mirrored write/read/write
    for(int i=0;i<14;i++)for(int j=i+1;j<14;j++)expected+=3; // grid read+trace input zero/store
    for(int q=0;q<t.Count;q++)expected+=28+3; // Bits output +two indexes+grid read
    using var s=MixedProducerAdmission.Enter(Generous);var slices=Mixed.TwoFormSlices(t);Need(s.Snapshot().Matrices.SlotVisits==expected&&expected==784+31*t.Count,"complete known source-array census");Need(ReferenceEquals(slices[0,1],slices[1,0]),"mirrored alias preserved");
   }
  });
  Test("MatrixArg empty wire omits unused capacity even without active trace",()=>
  {
   Matrix m;using(var s=MixedProducerAdmission.Enter(Generous))m=new Matrix(14);
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    Need(MixedTrace.Active is null,"inactive recorder");
    Need(JsonSerializer.Serialize(MixedTrace.MatrixArg(m),MixedTrace.JsonOptions)=="{\"matrix\":[]}","no trailing nulls");
    var r=s.Snapshot();Need(r.Matrices.ArraySlots==392&&r.Matrices.ArrayObjects==2&&r.Matrices.SlotVisits==1568&&r.Tensors.MetadataObjects==197,"full prospective envelope on zero path");
    Need(r.FormatCharacters==0&&r.Matrices.Objects==0,"no zero formatting or new Matrix");
   }
  });
  Test("MatrixArg sparse wire retains exact row-major values and captured strings",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(14);m[13,0]=-3;m[0,13]=new Rational(2,3);m[0,1]=7;
   var arg=MixedTrace.MatrixArg(m);m[0,1]=99;
   Need(JsonSerializer.Serialize(arg,MixedTrace.JsonOptions)=="{\"matrix\":[{\"row\":0,\"column\":1,\"value\":\"7\"},{\"row\":0,\"column\":13,\"value\":\"2/3\"},{\"row\":13,\"column\":0,\"value\":\"-3\"}]}","property order, sparse row order, exact captured values");
  });
  Test("MatrixArg full wire matches historical List builder byte for byte",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var m=new Matrix(14);
   for(int row=0;row<14;row++)for(int column=0;column<14;column++)m[row,column]=row*14+column+1;
   var legacy=new List<object>();for(int row=0;row<14;row++)for(int column=0;column<14;column++)if(m[row,column]!=0)legacy.Add(new{row,column,value=m[row,column].ToString()});
   string wire=JsonSerializer.Serialize(MixedTrace.MatrixArg(m),MixedTrace.JsonOptions);
   Need(wire==JsonSerializer.Serialize(new{matrix=legacy.ToArray()},MixedTrace.JsonOptions),"dense historical wire");
   using var document=JsonDocument.Parse(wire);Need(document.RootElement.GetProperty("matrix").GetArrayLength()==196,"all entries exactly once");
  });
  Test("MatrixArg buffer header metadata and visit quotas refuse before formatting",()=>
  {
   Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);m[0,0]=1;}
   foreach(var limits in new[]{Generous with{Matrices=Generous.Matrices with{ArraySlots=391}},Generous with{Matrices=Generous.Matrices with{ArrayObjects=1}},Generous with{Tensors=Generous.Tensors with{MetadataObjects=196}},Generous with{Matrices=Generous.Matrices with{SlotVisits=1567}}})
   {using var s=MixedProducerAdmission.Enter(limits);Refuses(()=>_=MixedTrace.MatrixArg(m));var r=s.Snapshot();Need(r.Poisoned&&r.FormatCharacters==0&&r.RationalConstructions==0&&r.IntegerOperations==0,"reservation before builder and any coefficient work");}
  });
  Test("MatrixArg mandatory scope and invalid dimensions fail closed",()=>
  {
   Matrix valid,invalid;using(var s=MixedProducerAdmission.Enter(Generous)){valid=new Matrix(14);invalid=new Matrix(15);}
   Refuses(()=>_=MixedTrace.MatrixArg(valid));
   foreach(var m in new[]{invalid,null!})
   {using var s=MixedProducerAdmission.Enter(Generous);bool failed=false;try{_=MixedTrace.MatrixArg(m);}catch(MixedTrace.EvidenceFailure){failed=true;}var r=s.Snapshot();Need(failed&&r.Poisoned&&r.Matrices.ArrayObjects==0&&r.FormatCharacters==0,"shape before reservation and allocation");}
  });
  Test("MatrixArg validates late borrowed height before allocating or formatting early entries",()=>
  {
   Matrix m;using(var s=MixedProducerAdmission.Enter(Generous)){m=new Matrix(14);m[0,0]=1;m[13,13]=new Rational(BigInteger.One<<100,BigInteger.One);}
   using(var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64}))
   {Refuses(()=>_=MixedTrace.MatrixArg(m));Need(s.Snapshot().Poisoned&&s.Snapshot().FormatCharacters==0&&s.Snapshot().RationalConstructions==0,"complete pre-scan before early entry");}
  });
  Test("MatrixArg cumulative exact envelope cannot be reused",()=>
  {
   Matrix m;using(var s=MixedProducerAdmission.Enter(Generous))m=new Matrix(14);
   using(var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{ArraySlots=392,ArrayObjects=2,SlotVisits=1568},Tensors=Generous.Tensors with{MetadataObjects=197}}))
   {_=MixedTrace.MatrixArg(m);Refuses(()=>_=MixedTrace.MatrixArg(m));Need(s.Snapshot().Poisoned,"cumulative admission, not reusable per-call allowance");}
  });
  Test("MatrixArg source-touch envelope matches independent full builder census",()=>
  {
   long touches=0;for(int i=0;i<196;i++)touches++; // validation scan
   for(int i=0;i<196;i++)touches++; // fixed buffer zeroing
   for(int row=0;row<14;row++)for(int column=0;column<14;column++)touches+=3; // condition, value, store
   for(int i=0;i<196;i++)touches+=3; // result zeroing, copy read/write
   Need(touches==1568,"independent dense maximum");
   Matrix m;using(var s=MixedProducerAdmission.Enter(Generous))m=new Matrix(14);
   using(var s=MixedProducerAdmission.Enter(Generous)){MixedProducerStages.MatrixArgument(m);Need(s.Snapshot().Matrices.SlotVisits==touches&&s.Snapshot().FormatCharacters==0,"reservation and input validation without builder");}
  });
  Test("fingerprint explicit sort preserves empty singleton and every grade wire and hash",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var random=new Random(627);
   foreach(int degree in Enumerable.Range(0,15))foreach(int count in new[]{0,1,2,3,7,16,33})
   {
    var t=new FT();var keys=Enumerable.Range(0,count).OrderBy(_=>random.Next()).ToArray();
    foreach(int k in keys)t.Add(((1<<degree)-1,k,0,0),new Scalar(new Rational(k%2==0?k+1:-k-1,k+2),new Rational(k%3-1)));
    byte[] expected=TensorWire(t,degree,true),actual=TensorWire(t,degree);Need(actual.SequenceEqual(expected),"historical wire grade="+degree+" count="+count);
    var fingerprint=MixedTrace.Fingerprint(t,degree,WireLimits);
    Need(fingerprint.Bytes==expected.Length&&fingerprint.Sha256==Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant(),"canonical fingerprint");
   }
  });
  Test("fingerprint sorts by form before blade and ignores dictionary insertion order",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var t=new FT();
   foreach(var key in new[]{(8192,0,0,0),(2,5,0,0),(1,10,0,0),(2,1,0,0),(1,2,0,0)})t.Add(key,new Scalar(1));
   byte[] expected=TensorWire(t,1,true);Need(TensorWire(t,1).SequenceEqual(expected),"lexicographic keys not insertion or blade-first");
   var reverse=new FT();foreach(var q in t.Reverse())reverse.Add(q.Key,q.Value);
   Need(MixedTrace.Fingerprint(t,1,WireLimits)==MixedTrace.Fingerprint(reverse,1,WireLimits),"insertion-invariant digest");
  });
  Test("fingerprint preserves culture formatting and exact per-record decimal ceiling",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var t=new FT(){[(1,0,0,0)]=new Scalar(new Rational(-2,3),0)};
   var original=CultureInfo.CurrentCulture;try
   {
    var culture=(CultureInfo)CultureInfo.InvariantCulture.Clone();culture.NumberFormat.NegativeSign="minus";CultureInfo.CurrentCulture=culture;
    Need(TensorWire(t,1).SequenceEqual(TensorWire(t,1,true)),"source culture behavior");
   }finally{CultureInfo.CurrentCulture=original;}
   // Bit bounds for9 exceed one character; do not apply that loose bound as
   // a stricter wire-domain ceiling. The exact historical length still wins.
   var one=new FT(){[(1,0,0,0)]=new Scalar(9)};
   Need(TensorWire(one,1,limits:WireLimits with{RationalCharacters=1}).SequenceEqual(TensorWire(one,1,true)),"no domain narrowing at decimal boundary");
  });
  Test("fingerprint sorting quotas reject before touching a borrowed writer",()=>
  {
   FT t;using(var s=MixedProducerAdmission.Enter(Generous))t=new FT(){[(1,2,0,0)]=new Scalar(2),[(1,1,0,0)]=new Scalar(3),[(1,0,0,0)]=new Scalar(4)};
   var costs=MixedProducerStages.FingerprintSortCosts(3);
   foreach(var limits in new[]{Generous with{Matrices=Generous.Matrices with{ArraySlots=2}},Generous with{Matrices=Generous.Matrices with{SlotVisits=costs.ArrayTouches-1}},Generous with{Tensors=Generous.Tensors with{SortWork=costs.ComponentComparisons-1}},Generous with{Tensors=Generous.Tensors with{SequenceSlots=2}},Generous with{Tensors=Generous.Tensors with{TemporarySlots=3}},Generous with{Tensors=Generous.Tensors with{RecordVisits=5}}})
   {
    using var s=MixedProducerAdmission.Enter(limits);using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);
    Refuses(()=>MixedTrace.WriteTensor(writer,t,1,WireLimits));var r=s.Snapshot();
    Need(r.Poisoned&&r.FormatCharacters==0&&r.RationalConstructions==0&&r.IntegerOperations==0&&writer.BytesPending==0&&stream.Length==0,"no coefficient work, formatted string or writer output before quota");
   }
  });
  Test("fingerprint complete formatting preview precedes sort and writer work",()=>
  {
   FT t;using(var s=MixedProducerAdmission.Enter(Generous))t=new FT(){[(1,0,0,0)]=new Scalar(2),[(1,1,0,0)]=new Scalar(3),[(1,2,0,0)]=new Scalar(4)};
   long characters=0;using(var s=MixedProducerAdmission.Enter(Generous))
   {foreach(var q in t){characters+=s.FormattingCharacters(q.Value.Real.Numerator,q.Value.Real.Denominator,true);characters+=s.FormattingCharacters(q.Value.Imaginary.Numerator,q.Value.Imaginary.Denominator,true);}Need(s.Snapshot().FormatCharacters==0,"preview does not consume credit");}
   using(var s=MixedProducerAdmission.Enter(Generous with{FormatCharacters=characters-1}))
   {using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);Refuses(()=>MixedTrace.WriteTensor(writer,t,1,WireLimits));Need(s.Snapshot().FormatCharacters==0&&writer.BytesPending==0&&stream.Length==0,"aggregate failure before even the first record string");}
   using(var s=MixedProducerAdmission.Enter(Generous with{FormatCharacters=characters}))
   {_=TensorWire(t,1);Need(s.Snapshot().FormatCharacters==characters,"ToString retains the actual cumulative charge");Refuses(()=>_=TensorWire(t,1));}
  });
  Test("fingerprint known owned headers digest array and hex strings are admitted first",()=>
  {
   var t=new FT();foreach(var limits in new[]{Generous with{Tensors=Generous.Tensors with{MetadataObjects=4}},Generous with{Matrices=Generous.Matrices with{ArrayObjects=1}},Generous with{Matrices=Generous.Matrices with{ArraySlots=31}},Generous with{FormatCharacters=127}})
   {using var s=MixedProducerAdmission.Enter(limits);Refuses(()=>_=MixedTrace.Fingerprint(t,-1,WireLimits));Need(s.Snapshot().Poisoned&&s.Snapshot().FormatCharacters==0&&s.Snapshot().IntegerOperations==0,"before known hash/writer/hex setup");}
   using(var s=MixedProducerAdmission.Enter(Generous))
   {var result=MixedTrace.Fingerprint(t,-1,WireLimits);var r=s.Snapshot();Need(result.Bytes==2&&r.Matrices.ArrayObjects==2&&r.Matrices.ArraySlots==32&&r.Tensors.MetadataObjects==5&&r.FormatCharacters==128,"empty output still admits named wrappers and digest strings");}
  });
  Test("fingerprint validates late borrowed height and malformed records before output",()=>
  {
   FT tooTall;using(var s=MixedProducerAdmission.Enter(Generous))tooTall=new FT(){[(1,0,0,0)]=new Scalar(1),[(1,1,0,0)]=new Scalar(new Rational(BigInteger.One<<100,BigInteger.One),0)};
   using(var s=MixedProducerAdmission.Enter(Generous with{IntegerBits=64}))
   {using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);Refuses(()=>MixedTrace.WriteTensor(writer,tooTall,1,WireLimits));Need(s.Snapshot().FormatCharacters==0&&writer.BytesPending==0&&stream.Length==0,"late height precedes early serialization");}
   foreach(var key in new[]{(-1,0,0,0),(1,16384,0,0),(1,0,1,0),(3,0,0,0)})
   {
    using var s=MixedProducerAdmission.Enter(Generous);var bad=new FT(){[(1,2,0,0)]=new Scalar(1),[key]=new Scalar(1)};
    using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);bool failed=false;
    try{MixedTrace.WriteTensor(writer,bad,1,WireLimits);}catch(MixedTrace.EvidenceFailure){failed=true;}
    Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().FormatCharacters==0&&writer.BytesPending==0&&stream.Length==0,"late record domain checked before output");
   }
  });
  Test("fingerprint mandatory scope also covers empty borrowed and owned entry points",()=>
  {
   var t=new FT();Refuses(()=>_=MixedTrace.Fingerprint(t,-1,WireLimits));
   using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);Refuses(()=>MixedTrace.WriteTensor(writer,t,-1,WireLimits));Need(writer.BytesPending==0,"empty no-scope refusal");
  });
  Test("fingerprint sort costs cover independent worst-path loop census and checked domain",()=>
  {
   foreach(int n in new[]{0,1,2,3,4,7,8,31,32,100,int.MaxValue})
   {
    long calls=(long)n/2+Math.Max((long)n-1,0),depth=0;for(long width=n;width>1;width/=2)depth++;
    // Each call can descend at most floor(log2(n)) levels. This is a tighter
    // independent census than the admitted2n * bit-length(n) envelope.
    long comparisons=calls*depth*8,touches=3L*n+4*Math.Max((long)n-1,0)+calls*depth*8;
    var costs=MixedProducerStages.FingerprintSortCosts(n);Need(costs.ArrayTouches>=touches&&costs.ComponentComparisons>=comparisons,"all heap levels and extraction swaps n="+n);
   }
   Need(MixedProducerStages.FingerprintSortCosts(0)==(0,0)&&MixedProducerStages.FingerprintSortCosts(1)==(3,0),"empty/singleton no sift");
   bool failed=false;try{_=MixedProducerStages.FingerprintSortCosts(-1);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed,"negative domain");
  });
  Test("fingerprint exact byte cap and decimal rejection stay fail closed",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var t=new FT(){[(1,0,0,0)]=new Scalar(12)};byte[] expected=TensorWire(t,1,true);
   Need(MixedTrace.Fingerprint(t,1,WireLimits with{TensorBytes=expected.Length}).Bytes==expected.Length,"exact byte boundary");
   bool bytes=false,characters=false;try{_=MixedTrace.Fingerprint(t,1,WireLimits with{TensorBytes=expected.Length-1});}catch(MixedTrace.EvidenceFailure){bytes=true;}
   try{_=MixedTrace.Fingerprint(t,1,WireLimits with{RationalCharacters=1});}catch(MixedTrace.EvidenceFailure){characters=true;}
   Need(bytes&&characters,"original exact output limits remain enforced");
  });
  Test("explicit admitted fingerprint has mandatory scope even for empty input",()=>
  {Refuses(()=>_=MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits,_=>throw new Exception("unexpected byte callback")));});
  Test("explicit admitted fingerprint canonical hash, insertion order and exact primitive counts",()=>
  {
   FT t,reverse;byte[] expected;
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    t=new FT(){[(2,5,0,0)]=new Scalar(new Rational(-2,3),0),[(1,2,0,0)]=new Scalar(9)};
    reverse=new FT();foreach(var q in t.Reverse())reverse.Add(q.Key,q.Value);expected=TensorWire(t,1,true);
   }
   MixedProducerAdmission.Report legacy;using(var s=MixedProducerAdmission.Enter(Generous))
   {_=MixedTrace.Fingerprint(t,1,WireLimits);legacy=s.Snapshot();}
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    long charged=0;int calls=0;var result=MixedTrace.FingerprintAdmitted(t,1,WireLimits,n=>{Need(n>0,"positive chunk");charged+=n;calls++;});
    Need(result.Bytes==expected.Length&&charged==result.Bytes&&calls>0&&result.Sha256==Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant(),"same canonical bytes, no LF, every chunk charged");
    Need(s.Snapshot()==legacy,"shared accounting, no double guarded formatting charge");
    Need(MixedTrace.FingerprintAdmitted(reverse,1,WireLimits,_=>{})==result,"insertion-independent digest");
   }
   foreach(int degree in new[]{0,14})using(var s=MixedProducerAdmission.Enter(Generous))
   {var value=new FT(){[((1<<degree)-1,16383,0,0)]=new Scalar(1)};Need(MixedTrace.FingerprintAdmitted(value,degree,WireLimits,_=>{})==MixedTrace.Fingerprint(value,degree,WireLimits),"inclusive degree and blade bounds");}
  });
  Test("explicit admitted fingerprint rejects nulls invalid degrees and records before byte callback",()=>
  {
   FT zero,mode,grade;using(var s=MixedProducerAdmission.Enter(Generous))
   {zero=new FT(){[(1,0,0,0)]=default};mode=new FT(){[(1,0,0,1)]=new Scalar(1)};grade=new FT(){[(3,0,0,0)]=new Scalar(1)};}
   foreach(int which in Enumerable.Range(0,8))
   {
    using var s=MixedProducerAdmission.Enter(Generous);bool failed=false;int calls=0;Action<int> charge=_=>calls++;
    try
    {
     _=which switch
     {
      0=>MixedTrace.FingerprintAdmitted(null!,0,WireLimits,charge),
      1=>MixedTrace.FingerprintAdmitted(new FT(),0,null!,charge),
      2=>MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits,null!),
      3=>MixedTrace.FingerprintAdmitted(new FT(),-1,WireLimits,charge),
      4=>MixedTrace.FingerprintAdmitted(new FT(),15,WireLimits,charge),
      _=>MixedTrace.FingerprintAdmitted(which==5?zero:which==6?mode:grade,1,WireLimits,charge)
     };
    }catch(Exception ex)when(ex is ArgumentNullException or MixedTrace.EvidenceFailure){failed=true;}
    Need(failed&&calls==0&&s.Snapshot().Poisoned,"input failure poisons before append");
   }
  });
  Test("explicit admitted fingerprint callback throws or swallows quota failure fail closed",()=>
  {
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    bool caught=false;int calls=0;var marker=new InvalidOperationException("byte charge refused");
    try{_=MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits,_=>{calls++;throw marker;});}catch(InvalidOperationException ex){caught=ReferenceEquals(ex,marker);}
    Need(caught&&calls==1&&s.Snapshot().Poisoned,"callback exception preserved and scope poisoned");
   }
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    int calls=0;Refuses(()=>_=MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits,_=>{calls++;Refuses(()=>s.TextCharacters(Generous.FormatCharacters));}));
    Need(calls==1&&s.Snapshot().Poisoned,"post-callback health rejects swallowed refusal");
   }
   using(var s=MixedProducerAdmission.Enter(Generous))
   {
    int calls=0;EvidenceRefuses(()=>_=MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits with{TensorBytes=1},_=>calls++));
    Need(calls==0&&s.Snapshot().Poisoned,"local byte cap precedes callback and append");
   }
  });
  Test("explicit admitted fingerprint borrowed bit and cumulative format previews precede output",()=>
  {
   FT t;using(var s=MixedProducerAdmission.Enter(Generous))t=new FT(){[(1,0,0,0)]=new Scalar(1),[(1,1,0,0)]=new Scalar(new Rational(BigInteger.One<<100,1),0)};
   foreach(var limits in new[]{Generous with{IntegerBits=64},Generous with{FormatCharacters=129},Generous with{Matrices=Generous.Matrices with{ArraySlots=33}}})
   {
    using var s=MixedProducerAdmission.Enter(limits);int calls=0;Refuses(()=>_=MixedTrace.FingerprintAdmitted(t,1,WireLimits,_=>calls++));
    Need(calls==0&&s.Snapshot().Poisoned&&s.Snapshot().FormatCharacters<=128&&s.Snapshot().IntegerOperations==0,"no rational formatting or output on late borrowed height/aggregate format/array refusal");
   }
  });
  Test("explicit admitted fingerprint refuses noncanonical ambient fractional sign",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var original=CultureInfo.CurrentCulture;
   try
   {
    var culture=(CultureInfo)CultureInfo.InvariantCulture.Clone();culture.NumberFormat.NegativeSign="minus";CultureInfo.CurrentCulture=culture;
    int calls=0;EvidenceRefuses(()=>_=MixedTrace.FingerprintAdmitted(new FT(),0,WireLimits,_=>calls++));
    Need(calls==0&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArrayObjects==0,"canonical sign refusal before preparation");
   }finally{CultureInfo.CurrentCulture=original;}
  });
  Test("full recorder capacity rejects getters and validators before preparation",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);int getters=0,callbacks=0;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=1},(_,_,_)=>{},(_,_)=>callbacks++);
   trace.Record("zero",new FT(),[]);int before=callbacks;var admitted=s.Snapshot();
   EvidenceRefuses(()=>trace.Record("zero",new FT(),[],new ParameterProbe(()=>getters++)));
   EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(getters==0&&callbacks==before&&trace.Snapshot().Nodes.Length==1,"full capacity before parameter getter/validator or append");
   Need(s.Snapshot()==admitted,"no producer preparation work at full capacity");
  });
  Test("one-short node capacity cannot append any implicit empty operands",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);int callbacks=0,getters=0;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=2},(_,_,_)=>{},(_,_)=>callbacks++);
   var a=new FT();var b=new FT();
   EvidenceRefuses(()=>trace.Record("sum",new FT(),[a,a,b],new ParameterProbe(()=>getters++)));
   Need(trace.Snapshot().Nodes.Length==0&&getters==0&&callbacks==0&&s.Snapshot().FormatCharacters==0,"all distinct implicit nodes and requested node preflighted together");
  });
  Test("exact node capacity preserves distinct empty identities and repeated aliases",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);int callbacks=0;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=3},(_,_,_)=>{},(_,_)=>callbacks++);
   var a=new FT();var b=new FT();trace.Record("sum",new FT(),[a,a,b,a]);var graph=trace.Finish();
   Need(graph.Nodes.Select(n=>n.Op).SequenceEqual(new[]{"zero","zero","sum"})&&graph.Nodes[2].Inputs.SequenceEqual(new[]{0,0,1,0}),"first-occurrence zero order and reference aliases");
   Need(callbacks==1&&graph.Nodes.All(n=>n.Records==0),"implicit zeros remain internal and empty");
  });
  Test("recorded empty inputs consume no extra implicit node capacity",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=2},(_,_,_)=>{},(_,_)=>{});
   var a=new FT();trace.Record("zero",a,[]);trace.Record("sum",new FT(),[a,a]);var graph=trace.Finish();
   Need(graph.Nodes.Length==2&&graph.Nodes[1].Inputs.SequenceEqual(new[]{0,0}),"registered identity reused at exact ceiling");
  });
  Test("late unregistered nonzero operand is rejected before earlier zero expansion",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   var nonzero=new FT(){[(1,0,0,0)]=new Scalar(1)};
   EvidenceRefuses(()=>trace.Record("sum",new FT(),[new FT(),nonzero]));
   Need(trace.Snapshot().Nodes.Length==0&&s.Snapshot().FormatCharacters==0,"operand validation does not partially mutate identities");
  });
  Test("leaf node capacity is checked before registration mutation and hashing",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);string hash=Convert.ToHexString(SHA256.HashData("[]"u8)).ToLowerInvariant();
   using var trace=new MixedTrace.Session(CaptureCaps,[new("leaf",1,"manufactured",hash)],WireLimits with{Nodes=1},(_,_,_)=>{},(_,_)=>{});
   trace.Record("zero",new FT(),[]);var before=s.Snapshot();
   EvidenceRefuses(()=>trace.RegisterLeaf("leaf",1,new FT()));Need(s.Snapshot()==before&&trace.Snapshot().Nodes.Length==1,"no early leaf hashing or admission work");
   EvidenceRefuses(()=>trace.Finish()); // Refused leaf cannot satisfy registration census.
  });
  Test("full nodes permit marking an existing identity but not creating a new one",()=>
  {
   foreach(bool existing in new[]{false,true})
   {
   using var s=MixedProducerAdmission.Enter(Generous);int expanded=0;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=1},(_,_,_)=>expanded++,(_,_)=>{});
   var known=new FT();trace.Record("zero",known,[]);var before=s.Snapshot();
   if(!existing)
   {EvidenceRefuses(()=>trace.Mark("unknown",1,new FT(),true));Need(expanded==0&&trace.Snapshot().Marks.Length==0&&s.Snapshot()==before,"unknown mark refused before fingerprint or sink");EvidenceRefuses(()=>trace.Finish());}
   else
   {trace.Mark("known",1,known,true);var graph=trace.Finish();Need(expanded==1&&graph.Nodes.Length==1&&graph.Marks.Single().Node==0,"healthy known mark remains valid at node ceiling");}
   }
  });
  Test("recorder scan and explicit array quotas precede implicit node work",()=>
  {
   foreach(long visits in new[]{2L,16L})
   {
    using var s=MixedProducerAdmission.Enter(Generous with{Matrices=Generous.Matrices with{SlotVisits=visits}});int callbacks=0;
    using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=3},(_,_,_)=>{},(_,_)=>callbacks++);
    Refuses(()=>trace.Record("sum",new FT(),[new FT(),new FT()]));
    Need(s.Snapshot().Poisoned&&s.Snapshot().FormatCharacters==0&&callbacks==0&&trace.Snapshot().Nodes.Length==0,"quadratic alias scan and two arrays reserved before any implicit zero");
   }
  });
  Test("result alias still needs its requested node beyond an implicit zero",()=>
  {
   foreach(int capacity in new[]{1,2})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits with{Nodes=capacity},(_,_,_)=>{},(_,_)=>{});var a=new FT();
    if(capacity==1){EvidenceRefuses(()=>trace.Record("sum",a,[a]));Need(trace.Snapshot().Nodes.Length==0,"result alias is not a free requested node");}
    else{trace.Record("sum",a,[a]);trace.Mark("aliased",1,a,false);var graph=trace.Finish();Need(graph.Nodes.Select(n=>n.Op).SequenceEqual(new[]{"zero","sum"})&&graph.Nodes[1].Inputs.Single()==0&&graph.Marks.Single().Node==1,"requested node replaces identity only after source node capture");}
   }
  });
  Test("recorder array and alias-scan envelopes match source census without allocation",()=>
  {
   foreach(int n in new[]{0,1,2,3,14,100})
   {
    long visits=0;for(int i=0;i<n;i++){visits++;for(int j=0;j<i;j++)visits++;}
    for(int i=0;i<n;i++)visits+=3+4; // source zero/read/store and degree zero/ID read/node read/store
    using var s=MixedProducerAdmission.Enter(Generous);MixedProducerStages.RecorderOperandScan(n);MixedProducerStages.RecorderOperandArrays(n);var r=s.Snapshot();
    Need(r.Matrices.SlotVisits==visits&&r.Matrices.ArrayObjects==2&&r.Matrices.ArraySlots==2*n,"explicit array/source-index census");
    Need(r.FormatCharacters==0&&r.RationalConstructions==0&&r.Tensors.Dictionaries==0,"metadata only");
   }
   using(var s=MixedProducerAdmission.Enter(Generous)){Refuses(()=>MixedProducerStages.RecorderOperandScan(int.MaxValue));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArrayObjects==0,"large prospective scan rejects without wrap or allocation");}
  });
  Test("swallowed validator reentry cannot append either nested or outer node",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);MixedTrace.Session? trace=null;int callbacks=0;bool innerRejected=false;
   using var owned=trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>
   {if(callbacks++==0){try{trace!.Record("zero",new FT(),[]);}catch(MixedTrace.EvidenceFailure){innerRejected=true;}}});
   EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(innerRejected&&callbacks==1&&trace.Snapshot().Nodes.Length==0,"swallowed nested mutation leaves no success path");EvidenceRefuses(()=>trace.Finish());
  });
  Test("getters validators and expanded sinks reject every public reentrant mutator",()=>
  {
   foreach(string origin in new[]{"getter","validator","expanded"})foreach(string mutation in new[]{"record","leaf","mark","finish","dispose"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);MixedTrace.Session? trace=null;bool armed=false,rejected=false;var known=new FT();
    void Callback()
    {
     if(!armed)return;armed=false;
     try
     {
      switch(mutation)
      {
       case "record":trace!.Record("zero",new FT(),[]);break;
       case "leaf":trace!.RegisterLeaf("late",1,new FT());break;
       case "mark":trace!.Mark("nested",1,known,false);break;
       case "finish":_=trace!.Finish();break;
       case "dispose":trace!.Dispose();break;
      }
     }catch(MixedTrace.EvidenceFailure){rejected=true;}
    }
    using var owned=trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{if(origin=="expanded")Callback();},(_,_)=>{if(origin=="validator")Callback();});
    trace.Record("zero",known,[]);MixedTrace.Active=trace;armed=true;
    EvidenceRefuses(()=>
    {
     if(origin=="expanded")trace.Mark("outer",1,known,true);
     else if(origin=="getter")trace.Record("unit",new FT(),[],new UnitParameterProbe(Callback));
     else trace.Record("zero",new FT(),[]);
    });
    var snapshot=trace.Snapshot();Need(rejected&&snapshot.Nodes.Length==1&&snapshot.Marks.Length==0&&ReferenceEquals(MixedTrace.Active,trace),origin+"/"+mutation+": no nested mutation, no outer commit, active tripwire retained");
    EvidenceRefuses(()=>trace.Finish());EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   }
  });
  Test("rejected nested mutation cannot release the outer busy flag for disposal",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);MixedTrace.Session? trace=null;int rejected=0;
   using var owned=trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>
   {
    try{trace!.Record("zero",new FT(),[]);}catch(MixedTrace.EvidenceFailure){rejected++;}
    try{trace!.Dispose();}catch(MixedTrace.EvidenceFailure){rejected++;}
   });
   MixedTrace.Active=trace;EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(rejected==2&&ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==0,"nested finally cannot enable cleanup inside outer callback");
   trace.Dispose();trace.Dispose();Need(MixedTrace.Active is null,"ordinary post-failure cleanup stays idempotent");
  });
  Test("ordinary callback exceptions are terminal while partial evidence remains inspectable",()=>
  {
   foreach(string origin in new[]{"getter","validator","expanded"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);bool armed=false;int effects=0;var marker=new InvalidOperationException("manufactured callback failure");
    void Fail(){if(armed){effects++;throw marker;}}
    using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{if(origin=="expanded")Fail();},(_,_)=>{if(origin=="validator")Fail();});
    var known=new FT();trace.Record("zero",known,[]);armed=true;bool threw=false;
    try
    {
     if(origin=="expanded")trace.Mark("failed",1,known,true);
     else if(origin=="getter")trace.Record("unit",new FT(),[],new UnitParameterProbe(Fail));
     else trace.Record("sum",new FT(),[new FT()]);
    }catch(Exception ex){Need(ReferenceEquals(ex.GetBaseException(),marker),"original callback exception preserved");threw=true;}
    var snapshot=trace.Snapshot();Need(threw&&effects==1&&snapshot.Nodes.Length==(origin=="validator"?2:1)&&snapshot.Marks.Length==0,"partial nodes/effects are not rolled back or promoted");
    EvidenceRefuses(()=>trace.Finish());EvidenceRefuses(()=>trace.Mark("later",1,known,false));
   }
  });
  Test("failed leaf hash cannot turn its partial registration into a completed graph",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);
   using var trace=new MixedTrace.Session(CaptureCaps,[new("leaf",1,"manufactured",new string('0',64))],WireLimits,(_,_,_)=>{},(_,_)=>{});
   EvidenceRefuses(()=>trace.RegisterLeaf("leaf",1,new FT()));Need(trace.Snapshot().Nodes.Length==0&&trace.Snapshot().Leaves.Length==1,"declared but never accepted leaf remains diagnostic");
   EvidenceRefuses(()=>trace.Finish());EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
  });
  Test("failed Finish seals mutation but allows snapshot and unrelated active-session cleanup",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);string hash=Convert.ToHexString(SHA256.HashData("[]"u8)).ToLowerInvariant();
   using var failed=new MixedTrace.Session(CaptureCaps,[new("leaf",1,"manufactured",hash)],WireLimits,(_,_,_)=>{},(_,_)=>{});
   EvidenceRefuses(()=>failed.Finish());EvidenceRefuses(()=>failed.RegisterLeaf("leaf",1,new FT()));Need(failed.Snapshot().Nodes.Length==0,"cannot repair and promote after failed completion");
   using var healthy=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=healthy;
   failed.Dispose();failed.Dispose();Need(ReferenceEquals(MixedTrace.Active,healthy),"cleanup only clears its own active session");
   healthy.Record("zero",new FT(),[]);Need(healthy.Finish().Nodes.Length==1,"unrelated healthy session remains usable");
  });
  Test("producer quota failure also makes the session terminal without losing diagnostics",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous with{FormatCharacters=127});
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=trace;
   Refuses(()=>trace.Record("zero",new FT(),[]));Need(s.Snapshot().Poisoned&&trace.Snapshot().Nodes.Length==0,"scope failure keeps diagnostic graph available");
   EvidenceRefuses(()=>trace.Finish());trace.Dispose();Need(MixedTrace.Active is null,"failure cleanup does not need a healthy arithmetic scope");
  });
  Test("callback snapshots and finished graphs cannot mutate live input arrays or leaves",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);MixedTrace.Session? trace=null;bool inspect=false;int inspected=0;
   string hash=Convert.ToHexString(SHA256.HashData("[]"u8)).ToLowerInvariant();
   using var owned=trace=new MixedTrace.Session(CaptureCaps,[new("leaf",1,"manufactured",hash)],WireLimits,(_,_,_)=>{},(_,_)=>
   {
    if(!inspect)return;inspected++;var snapshot=trace!.Snapshot();
    snapshot.Nodes[1].Inputs[0]=999;snapshot.Leaves[0]=snapshot.Leaves[0] with{Id="changed"};snapshot.Marks[0]=snapshot.Marks[0] with{Name="changed"};
   });
   var a=new FT();var b=new FT();trace.RegisterLeaf("leaf",1,a);trace.Record("sum",b,[a]);trace.Mark("before",1,b,false);inspect=true;
   trace.Record("zero",new FT(),[]);var graph=trace.Finish();
   Need(inspected==1&&graph.Nodes[1].Inputs.Single()==0&&graph.Leaves.Single().Id=="leaf"&&graph.Marks.Single().Name=="before","legitimate read-only callback inspection is detached");
   graph.Nodes[1].Inputs[0]=888;graph.Leaves[0]=graph.Leaves[0] with{Id="changed-again"};
   var after=trace.Snapshot();Need(after.Nodes[1].Inputs.Single()==0&&after.Leaves.Single().Id=="leaf","successful graph result cannot rewrite diagnostic state");
  });
  Test("expanded callback can inspect a snapshot without reentering mutation",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);MixedTrace.Session? trace=null;int inspections=0;
   using var owned=trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>
   {var snapshot=trace!.Snapshot();Need(snapshot.Nodes.Length==1&&snapshot.Marks.Length==0,"mark not committed before expanded callback succeeds");inspections++;},(_,_)=>{});
   var value=new FT();trace.Record("zero",value,[]);trace.Mark("expanded",1,value,true);var graph=trace.Finish();
   Need(inspections==1&&graph.Marks.Length==1,"nonmutating inspection remains valid");
  });
  Test("swallowed primitive quota failure in expanded callback cannot commit a mark",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);bool swallowed=false;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>
   {try{s.MatrixVisits(long.MaxValue);}catch(MixedProducerAdmission.Refused){swallowed=true;}},(_,_)=>{});
   var value=new FT();trace.Record("zero",value,[]);Refuses(()=>trace.Mark("expanded",1,value,true));
   Need(swallowed&&s.Snapshot().Poisoned&&trace.Snapshot().Nodes.Length==1&&trace.Snapshot().Marks.Length==0,"scope health checked after callback even without Session reentry");EvidenceRefuses(()=>trace.Finish());
  });
  Test("scope failure at mutation entry permanently fails the session across fresh scopes",()=>
  {
   MixedTrace.Session trace;
   using(var s=MixedProducerAdmission.Enter(Generous))
   {trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});trace.Record("zero",new FT(),[]);s.Poison();Refuses(()=>trace.Finish());}
   using(trace)using(var fresh=MixedProducerAdmission.Enter(Generous))
   {EvidenceRefuses(()=>trace.Finish());Need(trace.Snapshot().Nodes.Length==1,"fresh arithmetic admission cannot revive a failed session");}
  });
  Test("callback cannot disable active tracing and then complete its outer node",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);bool rejected=false;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>
   {try{MixedTrace.Active=null;}catch(MixedTrace.EvidenceFailure){rejected=true;}});
   MixedTrace.Active=trace;EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(rejected&&ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==0,"active pointer protected before mutation");EvidenceRefuses(()=>trace.Finish());
  });
  Test("all callback origins refuse null self and replacement active-pointer writes",()=>
  {
   foreach(string origin in new[]{"getter","validator","expanded"})foreach(string target in new[]{"null","self","other"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var other=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
    MixedTrace.Session? trace=null;bool armed=false,rejected=false;
    void Change(){if(!armed)return;try{MixedTrace.Active=target=="null"?null:target=="self"?trace:other;}catch(MixedTrace.EvidenceFailure){rejected=true;}}
    using var owned=trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{if(origin=="expanded")Change();},(_,_)=>{if(origin=="validator")Change();});
    var known=new FT();trace.Record("zero",known,[]);MixedTrace.Active=trace;armed=true;
    EvidenceRefuses(()=>{if(origin=="expanded")trace.Mark("bad",1,known,true);else if(origin=="getter")trace.Record("unit",new FT(),[],new UnitParameterProbe(Change));else trace.Record("zero",new FT(),[]);});
    Need(rejected&&ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==1&&trace.Snapshot().Marks.Length==0,origin+"/"+target+": assignment rejected without changing owner");EvidenceRefuses(()=>trace.Finish());
   }
  });
  Test("callback cannot mutate or dispose a different session",()=>
  {
   foreach(string mutation in new[]{"record","leaf","mark","finish","dispose"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var other=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});var known=new FT();other.Record("zero",known,[]);bool rejected=false;
    using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>
    {
     try{switch(mutation){case "record":other.Record("zero",new FT(),[]);break;case "leaf":other.RegisterLeaf("late",1,new FT());break;case "mark":other.Mark("late",1,known,false);break;case "finish":_=other.Finish();break;case "dispose":other.Dispose();break;}}
     catch(MixedTrace.EvidenceFailure){rejected=true;}
    });
    MixedTrace.Active=trace;EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
    Need(rejected&&trace.Snapshot().Nodes.Length==0&&other.Snapshot().Nodes.Length==1&&other.Snapshot().Marks.Length==0,"neither session gains an accepted mutation");EvidenceRefuses(()=>trace.Finish());EvidenceRefuses(()=>other.Finish());
   }
  });
  Test("global mutation lease protects a direct session even when Active is null",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var other=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});bool rejected=false;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{try{MixedTrace.Active=other;}catch(MixedTrace.EvidenceFailure){rejected=true;}});
   Need(MixedTrace.Active is null,"direct fixture starts without active pointer");EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(rejected&&MixedTrace.Active is null&&trace.Snapshot().Nodes.Length==0,"owner tracked independently of active pointer");
   other.Record("zero",new FT(),[]);Need(other.Finish().Nodes.Length==1,"unused assignment target was not mutated or poisoned");
  });
  Test("healthy sealed point restores between children and remains a computation tripwire",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var point=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   MixedTrace.Active=point;point.Record("zero",new FT(),[]);_=point.Finish();
   for(int i=0;i<2;i++)
   {
    using var child=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=child;child.Record("zero",new FT(),[]);_=child.Finish();MixedTrace.Active=null;child.Dispose();
    MixedTrace.Active=point;Need(ReferenceEquals(MixedTrace.Active,point),"sealed point restoration after child close");
   }
   EvidenceRefuses(()=>MixedTrace.Record("zero",new FT(),[]));Need(ReferenceEquals(MixedTrace.Active,point)&&point.Snapshot().Nodes.Length==1,"finished point rejects computation rather than dropping tracing");
   MixedTrace.Active=null;point.Dispose();Need(MixedTrace.Active is null,"point/failure cleanup remains legal");
  });
  Test("failed and disposed targets cannot replace a healthy active session",()=>
  {
   foreach(bool disposed in new[]{false,true})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var target=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
    if(disposed)target.Dispose();else EvidenceRefuses(()=>target.Record("invalid",new FT(),[]));
    using var healthy=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=healthy;
    EvidenceRefuses(()=>MixedTrace.Active=target);Need(ReferenceEquals(MixedTrace.Active,healthy),"rejected target cannot replace owner");healthy.Record("zero",new FT(),[]);Need(healthy.Finish().Nodes.Length==1,"healthy owner unaffected by rejected inactive target");
   }
  });
  Test("foreign thread cannot mutate snapshot finish or dispose a session",()=>
  {
   foreach(string operation in new[]{"record","leaf","mark","finish","snapshot","dispose"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});var known=new FT();trace.Record("zero",known,[]);MixedTrace.Active=trace;
    Foreign(()=>EvidenceRefuses(()=>{switch(operation){case "record":trace.Record("zero",new FT(),[]);break;case "leaf":trace.RegisterLeaf("late",1,new FT());break;case "mark":trace.Mark("late",1,known,false);break;case "finish":_=trace.Finish();break;case "snapshot":_=trace.Snapshot();break;case "dispose":trace.Dispose();break;}}));
    Need(ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==1&&trace.Snapshot().Marks.Length==0,"foreign access leaves graph/pointer intact");EvidenceRefuses(()=>trace.Finish());
   }
  });
  Test("foreign active-pointer writes refuse without holding a lock across callbacks",()=>
  {
   foreach(bool inCallback in new[]{false,true})
   {
    using var s=MixedProducerAdmission.Enter(Generous);bool attempted=false;
    using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>
    {if(inCallback){Foreign(()=>EvidenceRefuses(()=>MixedTrace.Active=null));attempted=true;}});
    MixedTrace.Active=trace;
    if(inCallback)EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));else{Foreign(()=>EvidenceRefuses(()=>MixedTrace.Active=null));attempted=true;}
    Need(attempted&&ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==0,"foreign setter neither clears owner nor deadlocks callback");EvidenceRefuses(()=>trace.Finish());
   }
  });
  Test("healthy live session cannot reset admission by entering a fresh scope",()=>
  {
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   using(var first=MixedProducerAdmission.Enter(Generous))trace.Record("zero",new FT(),[]);
   using(var second=MixedProducerAdmission.Enter(Generous))
   {EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));Need(second.Snapshot().FormatCharacters==0&&second.Snapshot().Matrices.ArrayObjects==0&&trace.Snapshot().Nodes.Length==1,"scope identity checked before new resource work");EvidenceRefuses(()=>trace.Finish());}
  });
  Test("callback scope replacement cannot complete its in-flight session",()=>
  {
   using var first=MixedProducerAdmission.Enter(Generous);MixedProducerAdmission.Scope? replacement=null;
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{first.Dispose();replacement=MixedProducerAdmission.Enter(Generous);});
   try{EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));Need(trace.Snapshot().Nodes.Length==0,"scope switch detected before append");EvidenceRefuses(()=>trace.Finish());}
   finally{replacement?.Dispose();}
  });
  Test("foreign-owned inactive session cannot be installed as the active pointer",()=>
  {
   using var ready=new ManualResetEventSlim();using var release=new ManualResetEventSlim();MixedTrace.Session? foreign=null;Exception? error=null;
   var worker=new Thread(()=>
   {
    try{using var own=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});foreign=own;ready.Set();Need(release.Wait(5000),"owner release bounded");EvidenceRefuses(()=>own.Finish());}
    catch(Exception ex){error=ex;ready.Set();}
   }){IsBackground=true};worker.Start();
   try{Need(ready.Wait(5000)&&foreign is not null,"foreign fixture created");EvidenceRefuses(()=>MixedTrace.Active=foreign);Need(MixedTrace.Active is null,"rejected inactive target not installed");}
   finally{release.Set();Need(worker.Join(5000),"foreign owner cleanup bounded");}
   Need(error is null,"creator retains legal cleanup after rejected activation");
  });
  Test("another thread cannot mutate its own session while a foreign active owner exists",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var owner=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});MixedTrace.Active=owner;
   Foreign(()=>
   {
    using var otherScope=MixedProducerAdmission.Enter(Generous);using var other=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
    EvidenceRefuses(()=>other.Record("zero",new FT(),[]));Need(other.Snapshot().Nodes.Length==0&&otherScope.Snapshot().FormatCharacters==0,"active owner checked before foreign scope work");EvidenceRefuses(()=>other.Finish());
   });
   Need(ReferenceEquals(MixedTrace.Active,owner)&&owner.Snapshot().Nodes.Length==0,"foreign cleanup does not detach existing owner");EvidenceRefuses(()=>owner.Finish());
  });
  Test("sink lifetime refuses public pointer writes between session mutations",()=>
  {
   foreach(string target in new[]{"null","self"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
    var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});owner.Activate(trace);
    EvidenceRefuses(()=>owner.Callback(()=>{EvidenceRefuses(()=>MixedTrace.Active=target=="null"?null:trace);}));
    Need(ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==0,"temporary assignment refused before changing pointer");
    EvidenceRefuses(owner.CheckHealthy);EvidenceRefuses(()=>trace.Finish());owner.Detach();owner.DisposeSession(trace);
   }
  });
  Test("sink fences direct session mutation disposal and construction",()=>
  {
   foreach(string op in new[]{"record","leaf","mark","finish","dispose","construct","acquire"})
   {
    using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
    var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});owner.Activate(trace);
    EvidenceRefuses(()=>owner.Callback(()=>EvidenceRefuses(()=>
    {
     switch(op){case "record":trace.Record("zero",new FT(),[]);break;case "leaf":trace.RegisterLeaf("late",1,new FT());break;case "mark":trace.Mark("late",1,new FT(),false);break;case "finish":_=trace.Finish();break;case "dispose":trace.Dispose();break;case "construct":_=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});break;case "acquire":_=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);break;}
    })));
    Need(ReferenceEquals(MixedTrace.Active,trace)&&trace.Snapshot().Nodes.Length==0,"fenced "+op+" leaves state unchanged");owner.DisposeSession(trace);
   }
  });
  Test("sink callback cannot drop dispatched operations while no session is active",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
   EvidenceRefuses(()=>owner.Callback(()=>EvidenceRefuses(()=>MixedTrace.Record("zero",new FT(),[]))));
   Need(MixedTrace.Active is null,"null pointer remains unchanged");EvidenceRefuses(owner.CheckHealthy);
  });
  Test("even leaked sink capability cannot transition or clean up inside callback",()=>
  {
   foreach(string op in new[]{"activate","detach","disposeSession","disposeOwner","create","control"})
   {
    using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});owner.Activate(trace);
    EvidenceRefuses(()=>owner.Callback(()=>EvidenceRefuses(()=>
    {switch(op){case "activate":owner.Activate(null);break;case "detach":owner.Detach();break;case "disposeSession":owner.DisposeSession(trace);break;case "disposeOwner":owner.Dispose();break;case "create":_=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});break;case "control":owner.RequireControl();break;}})));
    Need(ReferenceEquals(MixedTrace.Active,trace),"rejected "+op+" cannot release ownership");owner.DisposeSession(trace);
   }
  });
  Test("nested rejected sink callback cannot release outer fence",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);int rejected=0;
   EvidenceRefuses(()=>owner.Callback(()=>
   {
    try{owner.Callback(()=>{});}catch(MixedTrace.EvidenceFailure){rejected++;}
    try{owner.Detach();}catch(MixedTrace.EvidenceFailure){rejected++;}
    try{owner.Dispose();}catch(MixedTrace.EvidenceFailure){rejected++;}
   }));Need(rejected==3,"all nested entries refused without lifting fence");owner.Detach();
  });
  Test("sink point child restoration and callback snapshots remain supported",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
   var point=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>owner.Callback(()=>{}));owner.Activate(point);MixedTrace.Record("zero",new FT(),[]);_=point.Finish();
   owner.Callback(()=>Need(point.Snapshot().Nodes.Length==1,"sealed background remains readable"));
   for(int i=0;i<2;i++)
   {
    var child=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>owner.Callback(()=>{}),(_,_)=>owner.Callback(()=>{}));owner.Activate(child);
    var value=new FT();MixedTrace.Record("zero",value,[]);child.Mark("expanded",1,value,true);_=child.Finish();owner.Detach();owner.Callback(()=>Need(child.Snapshot().Marks.Length==1,"closed child independently readable"));owner.DisposeSession(child);owner.Activate(point);
   }
   Need(ReferenceEquals(MixedTrace.Active,point),"healthy sealed point restored");owner.DisposeSession(point);owner.CheckHealthy();
  });
  Test("sink sealed point still trips on computation and fails whole owner",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
   var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});owner.Activate(trace);_=trace.Finish();
   EvidenceRefuses(()=>MixedTrace.Record("zero",new FT(),[]));EvidenceRefuses(owner.CheckHealthy);owner.DisposeSession(trace);
  });
  Test("sink foreign calls refuse without deadlocking a waiting callback",()=>
  {
   foreach(string op in new[]{"pointer","control","callback","dispose"})
   {
    using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
    EvidenceRefuses(()=>owner.Callback(()=>Foreign(()=>EvidenceRefuses(()=>
    {switch(op){case "pointer":MixedTrace.Active=null;break;case "control":owner.RequireControl();break;case "callback":owner.Callback(()=>{});break;case "dispose":owner.Dispose();break;}}))));
    EvidenceRefuses(owner.CheckHealthy);owner.Detach();
   }
  });
  Test("sink failure cleanup and diagnostic access survive poisoned arithmetic",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
   var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});owner.Activate(trace);trace.Record("zero",new FT(),[]);s.Poison();Refuses(()=>trace.Finish());
   Need(trace.Snapshot().Nodes.Length==1,"incomplete diagnostics retained");owner.Detach();owner.DisposeSession(trace);owner.DisposeSession(trace);owner.Dispose();owner.Dispose();Need(MixedTrace.Active is null,"idempotent authorized cleanup requires no arithmetic");
  });
  Test("released sink sessions cannot resume or attach under a fresh owner",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var oldOwner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);
   var old=oldOwner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});oldOwner.Activate(old);oldOwner.Dispose();
   using var fresh=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);EvidenceRefuses(()=>old.Record("zero",new FT(),[]));EvidenceRefuses(fresh.CheckHealthy);
   Need(MixedTrace.Active is null&&old.Snapshot().Nodes.Length==0,"released sessions cannot escape token lifetime");
  });
  Test("ordinary callback exceptions fail sink ownership and preserve the exception",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var marker=new InvalidOperationException("manufactured prerequisite failure");bool caught=false;
   try{owner.Callback(()=>throw marker);}catch(InvalidOperationException ex){Need(ReferenceEquals(ex,marker),"original failure preserved");caught=true;}
   Need(caught,"callback threw");EvidenceRefuses(owner.CheckHealthy);owner.Detach();
  });
  Test("preexisting standalone session cannot bypass a new sink owner",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var standalone=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   using(var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps))
   {EvidenceRefuses(()=>standalone.Record("zero",new FT(),[]));EvidenceRefuses(owner.CheckHealthy);Need(standalone.Snapshot().Nodes.Length==0,"old inactive session cannot produce under new owner");}
  });
  Test("capture ceilings are mandatory nonnegative safe integers with no implicit defaults",()=>
  {
   foreach(long bad in new[]{-1L,9007199254740992L})foreach(int field in Enumerable.Range(0,5))foreach(int lane in Enumerable.Range(0,3))
   {
    var c=field switch{0=>CopyCaps with{Graphs=bad},1=>CopyCaps with{NodeObjects=bad},2=>CopyCaps with{Arrays=bad},3=>CopyCaps with{Slots=bad},_=>CopyCaps with{ElementCopies=bad}};
    var caps=lane switch{0=>CaptureCaps with{Normal=c},1=>CaptureCaps with{Inspection=c},_=>CaptureCaps with{Failure=c}};
    EvidenceRefuses(()=>MixedTrace.Session.AcquireSinkOwnership(caps));Need(MixedTrace.Active is null,"failed acquisition does not install an owner");
   }
   using var valid=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);valid.CheckHealthy();
  });
  Test("capture exact shape counts graph node clones every array and repeated input slot",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var cost=new MixedTrace.CopyCounts(1,2,5,6,6);
   using var owner=MixedTrace.Session.AcquireSinkOwnership(new(cost,cost,cost));
   string hash=Convert.ToHexString(SHA256.HashData("[]"u8)).ToLowerInvariant();
   var trace=owner.CreateSession(CopyCaps.Graphs,[new("leaf",1,"manufactured",hash)],WireLimits,(_,_,_)=>{},(_,_)=>{});
   var a=new FT();trace.RegisterLeaf("leaf",1,a);var b=trace.Record("sum",new FT(),[a,a]);trace.Mark("mark",1,b,false);
   var inspected=trace.Snapshot();var graph=trace.Finish();var usage=owner.CaptureUsage();
   Need(usage.Inspection==cost&&usage.Normal==cost&&usage.Failure==default,"separate exact charges; duplicate operands each copied");
   inspected.Nodes[1].Inputs[0]=999;Need(graph.Nodes[1].Inputs.SequenceEqual(new[]{0,0}),"inspection and final graph are detached");
   var failed=owner.FailureSnapshot(trace);Need(failed.Nodes[1].Inputs.SequenceEqual(new[]{0,0})&&owner.CaptureUsage().Failure==cost,"reserved failure copy remains detached");owner.DisposeSession(trace);
  });
  Test("each one-short capture dimension refuses atomically and leaves failure reserve untouched",()=>
  {
   foreach(bool normal in new[]{false,true})foreach(int field in Enumerable.Range(0,5))
   {
    using var s=MixedProducerAdmission.Enter(Generous);var cost=new MixedTrace.CopyCounts(1,1,4,1,1);
    var shortCost=field switch{0=>cost with{Graphs=0},1=>cost with{NodeObjects=0},2=>cost with{Arrays=3},3=>cost with{Slots=0},_=>cost with{ElementCopies=0}};
    var caps=normal?new MixedTrace.CaptureLimits(shortCost,cost,cost):new MixedTrace.CaptureLimits(cost,shortCost,cost);
    using var owner=MixedTrace.Session.AcquireSinkOwnership(caps);var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});trace.Record("zero",new FT(),[]);
    EvidenceRefuses(()=>{if(normal)_=trace.Finish();else _=trace.Snapshot();});var usage=owner.CaptureUsage();
    Need(usage.Normal==default&&usage.Inspection==default&&usage.Failure==default&&(normal?usage.NormalFailed:usage.InspectionFailed),"all checks precede counter commit/allocation");
    Need(owner.FailureSnapshot(trace).Nodes.Length==1&&owner.CaptureUsage().Failure==cost,"normal refusal cannot drain diagnostic reserve");owner.DisposeSession(trace);
   }
  });
  Test("capture budgets aggregate point and child without renewal on new sessions",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var cost=new MixedTrace.CopyCounts(1,1,4,1,1);
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps with{Normal=cost});
   var point=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});point.Record("zero",new FT(),[]);var retained=point.Finish();
   var child=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});child.Record("zero",new FT(),[]);EvidenceRefuses(()=>child.Finish());
   Need(owner.CaptureUsage().Normal==cost&&owner.CaptureUsage().NormalFailed&&retained.Nodes.Length==1,"child cannot replace cumulative budget while point result is retained");
   owner.DisposeSession(child);owner.DisposeSession(point);
  });
  Test("empty graphs still charge graph and three array headers",()=>
  {
   var cost=new MixedTrace.CopyCounts(1,0,3,0,0);using var trace=new MixedTrace.Session(new(cost,cost,cost),[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   Need(trace.Snapshot().Nodes.Length==0&&trace.CaptureUsage().Inspection==cost,"empty captures are not free");
   EvidenceRefuses(()=>trace.Snapshot());Need(trace.CaptureUsage().Inspection==cost&&trace.CaptureUsage().InspectionFailed,"empty snapshot repetition is bounded");
  });
  Test("swallowed callback inspection exhaustion prevents outer node commit",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps with{Inspection=default});MixedTrace.Session? trace=null;bool armed=false;
   trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{if(armed)EvidenceRefuses(()=>trace!.Snapshot());});
   trace.Record("zero",new FT(),[]);armed=true;EvidenceRefuses(()=>trace.Record("zero",new FT(),[]));
   Need(owner.FailureSnapshot(trace).Nodes.Length==1&&owner.CaptureUsage().InspectionFailed,"no accepted outer node after swallowed snapshot failure");owner.DisposeSession(trace);
  });
  Test("failure capture uses its frozen reserve after arithmetic poisoning and scope disposal",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   using(var s=MixedProducerAdmission.Enter(Generous)){trace.Record("zero",new FT(),[]);s.Poison();Refuses(()=>trace.Finish());}
   Need(owner.FailureSnapshot(trace).Nodes.Length==1&&owner.CaptureUsage().Failure.Graphs==1,"no new arithmetic scope or new reserve is needed");
   EvidenceRefuses(owner.CheckHealthy);owner.DisposeSession(trace);
  });
  Test("failure reserve covers simultaneous point and child once each and never refills",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);var cost=new MixedTrace.CopyCounts(2,2,8,2,2);
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps with{Failure=cost});
   var point=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});point.Record("zero",new FT(),[]);var complete=point.Finish();
   var child=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});child.Record("zero",new FT(),[]);EvidenceRefuses(()=>child.Record("invalid",new FT(),[]));
   var a=owner.FailureSnapshot(point);var b=owner.FailureSnapshot(child);
   Need(a.Nodes.Length==1&&b.Nodes.Length==1&&complete.Nodes.Length==1&&owner.CaptureUsage().Failure==cost,"all retained results included in cumulative lanes");
   EvidenceRefuses(()=>owner.FailureSnapshot(child));Need(owner.CaptureUsage().Failure==cost&&owner.CaptureUsage().FailureFailed,"failure copying itself has a terminal cap");
   owner.DisposeSession(point);owner.DisposeSession(child);
  });
  Test("callbacks cannot spend the sink failure reserve through a leaked capability",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   EvidenceRefuses(()=>owner.Callback(()=>EvidenceRefuses(()=>owner.FailureSnapshot(trace))));
   Need(owner.CaptureUsage().Failure==default,"fence protects reserve before charging");Need(owner.FailureSnapshot(trace).Nodes.Length==0,"legitimate subsequent failure remains observable");owner.DisposeSession(trace);
  });
  Test("foreign snapshot cannot drain either diagnostic lane",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var trace=owner.CreateSession(CopyCaps.Graphs,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   Foreign(()=>EvidenceRefuses(()=>trace.Snapshot()));Need(owner.CaptureUsage().Inspection==default&&owner.CaptureUsage().Failure==default,"thread identity precedes admission");
   Need(owner.FailureSnapshot(trace).Nodes.Length==0,"owner cleanup retains its reserve");owner.DisposeSession(trace);
  });
  Test("copy counts retain committed implicit zeros after a later parameter failure",()=>
  {
   using var s=MixedProducerAdmission.Enter(Generous);using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   var a=new FT();var b=new FT();EvidenceRefuses(()=>trace.Record("invalid",new FT(),[a,b,a]));
   var graph=trace.Snapshot();Need(graph.Nodes.Length==2&&trace.CaptureUsage().Inspection==new MixedTrace.CopyCounts(1,2,5,2,2),"failed requested node adds no phantom input slots to committed graph");
  });
  Test("capture usage is detached allocation-free value metadata and budgets cannot be replaced",()=>
  {
   using var trace=new MixedTrace.Session(CaptureCaps,[],WireLimits,(_,_,_)=>{},(_,_)=>{});var before=trace.CaptureUsage();_=trace.Snapshot();
   var after=trace.CaptureUsage();before=before with{Inspection=CopyCaps};
   Need(after.Inspection.Graphs==1&&trace.CaptureUsage().Inspection.Graphs==1,"returned usage values cannot rewrite counters");
  });
  Test("zero context inspection policy refuses before spending shared copy allowance",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var trace=owner.CreateSession(0,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   EvidenceRefuses(()=>trace.Snapshot());Need(owner.CaptureUsage().Inspection==default,"context policy precedes shared admission and allocation");
   Need(owner.FailureSnapshot(trace).Nodes.Length==0,"failure allowance remains independently usable");owner.DisposeSession(trace);
  });
  Test("context inspection policy cannot borrow another context allowance or reset on refusal",()=>
  {
   using var owner=MixedTrace.Session.AcquireSinkOwnership(CaptureCaps);var first=owner.CreateSession(1,[],WireLimits,(_,_,_)=>{},(_,_)=>{});
   var second=owner.CreateSession(2,[],WireLimits,(_,_,_)=>{},(_,_)=>{});_=first.Snapshot();EvidenceRefuses(()=>first.Snapshot());EvidenceRefuses(()=>first.Snapshot());
   Need(owner.CaptureUsage().Inspection.Graphs==1,"shared room cannot replenish exhausted context policy");
   _=second.Snapshot();_=second.Snapshot();EvidenceRefuses(()=>second.Snapshot());Need(owner.CaptureUsage().Inspection.Graphs==3,"separate bounded diagnostics still inspect after failure");
   owner.DisposeSession(first);owner.DisposeSession(second);
  });
  Console.WriteLine($"{tests} manufactured admission tests passed; no science executed.");
 }
}
