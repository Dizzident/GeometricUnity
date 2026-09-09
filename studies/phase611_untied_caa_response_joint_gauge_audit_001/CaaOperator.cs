using static Algebra;
using static Fourier;
using static Adjoint;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Caa
{
 public static FT Gamma1=>Enumerable.Range(0,14).Aggregate(new FT(),(s,a)=>Add(s,One(1<<a,1<<a,1)));
 public static FT Gamma2
 {get{var t=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(t,((1<<a)|(1<<b),(1<<a)|(1<<b),0,0),1);return t;}}
 public static FT[] ForwardStages(FT f,bool naive=false)
 {
  FT Prod(FT a,FT b,char kind)=>naive?NaiveProduct(a,b,kind):Product(a,b,kind);
  var sf=Star(f);var one=Prod(Gamma1,sf,'C');var inner=Prod(Gamma2,sf,'A');var zero=Star(inner);var outer=Prod(Gamma1,zero,'A');var upper=Add(one,Scale(Star(outer),Fourier.Half*-1));var lower=Star(upper);
  FT[] stages=[f,sf,one,inner,zero,outer,upper,lower];int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++)if(!Typed(stages[i],degrees[i]))throw new InvalidOperationException("CAA forward type");return stages;
 }
 public static FT Forward(FT f)=>ForwardStages(f)[7];
 public static (FT First,FT Second) AdjointLegs(FT y)
 {
  var upper=StarAdjoint(y,13);var first=StarAdjoint(BracketAdjoint(Gamma1,upper,'C'),2);var outerOne=StarAdjoint(upper,1);var zero=BracketAdjoint(Gamma1,outerOne,'A');var top=StarAdjoint(zero,14);var twelve=BracketAdjoint(Gamma2,top,'A');var second=Scale(StarAdjoint(twelve,2),Fourier.Half*-1);
  foreach(var (v,d) in new[]{(y,1),(upper,13),(first,2),(outerOne,1),(zero,0),(top,14),(twelve,12),(second,2)})if(!Typed(v,d))throw new InvalidOperationException("CAA reverse type");return(first,second);
 }
 public static FT AdjointLiteral(FT y){var legs=AdjointLegs(y);return Add(legs.First,legs.Second);}
 public static FT AdjointSimplified(FT y)
 {var first=Scale(Star(BracketAdjoint(Gamma1,Star(y),'C')),-1);var second=Scale(Star(BracketAdjoint(Gamma2,Star(BracketAdjoint(Gamma1,y,'A')),'A')),Fourier.Half*-1);return Add(first,second);}
 public static FT OuterAdjoint(FT y)=>BracketAdjoint(Gamma1,y,'A');
 // K0 is an abstract Fourier phase; t is its separately declared physical axis.
 public static FT Derivative(FT f,int t)=>Product(One(1<<t,0,1),Partial(f,0));
 public static FT DerivativeAdjoint(FT f,int t)
 {var r=new FT();foreach(var q in Partial(f,0))if((q.Key.Form&(1<<t))!=0){int sign=Degree(q.Key.Form&((1<<t)-1))%2==0?1:-1;Put(r,(q.Key.Form^(1<<t),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(-Sigma(t)*sign));}return r;}
 public static FT Hessian(FT f,int t)=>Scale(Add(Forward(Derivative(f,t)),DerivativeAdjoint(AdjointLiteral(f),t)),Fourier.Half);
 public static FT Mode(int n,bool sine,int form,int blade,Scalar coefficient)
 {if(n==0)return sine?new():One(form,blade,coefficient);var r=new FT();foreach(var q in Trig(0,sine,form,blade))Put(r,(q.Key.Form,q.Key.Blade,q.Key.K0*n,0),q.Value*coefficient);return r;}
 public static FT U(int t,int n)=>Mode(n,false,1<<t,1<<2,1);
 public static FT E(int t,int n)
 {var r=new FT();for(int j=0;j<14;j++)if(j!=t&&j!=2)r=Add(r,Mode(n,true,1<<j,(1<<2)^(1<<j),BladeSign(1<<2,1<<j)));return r;}
 public static FT ExpectedAdjointU(int t,int n)
 {var r=new FT();for(int j=0;j<14;j++)if(j!=t&&j!=2)r=Add(r,Mode(n,false,(1<<t)|(1<<j),(1<<2)^(1<<j),2*Shuffle(1<<t,1<<j)*BladeSign(1<<2,1<<j)));return r;}
 public static FT ExpectedAdjointE(int t,int n)
 {var r=new FT();for(int j=0;j<14;j++)if(j!=t&&j!=2)r=Add(r,Mode(n,true,(1<<2)|(1<<j),1<<j,2*Shuffle(1<<2,1<<j)));return r;}
 public static FT ReconstructAdjoint(FT y,int t,int n,int carrierKind,Action<bool> probe)
 {
  var output=new FT();var actual=AdjointLiteral(y);for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
  {
   int form=(1<<a)|(1<<b),blade=carrierKind==2?form:carrierKind==0?form^(1<<t)^(1<<2):form^(1<<2);Scalar phase=AdjointSign(blade)==-1?1:Scalar.I;
   var f=Mode(n,carrierKind==1,form,blade,phase);Rational norm=Pair(f,f);if(norm==0)throw new InvalidOperationException("probe Gram");Rational pairing=Pair(Forward(f),y);var coefficient=pairing*new Rational(norm.Denominator,norm.Numerator);output=Add(output,Scale(f,new Scalar(coefficient,0)));probe(pairing==Pair(f,actual)&&HAnti(f));
  }return output;
 }
}
