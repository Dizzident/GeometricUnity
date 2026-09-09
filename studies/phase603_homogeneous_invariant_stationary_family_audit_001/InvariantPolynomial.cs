using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Sparse five-variable exact polynomials, ordered (a,b,c,gamma,kappa).
// Tensor products and adjoints use the immutable full source implementation.
internal readonly record struct Mon(int A,int B,int C,int G,int K)
{
 public int this[int i]=>i switch{0=>A,1=>B,2=>C,3=>G,4=>K,_=>throw new ArgumentOutOfRangeException(nameof(i))};
 public static Mon Unit(int i)=>new(i==0?1:0,i==1?1:0,i==2?1:0,i==3?1:0,i==4?1:0);
 public static Mon operator +(Mon x,Mon y)=>new(x.A+y.A,x.B+y.B,x.C+y.C,x.G+y.G,x.K+y.K);
 public Mon Lower(int i)=>new(A-(i==0?1:0),B-(i==1?1:0),C-(i==2?1:0),G-(i==3?1:0),K-(i==4?1:0));
 public string Key=>$"{A},{B},{C},{G},{K}";
}
internal sealed class Poly
{
 public readonly Dictionary<Mon,Rational> Terms=new();
 public static Poly Var(int i){var p=new Poly();p.Put(Mon.Unit(i),1);return p;}
 public static implicit operator Poly(long n){var p=new Poly();p.Put(default,n);return p;}
 public void Put(Mon m,Rational r){Terms.TryGetValue(m,out var old);var v=old+r;if(v==0)Terms.Remove(m);else Terms[m]=v;}
 public static Poly operator +(Poly a,Poly b){var p=new Poly();foreach(var q in a.Terms)p.Put(q.Key,q.Value);foreach(var q in b.Terms)p.Put(q.Key,q.Value);return p;}
 public static Poly operator -(Poly a,Poly b)=>a+b.Scale(-1);
 public static Poly operator *(Poly a,Poly b){var p=new Poly();foreach(var x in a.Terms)foreach(var y in b.Terms)p.Put(x.Key+y.Key,x.Value*y.Value);return p;}
 public Poly Scale(Rational r){var p=new Poly();foreach(var q in Terms)p.Put(q.Key,q.Value*r);return p;}
 public Poly Derivative(int i){var p=new Poly();foreach(var q in Terms)if(q.Key[i]>0)p.Put(q.Key.Lower(i),q.Value*q.Key[i]);return p;}
 public Poly Substitute(Poly[] values){if(values.Length!=5)throw new ArgumentException("five substitutions");Poly p=0;foreach(var q in Terms){Poly term=((Poly)1).Scale(q.Value);for(int i=0;i<5;i++)for(int j=0;j<q.Key[i];j++)term*=values[i];p+=term;}return p;}
 public bool Same(Poly p)=>Terms.Count==p.Terms.Count&&Terms.All(q=>p.Terms.TryGetValue(q.Key,out var r)&&r==q.Value);
 public bool Zero=>Terms.Count==0;
 public object[] Text()=>Terms.OrderBy(q=>q.Key.Key,StringComparer.Ordinal).Select(q=>(object)new{powers=q.Key.Key,coefficient=q.Value.ToString()}).ToArray();
}
internal sealed class TensorPoly
{
 public readonly Dictionary<Mon,FT> Terms=new();
 public static TensorPoly From(FT t){var p=new TensorPoly();p.Put(default,t);return p;}
 public void Put(Mon m,FT t){var v=Terms.TryGetValue(m,out var old)?Fourier.Add(old,t):Fourier.Add(new FT(),t);if(v.Count==0)Terms.Remove(m);else Terms[m]=v;}
 public static TensorPoly operator +(TensorPoly a,TensorPoly b){var p=new TensorPoly();foreach(var q in a.Terms)p.Put(q.Key,q.Value);foreach(var q in b.Terms)p.Put(q.Key,q.Value);return p;}
 public static TensorPoly operator -(TensorPoly a,TensorPoly b)=>a+b.Scale(-1);
 public static TensorPoly operator *(Poly a,TensorPoly b){var p=new TensorPoly();foreach(var x in a.Terms)foreach(var y in b.Terms)p.Put(x.Key+y.Key,Fourier.Scale(y.Value,new Scalar(x.Value,0)));return p;}
 public TensorPoly Scale(Rational r)=>Map(t=>Fourier.Scale(t,new Scalar(r,0)));
 public TensorPoly Map(Func<FT,FT> f){var p=new TensorPoly();foreach(var q in Terms)p.Put(q.Key,f(q.Value));return p;}
 public static TensorPoly Bilinear(TensorPoly a,TensorPoly b,Func<FT,FT,FT> f){var p=new TensorPoly();foreach(var x in a.Terms)foreach(var y in b.Terms)p.Put(x.Key+y.Key,f(x.Value,y.Value));return p;}
 public static Poly Pair(TensorPoly a,TensorPoly b){var p=new Poly();foreach(var x in a.Terms)foreach(var y in b.Terms)p.Put(x.Key+y.Key,Fourier.Pair(x.Value,y.Value));return p;}
 public TensorPoly Derivative(int i){var p=new TensorPoly();foreach(var q in Terms)if(q.Key[i]>0)p.Put(q.Key.Lower(i),Fourier.Scale(q.Value,q.Key[i]));return p;}
 public TensorPoly Substitute(Poly[] v){var p=new TensorPoly();foreach(var q in Terms){var scalar=new Poly();scalar.Put(q.Key,1);p+=scalar.Substitute(v)*From(q.Value);}return p;}
 public bool Same(TensorPoly p)=>Terms.Count==p.Terms.Count&&Terms.All(q=>p.Terms.TryGetValue(q.Key,out var t)&&Fourier.Equal(q.Value,t));
 public bool Zero=>Terms.Count==0;
 public int CoefficientCount=>Terms.Sum(q=>q.Value.Count);
 public bool TypedAnti(int degree)=>Terms.Values.All(t=>Fourier.Typed(t,degree)&&Fourier.HAnti(t)&&t.Keys.All(k=>k.K0==0&&k.K1==0));
 public object[] Text()=>Terms.OrderBy(q=>q.Key.Key,StringComparer.Ordinal).Select(q=>(object)new{powers=q.Key.Key,terms=Fourier.Terms(q.Value)}).ToArray();
}
