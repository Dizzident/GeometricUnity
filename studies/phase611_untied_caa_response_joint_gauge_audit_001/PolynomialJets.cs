using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

readonly record struct Monomial(int X,int Y,int Z)
{
 public int Degree=>X+Y+Z;
 public static Monomial operator +(Monomial a,Monomial b)=>new(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
 public int Exponent(int axis)=>axis==0?X:axis==1?Y:Z;
 public Monomial Lower(int axis)=>new(X-(axis==0?1:0),Y-(axis==1?1:0),Z-(axis==2?1:0));
 public static IEnumerable<Monomial> Menu(int order){for(int x=0;x<=order;x++)for(int y=0;y<=order-x;y++)for(int z=0;z<=order-x-y;z++)yield return new(x,y,z);}
}

// Exact finite jets; every multiplication discards only degrees above the
// declared order, never tensor components inside a retained coefficient.
internal sealed class FP(int order)
{
 public int Order{get;}=order;public Dictionary<Monomial,FT> Coefficients{get;}=new();
 public FT Get(Monomial m)=>Coefficients.TryGetValue(m,out var v)?v:new();
 public bool Zero=>Coefficients.Count==0;
 public void Put(Monomial m,FT f){if(m.Degree>Order)return;var sum=Fourier.Add(Get(m),f);if(sum.Count==0)Coefficients.Remove(m);else Coefficients[m]=sum;}
 public static FP Constant(int order,FT f){var p=new FP(order);p.Put(new(0,0,0),f);return p;}
 public static FP Variable(int order,int axis,FT f){var p=new FP(order);p.Put(new(axis==0?1:0,axis==1?1:0,axis==2?1:0),f);return p;}
 public static FP Add(FP a,FP b){Check(a,b);var p=new FP(a.Order);foreach(var q in a.Coefficients)p.Put(q.Key,q.Value);foreach(var q in b.Coefficients)p.Put(q.Key,q.Value);return p;}
 public static FP Scale(FP a,Scalar s){var p=new FP(a.Order);foreach(var q in a.Coefficients)p.Put(q.Key,Fourier.Scale(q.Value,s));return p;}
 public static FP Product(FP a,FP b,char kind='W')
 {Check(a,b);var p=new FP(a.Order);foreach(var q in a.Coefficients)foreach(var r in b.Coefficients){var m=q.Key+r.Key;if(m.Degree<=a.Order)p.Put(m,Fourier.Product(q.Value,r.Value,kind));}return p;}
 public static FP Star(FP a){var p=new FP(a.Order);foreach(var q in a.Coefficients)p.Put(q.Key,Fourier.Star(q.Value));return p;}
 public static FP D(FP a,int t){var p=new FP(a.Order);foreach(var q in a.Coefficients)p.Put(q.Key,Caa.Derivative(q.Value,t));return p;}
 public static FP HAdjoint(FP a){var p=new FP(a.Order);foreach(var q in a.Coefficients)p.Put(q.Key,Fourier.HAdjoint(q.Value));return p;}
 public static bool Typed(FP a,int degree)=>a.Coefficients.Values.All(v=>Fourier.Typed(v,degree));
 public static bool Anti(FP a)=>a.Coefficients.Values.All(Fourier.HAnti);
 public static FP Chain(FP f,FP phi1,FP phi2)
 {var sf=Star(f);var one=Product(phi1,sf,'C');var inner=Product(phi2,sf,'A');var zero=Star(inner);var outer=Product(phi1,zero,'A');var upper=Add(one,Scale(Star(outer),Fourier.Half*-1));var lower=Star(upper);foreach(var (v,d) in new[]{(phi1,1),(phi2,2),(f,2),(sf,12),(one,13),(inner,14),(zero,0),(outer,1),(upper,13),(lower,1)})if(!Typed(v,d)||!Anti(v))throw new InvalidOperationException("CAA jet type/real form");return lower;}
 public static SP Pair(FP a,FP b)
 {Check(a,b);if(!Anti(a)||!Anti(b))throw new InvalidOperationException("Grouped real-form pairing required");var p=new SP(a.Order);foreach(var q in a.Coefficients)foreach(var r in b.Coefficients){var m=q.Key+r.Key;if(m.Degree<=a.Order)p.Put(m,Fourier.Pair(q.Value,r.Value));}return p;}
 public static bool Equal(FP a,FP b)=>a.Order==b.Order&&a.Coefficients.Count==b.Coefficients.Count&&a.Coefficients.All(q=>Fourier.Equal(q.Value,b.Get(q.Key)));
 private static void Check(FP a,FP b){if(a.Order!=b.Order)throw new ArgumentException("jet orders differ");}
 public object[] Terms()=>Coefficients.OrderBy(q=>q.Key.X).ThenBy(q=>q.Key.Y).ThenBy(q=>q.Key.Z).Select(q=>(object)new{x=q.Key.X,y=q.Key.Y,z=q.Key.Z,tensor=Fourier.Terms(q.Value)}).ToArray();
}

internal sealed class SP(int order)
{
 public int Order{get;}=order;private readonly Dictionary<Monomial,Rational> coefficients=new();
 public Rational Get(Monomial m)=>coefficients.GetValueOrDefault(m);
 public void Put(Monomial m,Rational value){if(m.Degree>Order)return;Rational sum=Get(m)+value;if(sum==0)coefficients.Remove(m);else coefficients[m]=sum;}
 public SP Derivative(int axis){var p=new SP(Order);foreach(var q in coefficients){int n=q.Key.Exponent(axis);if(n>0)p.Put(q.Key.Lower(axis),q.Value*n);}return p;}
 public SP Scale(Rational s){var p=new SP(Order);foreach(var q in coefficients)p.Put(q.Key,q.Value*s);return p;}
 public bool Equal(SP p)=>Order==p.Order&&Monomial.Menu(Order).All(m=>Get(m)==p.Get(m));
 public object[] Terms()=>coefficients.OrderBy(q=>q.Key.X).ThenBy(q=>q.Key.Y).ThenBy(q=>q.Key.Z).Select(q=>(object)new{x=q.Key.X,y=q.Key.Y,z=q.Key.Z,value=q.Value.ToString()}).ToArray();
}
