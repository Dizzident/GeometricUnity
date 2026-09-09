using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Covariant
{
 public static FT Phase(FT coefficient,bool sine)=>Product(coefficient,Trig(0,sine));
 public static FT Wedge(FT a,FT b)=>NaiveProduct(a,b,'W');
 public static FT CliffordWedge(FT a,FT b)=>Scale(NaiveProduct(a,b,'C'),Fourier.Half);
 public static FT Covector(int a,int b,int n)=>Add(One(1,0,n*a),One(128,0,n*b));
 public static FT RaisedClifford(int a,int b,int n)=>Add(One(0,1,n*a),One(0,128,-n*b));
 public static FT Contract(FT q,FT f,bool wrongPositiveMetric=false)
 {var result=new FT();foreach(var cov in q){int axis=System.Numerics.BitOperations.TrailingZeroCount((uint)cov.Key.Form);if(cov.Key.Blade!=0||cov.Key.K0!=0||cov.Key.K1!=0||Degree(cov.Key.Form)!=1)throw new InvalidOperationException("constant scalar covector");foreach(var x in f)if((x.Key.Form&cov.Key.Form)!=0){int sign=Degree(x.Key.Form&(cov.Key.Form-1))%2==0?1:-1;Put(result,(x.Key.Form^cov.Key.Form,x.Key.Blade,x.Key.K0,x.Key.K1),cov.Value*x.Value*(sign*(wrongPositiveMetric?1:Sigma(axis))));}}return result;}
 public static FT Dq(FT q,FT f)=>Product(q,Partial(f,0));
 public static FT DqAdjoint(FT q,FT f)=>Scale(Contract(q,Partial(f,0)),-1);
 public static FT H(FT q,FT f)=>Scale(Add(Caa.Forward(Dq(q,f)),DqAdjoint(q,Caa.AdjointLiteral(f))),Fourier.Half);
 // Coefficient tensors U,A,qC,E,Y,Z, without trigonometric factors.
 public static FT[] Coefficients(int a,int b,int n)
 {FT q=Covector(a,b,n),Q=RaisedClifford(a,b,n),V=One(0,4,1);int s=n*n*(a*a-b*b);var A=CliffordWedge(V,Caa.Gamma1);var C=CliffordWedge(V,Q);var U=Wedge(q,V);var qC=Wedge(q,C);return[U,A,qC,Add(Scale(A,s),Scale(qC,-1)),Wedge(q,Q),Wedge(One(4,0,1),Q)];}
 public static FT ExplicitE(int a,int b,int n)
 {int nn=n*n;var t=Add(Add(One(1,5,nn*b*b),One(1,132,nn*a*b)),Add(One(128,5,nn*a*b),One(128,132,nn*a*a)));for(int j=0;j<14;j++)if(j!=0&&j!=2&&j!=7)t=Add(t,One(1<<j,4^(1<<j),nn*(a*a-b*b)*WordSign(4,1<<j)));return t;}
 public static FT[] ExpectedAdjoints(FT q,FT Q,FT[] coefficient,int s)
 {FT vflat=One(4,0,1);var firstU=Scale(Wedge(q,coefficient[1]),2);var firstA=Scale(Wedge(vflat,Caa.Gamma1),2);var firstQC=Scale(Wedge(Wedge(vflat,q),Q),2);var firstE=Scale(Wedge(vflat,Add(Scale(Caa.Gamma1,s),Scale(Wedge(q,Q),-1))),2);var firstY=Scale(Wedge(q,CliffordWedge(Q,Caa.Gamma1)),2);return[firstU,firstA,firstQC,firstE,firstY];}
 public static int[] CandidateBlades(int form,int input)=>input switch{0=>[form^1^4,form^128^4],1=>[form^4],2 or 3=>[form^4,form^4^1^128],4=>[form,form^1^128],_=>throw new ArgumentOutOfRangeException(nameof(input))};
 public static FT RealProbe(int form,int blade,bool sine)=>Scale(Trig(0,sine,form,blade),AdjointSign(blade)==-1?1:Scalar.I);
 public static FT Reconstruct(FT input,int kind,Action<bool,bool> probe)
 {bool sine=kind is 1 or 2 or 3;var actual=Caa.AdjointLiteral(input);var reconstructed=new FT();var positions=new HashSet<(int Form,int Blade)>();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++){int form=(1<<a)|(1<<b);foreach(int blade in CandidateBlades(form,kind)){positions.Add((form,blade));var f=RealProbe(form,blade,sine);Rational norm=Pair(f,f),left=Pair(Caa.ForwardStages(f,true)[7],input);if(norm==0)throw new InvalidOperationException("nondegenerate candidate probe");bool pass=HAnti(f)&&Typed(f,2)&&left==Pair(f,actual);probe(pass,left!=0);reconstructed=Add(reconstructed,Scale(f,new Scalar(left*new Rational(norm.Denominator,norm.Numerator),0)));}}if(actual.Keys.Any(k=>!positions.Contains((k.Form,k.Blade))))throw new InvalidOperationException("adjoint escaped proved support");return reconstructed;}
 public static int Positions(FT f)=>f.Keys.Select(k=>(k.Form,k.Blade)).Distinct().Count();
 public static bool FourierReal(FT f,int degree)=>Typed(f,degree)&&HAnti(f)&&f.Keys.All(k=>System.Math.Abs(k.K0)==1&&k.K1==0);
}
