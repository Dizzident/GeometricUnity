using static Algebra;
using static Fourier;
using static Adjoint;
using static Caa;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Isotropic
{
 public static FT Unit(int a,int b)=>One(1<<a,1<<b,1);
 public static Rational Trace(FT v){Rational r=0;for(int i=0;i<14;i++)if(v.TryGetValue((1<<i,1<<i,0,0),out var z))r+=z.Real;return r;}
 public static FT Transpose(FT v)
 {var r=new FT();foreach(var q in v){if(Degree(q.Key.Form)!=1||Degree(q.Key.Blade)!=1||q.Key.K0!=0||q.Key.K1!=0||q.Value.Imaginary!=0)throw new InvalidOperationException("constant vector tensor required");int metric=(Degree(q.Key.Form&0x3f80)+Degree(q.Key.Blade&0x3f80))%2==0?1:-1;Put(r,(q.Key.Blade,q.Key.Form,0,0),q.Value*metric);}return r;}
 public static FT[] OracleLegs(FT v)
 {var trace=Scale(Gamma1,new Scalar(Trace(v),0));return[Scale(Add(trace,Scale(v,-1)),48),Scale(Add(trace,Scale(Transpose(v),-1)),48),Scale(Add(trace,Scale(v,-1)),48)];}
 public static FT[] ActualLegs(FT v,FT kadS)
 {var s=Gamma1;return[Forward(Product(s,v,'C')),DQAdjoint(v,kadS),DQAdjoint(s,AdjointLiteral(v))];}
 public static FT Potential(FT[] legs)=>Scale(Add(Add(legs[0],legs[1]),legs[2]),new Scalar(new Rational(1,3),0));
 public static (FT First,FT Second) OracleAdjoint(int a,int b)
 {var first=new FT();for(int j=0;j<14;j++)if(j!=a&&j!=b)Put(first,((1<<a)|(1<<j),(1<<b)^(1<<j),0,0),2*Shuffle(1<<a,1<<j)*BladeSign(1<<b,1<<j));return(first,a==b?Scale(Gamma2,-2):new FT());}
 public static FT Reconstruct(int a,int b,FT actual,Action<bool,bool> check)
 {var y=Unit(a,b);var result=new FT();for(int i=0;i<14;i++)for(int j=i+1;j<14;j++){int form=(1<<i)|(1<<j),blade=form^(1<<a)^(1<<b);Scalar phase=AdjointSign(blade)==-1?1:Scalar.I;var f=One(form,blade,phase);var norm=Pair(f,f);if(norm==0)throw new InvalidOperationException("singular probe");var value=Pair(Forward(f),y);var coefficient=value*new Rational(norm.Denominator,norm.Numerator);result=Add(result,Scale(f,new Scalar(coefficient,0)));check(value==Pair(f,actual)&&HAnti(f),coefficient!=0);}return result;}
 public static FT ApplyColumns(FT v,FT[] columns)
 {var result=new FT();for(int a=0;a<14;a++)for(int b=0;b<14;b++)if(v.TryGetValue((1<<a,1<<b,0,0),out var z))result=Add(result,Scale(columns[14*a+b],z));return result;}
 public static FP Map(FP p,Func<FT,FT> map){var r=new FP(p.Order);foreach(var q in p.Coefficients)r.Put(q.Key,map(q.Value));return r;}
 public static FP AdjProduct(FP a,FP b)
 {if(a.Order!=b.Order)throw new ArgumentException("order");var r=new FP(a.Order);foreach(var q in a.Coefficients)foreach(var z in b.Coefficients)if((q.Key+z.Key).Degree<=r.Order)r.Put(q.Key+z.Key,DQAdjoint(q.Value,z.Value));return r;}
 public static FP Shift(FP p,Monomial m){var r=new FP(p.Order);foreach(var q in p.Coefficients)r.Put(q.Key+m,q.Value);return r;}
 public static SP Shift(SP p,Monomial m){var r=new SP(p.Order);foreach(var q in Monomial.Menu(p.Order))r.Put(q+m,p.Get(q));return r;}
 public static SP Sum(SP a,SP b){var r=new SP(a.Order);foreach(var m in Monomial.Menu(a.Order))r.Put(m,a.Get(m)+b.Get(m));return r;}
 public static Rational Pow(Rational x,int n){Rational r=1;for(int i=0;i<n;i++)r*=x;return r;}
 public static Rational Evaluate(SP p,Rational x,Rational y,Rational z){Rational r=0;foreach(var m in Monomial.Menu(p.Order))r+=p.Get(m)*Pow(x,m.X)*Pow(y,m.Y)*Pow(z,m.Z);return r;}
 public static FT Evaluate(FP p,Rational x,Rational y,Rational z){var r=new FT();foreach(var q in p.Coefficients)r=Add(r,Scale(q.Value,new Scalar(Pow(x,q.Key.X)*Pow(y,q.Key.Y)*Pow(z,q.Key.Z),0)));return r;}
 public static IEnumerable<(string Sector,FT Vector,int Coefficient)> Sectors()
 {yield return("trace",Gamma1,624);for(int i=1;i<14;i++)yield return("symmetric-traceless",Add(Unit(i,i),Scale(Unit(0,0),-1)),-48);for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)yield return("symmetric-traceless",Add(Unit(a,b),Scale(Unit(b,a),Sigma(a)*Sigma(b))),-48);for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)yield return("metric-skew",Add(Unit(a,b),Scale(Unit(b,a),-Sigma(a)*Sigma(b))),-16);}
}
