using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class CurvedWard
{
 public static long SquareEntries{get;private set;}
 public static long ChainStages{get;private set;}
 public static bool FrequencyPassed{get;private set;}=true;
 public static bool CarrierPassed{get;private set;}=true;
 public static FT Read(JsonElement e){var r=new FT();foreach(var q in e.EnumerateArray()){int f=q.GetProperty("form").GetInt32(),b=q.GetProperty("blade").GetInt32(),k=q.GetProperty("k0").GetInt32(),l=q.GetProperty("k1").GetInt32();var c=new Scalar(SpinGeometry.Parse(q.GetProperty("real").GetString()!),SpinGeometry.Parse(q.GetProperty("imaginary").GetString()!));if(f<0||f>Full||b<0||b>Full||k!=0||l!=0||c.IsZero||r.ContainsKey((f,b,k,l)))throw new ArgumentException("tensor record");Put(r,(f,b,k,l),c);}return r;}
 public static FT Neg(FT x)=>Scale(x,-1);
 public static Jet Neg(Jet x)=>Jet.Scale(x,-1);
 public static Jet Conj(Jet inverse,Jet x,Jet epsilon)=>Jet.Product(Jet.Product(inverse,x),epsilon);
 public static FT Comm(FT x,FT y)=>Add(Product(x,y),Neg(Product(y,x)));
 public static Jet Comm(Jet x,Jet y)=>Jet.Add(Jet.Product(x,y),Neg(Jet.Product(y,x)));
 public static FT Anti(FT x,FT y)=>Add(Product(x,y),Product(y,x));
 public static Jet Anti(Jet x,Jet y)=>Jet.Add(Jet.Product(x,y),Jet.Product(y,x));
 // At p: A_a=0 and partial_a A_b=F_ab/2. Ordinary mixed second
 // partials of any smooth endomorphism cancel; the remaining connection
 // terms are expanded individually, before comparing with [F,z].
 public static FT NormalSquare(FT f,FT z)
 {
  var result=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
  {
   SquareEntries++;var fab=SpinGeometry.Slice(f,a,b);var ab=Scale(fab,Fourier.Half);var ba=Scale(fab,Fourier.Half*-1);
   var left=Add(Product(ab,z),Neg(Product(z,ab)));var right=Add(Product(ba,z),Neg(Product(z,ba)));
   result=Add(result,Product(One((1<<a)|(1<<b),0,1),Add(left,Neg(right))));
  }return result;
 }
 public static Jet NormalSquare(FT f,Jet z)=>new(NormalSquare(f,z.Value),NormalSquare(f,z.Delta));
 public static Jet[] Chain(Jet f,Jet pFirst,Jet pOuter,Jet pInner)
 {
  var sf=Jet.Star(f);var first=Jet.Product(pFirst,sf,'C');var inner=Jet.Product(pInner,sf,'A');var zero=Jet.Star(inner);var outer=Jet.Product(pOuter,zero,'A');var upper=Jet.Add(first,Jet.Scale(Jet.Star(outer),Fourier.Half*-1));var lower=Jet.Star(upper);
  Jet[] stages=[f,sf,first,inner,zero,outer,upper,lower];int[] degree=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++){ChainStages++;if(!Typed(stages[i].Value,degree[i])||!Typed(stages[i].Delta,degree[i]))throw new InvalidOperationException("CAA dual stage type");FrequencyPassed&=stages[i].Value.Keys.Concat(stages[i].Delta.Keys).All(q=>q.K0==0&&q.K1==0);}return stages;
 }
 public static (FT E,FT DE,FT Inverse,FT DInverse) Epsilon(int mode)
 {
  var one=One(0,0,1);var n=Add(One(0,4,1),One(0,6,1));var m=Add(One(0,4,1),One(0,6,-1));
  if(mode==0)return(one,new(),one,new());
  var a=Add(one,n);var b=Add(one,m);var ai=Add(one,Neg(n));var bi=Add(one,Neg(m));
  var da=One(1,0,1);da=Product(da,n);var db=Product(One(2,0,1),m);
  return(Product(a,b),Add(Product(da,b),Product(a,db)),Product(bi,ai),Add(Product(Neg(db),ai),Product(bi,Neg(da))));
 }
 public static (FT B,FT DB,FT Omega,FT DOmega) Coordinates(FT f,FT e,FT de,FT inverse,FT dinverse,FT s,FT ds)
 {
  var b=Product(inverse,de);var db=Add(Product(dinverse,de),Product(inverse,NormalSquare(f,e)));
  var t=Conjugate(inverse,s,e);var dt=Add(Add(Product(Product(dinverse,s),e),Product(Product(inverse,ds),e)),Neg(Product(Product(inverse,s),de)));
  return(b,db,Add(b,t),Add(db,dt));
 }
 public static Result Evaluate(FT f,Jet e,Jet de,Jet inverse,Jet dinverse,Jet omega,Jet domega,bool freezeCurvature=false,bool freezeTensors=false)
 {
  var b=Jet.Product(inverse,de);var db=Jet.Add(Jet.Product(dinverse,de),Jet.Product(inverse,NormalSquare(f,e)));
  var t=Jet.Add(omega,Neg(b));var dt=Jet.Add(domega,Neg(db));var fb=Jet.Add(Jet.Fixed(f),Jet.Add(db,Jet.Product(b,b)));
  if(freezeCurvature)fb=Jet.Fixed(fb.Value);
  var covariant=Jet.Add(dt,Anti(b,t));var q=Jet.Product(t,t);
  var pFirst=Conj(inverse,Jet.Fixed(Caa.Gamma1),e);var pOuter=Conj(inverse,Jet.Fixed(Caa.Gamma1),e);var pInner=Conj(inverse,Jet.Fixed(Caa.Gamma2),e);
  if(freezeTensors){pFirst=Jet.Fixed(pFirst.Value);pOuter=Jet.Fixed(pOuter.Value);pInner=Jet.Fixed(pInner.Value);}
  foreach(var (tensor,degree) in new[]{(b,1),(db,2),(t,1),(dt,2),(fb,2),(covariant,2),(q,2),(pFirst,1),(pOuter,1),(pInner,2)})CarrierPassed&=Typed(tensor.Value,degree)&&Typed(tensor.Delta,degree)&&HAnti(tensor.Value)&&HAnti(tensor.Delta);
  var chains=new[]{Chain(fb,pFirst,pOuter,pInner),Chain(covariant,pFirst,pOuter,pInner),Chain(q,pFirst,pOuter,pInner)};
  var raw=new[]{Jet.Pair(t,chains[0][7]),Jet.Pair(t,chains[1][7]),Jet.Pair(t,chains[2][7]),Jet.Pair(t,t)};
  Rational[] factors=[1,new Rational(1,2),1,new Rational(1,2)];
  return new(b,db,t,dt,fb,covariant,q,pFirst,pOuter,pInner,chains,raw.Select((x,i)=>x.Value*factors[i]).ToArray(),raw.Select((x,i)=>x.Delta*factors[i]).ToArray());
 }
 public static Rational[] BaseVariation(FT f,FT s,FT ds,FT v,FT dv)
 {
  var q=Product(s,s);return[Pair(v,Caa.Forward(f)),(Pair(v,Caa.Forward(ds))+Pair(s,Caa.Forward(dv)))*new Rational(1,2),Pair(v,Caa.Forward(q))+Pair(s,Caa.Forward(Anti(v,s))),Pair(s,v)];
 }
 public static Rational Total(Rational[] p,int gamma,int kappa)=>p[0]+p[1]+p[2]*new Rational(gamma,3)+p[3]*kappa;
 public static bool REqual(Rational[] a,Rational[] b)=>a.SequenceEqual(b);
 public static object JetTerms(Jet j)=>new{value=Terms(j.Value),delta=Terms(j.Delta)};
 internal sealed record Result(Jet B,Jet DB,Jet T,Jet DT,Jet FB,Jet Covariant,Jet Q,Jet FirstPhi,Jet OuterPhi,Jet InnerPhi,Jet[][] Stages,Rational[] Pieces,Rational[] Derivatives)
 {
  public object Evidence()=>new{B=JetTerms(B),DB=JetTerms(DB),T=JetTerms(T),DT=JetTerms(DT),FB=JetTerms(FB),covariant=JetTerms(Covariant),Q=JetTerms(Q),firstPhi=JetTerms(FirstPhi),outerPhi=JetTerms(OuterPhi),innerPhi=JetTerms(InnerPhi),stages=Stages.Select(c=>c.Select(JetTerms).ToArray()).ToArray(),pieces=Pieces.Select(x=>x.ToString()).ToArray(),derivatives=Derivatives.Select(x=>x.ToString()).ToArray()};
 }
}
