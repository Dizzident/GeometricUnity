using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Full literal operators, with counters for loop visits BEFORE subset and
// Clifford-zero pruning. Nothing in this helper knows a target coefficient.
internal static class Certified
{
 public static long TransposePairVisits{get;private set;}
 public static long WordTransposePairVisits{get;private set;}
 public static long NaiveProductPairVisits{get;private set;}
 public static long TransposeCalls{get;private set;}
 public static long WordTransposeCalls{get;private set;}
 public static long NaiveProductCalls{get;private set;}
 public static long AdjointCalls{get;private set;}
 public static long KineticCalls{get;private set;}
 public static long DerivativeSlots{get;private set;}
 public static long ForwardCalls{get;private set;}
 public const long MaximumTransposePairVisits=300000000;
 public const long MaximumWordTransposePairVisits=250000000;
 public const long MaximumNaiveProductPairVisits=300000000;
 private static long Visits(FT a,FT b)=>checked((long)a.Count*b.Count);
 public static FT Transpose(FT phi,FT output,char kind)
 {TransposeCalls++;TransposePairVisits=checked(TransposePairVisits+Visits(phi,output));if(TransposePairVisits>MaximumTransposePairVisits)throw new ResourceLimitException("transpose pair-visit ceiling");return global::Adjoint.BracketAdjoint(phi,output,kind);}
 public static FT WordTranspose(FT phi,FT output)
 {
  WordTransposeCalls++;WordTransposePairVisits=checked(WordTransposePairVisits+Visits(phi,output));if(WordTransposePairVisits>MaximumWordTransposePairVisits)throw new ResourceLimitException("word-transpose pair-visit ceiling");
  var r=new FT();foreach(var x in phi)foreach(var y in output)if((x.Key.Form&y.Key.Form)==x.Key.Form)
  {int form=x.Key.Form^y.Key.Form,metric=Degree(x.Key.Form&0x3f80)%2==0?1:-1;int factor=WordSign(y.Key.Blade,x.Key.Blade)-WordSign(x.Key.Blade,y.Key.Blade);Put(r,(form,x.Key.Blade^y.Key.Blade,x.Key.K0+y.Key.K0,x.Key.K1+y.Key.K1),x.Value*y.Value*(metric*Shuffle(x.Key.Form,form)*factor));}return r;
 }
 public static FT Naive(FT a,FT b,char kind='W')
 {NaiveProductCalls++;NaiveProductPairVisits=checked(NaiveProductPairVisits+Visits(a,b));if(NaiveProductPairVisits>MaximumNaiveProductPairVisits)throw new ResourceLimitException("naive product pair-visit ceiling");return NaiveProduct(a,b,kind);}
 public static FT[] Forward(FT input,bool naive=false)
 {
  ForwardCalls++;FT Prod(FT a,FT b,char kind)=>naive?Naive(a,b,kind):Product(a,b,kind);
  var star=Star(input);var first=Prod(Caa.Gamma1,star,'C');var inner=Prod(Caa.Gamma2,star,'A');var zero=Star(inner);var outer=Prod(Caa.Gamma1,zero,'A');var upper=Add(first,Scale(Star(outer),Fourier.Half*-1));var lower=Star(upper);
  return[input,star,first,inner,zero,outer,upper,lower];
 }
 public static (FT First,FT Second,FT Full,FT Simplified) Adjoint(FT input)
 {
  AdjointCalls++;var upper=global::Adjoint.StarAdjoint(input,13);var first=global::Adjoint.StarAdjoint(Transpose(Caa.Gamma1,upper,'C'),2);var one=global::Adjoint.StarAdjoint(upper,1);var zero=Transpose(Caa.Gamma1,one,'A');var top=global::Adjoint.StarAdjoint(zero,14);var second=Scale(global::Adjoint.StarAdjoint(Transpose(Caa.Gamma2,top,'A'),2),Fourier.Half*-1);
  var simpleFirst=Scale(Star(Transpose(Caa.Gamma1,Star(input),'C')),-1);var simpleSecond=Scale(Star(Transpose(Caa.Gamma2,Star(Transpose(Caa.Gamma1,input,'A')),'A')),Fourier.Half*-1);return(first,second,Add(first,second),Add(simpleFirst,simpleSecond));
 }
 public static CertifiedKinetic Kinetic(Rational[][,] lambda,FT input)
 {
  KineticCalls++;var derivatives=new FT[14];var oracle=new FT[14];for(int a=0;a<14;a++){DerivativeSlots++;derivatives[a]=Fifth.Derivative(lambda[a],input);oracle[a]=Fifth.ExteriorSlots(lambda[a],input);}
  var exterior=new FT();var alternateExterior=new FT();for(int a=0;a<14;a++){exterior=Add(exterior,Product(One(1<<a,0,1),derivatives[a]));alternateExterior=Add(alternateExterior,Product(One(1<<a,0,1),oracle[a]));}
  var stages=Forward(exterior);var naive=Forward(exterior,true);var adjoint=Adjoint(input);var reverse=new FT[14];var alternateReverse=new FT[14];var alternateSimplified=new FT[14];
  for(int a=0;a<14;a++){reverse[a]=Fifth.Derivative(lambda[a],adjoint.Full);var parallel=Adjoint(derivatives[a]);alternateReverse[a]=parallel.Full;alternateSimplified[a]=parallel.Simplified;}
  var back=Fifth.Divergence(reverse);var alternateBack=Fifth.Divergence(alternateReverse);return new(input,derivatives,oracle,exterior,alternateExterior,stages,naive,adjoint.First,adjoint.Second,adjoint.Full,adjoint.Simplified,reverse,alternateReverse,alternateSimplified,back,alternateBack,Scale(Add(stages[7],back),Fourier.Half));
 }
 public static Rational Norm(FT input)=>input.Values.Aggregate(new Rational(0),(s,z)=>s+Abs(z.Real)+Abs(z.Imaginary));
 public static Rational Abs(Rational q)=>q.Numerator.Sign<0?q*(-1):q;
 public static FT Evaluate(FT[] coefficients,Rational lambda)
 {var result=new FT();Rational power=1;foreach(var coefficient in coefficients){result=Add(result,Scale(coefficient,new Scalar(power,0)));power*=lambda;}return result;}
 public static FT[] Zeros(int count)=>Enumerable.Range(0,count).Select(_=>new FT()).ToArray();
 public static FT Read(JsonElement rows)
 {var t=Feedback.Read(rows);var keys=rows.EnumerateArray().Select(r=>(r.GetProperty("form").GetInt32(),r.GetProperty("blade").GetInt32(),r.GetProperty("k0").GetInt32(),r.GetProperty("k1").GetInt32())).ToArray();if(t.Count!=keys.Length||keys.Distinct().Count()!=keys.Length||!keys.SequenceEqual(keys.Order()))throw new ArgumentException("ordered unique nonzero tensor");foreach(var r in rows.EnumerateArray()){if(r.EnumerateObject().Count()!=6||!r.EnumerateObject().Select(p=>p.Name).ToHashSet().SetEquals(new[]{"form","blade","k0","k1","real","imaginary"}))throw new ArgumentException("exact tensor properties");foreach(string key in new[]{"real","imaginary"}){string value=r.GetProperty(key).GetString()!;if(Feedback.Parse(value).ToString()!=value)throw new ArgumentException("canonical rational tensor");}}return t;}
}
internal sealed record CertifiedKinetic(FT Input,FT[] Derivatives,FT[] DerivativeOracles,FT Exterior,FT ExteriorOracle,FT[] Stages,FT[] NaiveStages,FT AdjointFirst,FT AdjointSecond,FT Adjoint,FT SimplifiedAdjoint,FT[] ReverseDerivatives,FT[] ParallelReverse,FT[] ParallelSimplified,FT Reverse,FT ReverseOracle,FT Full);
internal sealed class ResourceLimitException(string message):Exception(message);
