using static Algebra;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Fourier
{
 public const int Full=16383;
 public static Scalar Half=>new(new Rational(1,2),0);
 public static long CoefficientProducts{get;private set;}
 public static int LargestTensor{get;private set;}
 public static void Put(FT a,(int Form,int Blade,int K0,int K1) k,Scalar c){if(c.IsZero)return;Scalar s=a.GetValueOrDefault(k)+c;if(s.IsZero)a.Remove(k);else a[k]=s;LargestTensor=System.Math.Max(LargestTensor,a.Count);}
 public static FT One(int form,int blade,Scalar c){var a=new FT();Put(a,(form,blade,0,0),c);return a;}
 public static FT Trig(int axis,bool sine,int form=0,int blade=0)
 {var a=new FT();Put(a,(form,blade,axis==0?1:0,axis==1?1:0),sine?Scalar.I*Half*-1:Half);Put(a,(form,blade,axis==0?-1:0,axis==1?-1:0),sine?Scalar.I*Half:Half);return a;}
 public static FT Add(FT a,FT b){var t=new FT(a);foreach(var q in b)Put(t,q.Key,q.Value);return t;}
 public static FT Scale(FT a,Scalar c){var t=new FT();foreach(var q in a)Put(t,q.Key,q.Value*c);return t;}
 // Group only by form; prune known-zero form overlaps and matrix coefficients.
 // No covariance identity or target coefficient is used by this kernel.
 public static FT Product(FT a,FT b,char kind='W')
 {var t=new FT();foreach(var ga in a.GroupBy(q=>q.Key.Form))foreach(var gb in b.GroupBy(q=>q.Key.Form))
  {if((ga.Key&gb.Key)!=0)continue;int fs=Shuffle(ga.Key,gb.Key);foreach(var q in ga)foreach(var r in gb)
   {int ab=BladeSign(q.Key.Blade,r.Key.Blade),factor=kind=='W'?ab:kind=='C'?ab-BladeSign(r.Key.Blade,q.Key.Blade):ab+BladeSign(r.Key.Blade,q.Key.Blade);if(factor==0)continue;
    CoefficientProducts++;Scalar c=q.Value*r.Value*(fs*factor);if(kind=='A')c*=Scalar.I;Put(t,(ga.Key|gb.Key,q.Key.Blade^r.Key.Blade,q.Key.K0+r.Key.K0,q.Key.K1+r.Key.K1),c);}}
  return t;}
 public static FT NaiveProduct(FT a,FT b,char kind)
 {var t=new FT();foreach(var q in a)foreach(var r in b)if((q.Key.Form&r.Key.Form)==0)
  {int ab=WordSign(q.Key.Blade,r.Key.Blade),ba=WordSign(r.Key.Blade,q.Key.Blade);Scalar c=q.Value*r.Value*(Shuffle(q.Key.Form,r.Key.Form)*(kind=='W'?ab:kind=='C'?ab-ba:ab+ba));if(kind=='A')c*=Scalar.I;Put(t,(q.Key.Form|r.Key.Form,q.Key.Blade^r.Key.Blade,q.Key.K0+r.Key.K0,q.Key.K1+r.Key.K1),c);}return t;}
 public static FT Star(FT a){var t=new FT();foreach(var q in a)Put(t,(Full^q.Key.Form,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*HodgeSign(q.Key.Form));return t;}
 public static FT Omega(FT a){var t=new FT();foreach(var q in a)Put(t,(q.Key.Form,Full^q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*BladeSign(Full,q.Key.Blade));return t;}
 public static FT HAdjoint(FT a){var t=new FT();foreach(var q in a)Put(t,(q.Key.Form,q.Key.Blade,-q.Key.K0,-q.Key.K1),q.Value.Conjugate()*AdjointSign(q.Key.Blade));return t;}
 public static FT Partial(FT a,int axis){var t=new FT();foreach(var q in a)Put(t,q.Key,q.Value*Scalar.I*(axis==0?q.Key.K0:q.Key.K1));return t;}
 public static FT D(FT a)=>Add(Product(One(1,0,1),Partial(a,0)),Product(One(2,0,1),Partial(a,1)));
 public static FT Covariant(FT b,FT t)=>Add(D(t),Add(Product(b,t),Product(t,b)));
 public static FT Conjugate(FT inverse,FT t,FT epsilon)=>Product(Product(inverse,t),epsilon);
 public static bool Equal(FT a,FT b)=>a.Count==b.Count&&a.All(q=>b.TryGetValue(q.Key,out var c)&&q.Value==c);
 public static bool Typed(FT a,int degree)=>a.Keys.All(k=>Degree(k.Form)==degree);
 public static bool HAnti(FT a)=>Equal(HAdjoint(a),Scale(a,-1));
 public static Rational Pair(FT a,FT b)
 {Scalar s=0;foreach(var q in a){var key=(q.Key.Form,q.Key.Blade,-q.Key.K0,-q.Key.K1);if(b.TryGetValue(key,out var c))s+=q.Value*c*(-BladeSign(q.Key.Blade,q.Key.Blade)*(Degree(q.Key.Form&0x3f80)%2==0?1:-1));}if(s.Imaginary!=0)throw new InvalidOperationException("Nonreal pairing");return s.Real;}
 public static Rational Top(FT a){if(!Typed(a,14))throw new InvalidOperationException("Top degree");Scalar c=a.GetValueOrDefault((Full,0,0,0));if(c.Imaginary!=0)throw new InvalidOperationException("Nonreal top");return c.Real*-1;}
 public static FT Chain(FT f,FT p1,FT p2,bool first)
 {FT sf=Star(f),one=first?Product(p1,sf,'C'):new(),inner=Product(p2,sf,'A'),zero=Star(inner),outer=Product(p1,zero,'C');FT upper=Add(one,Scale(Star(outer),Half*-1)),lower=Star(upper);
  if(!Typed(f,2)||!Typed(sf,12)||!Typed(one,13)||!Typed(inner,14)||!Typed(zero,0)||!Typed(outer,1)||!Typed(upper,13)||!Typed(lower,1))throw new InvalidOperationException("Chain form types");return lower;}
 public static object[] Terms(FT a)=>a.OrderBy(q=>q.Key).Select(q=>(object)new{form=q.Key.Form,blade=q.Key.Blade,k0=q.Key.K0,k1=q.Key.K1,real=q.Value.Real.ToString(),imaginary=q.Value.Imaginary.ToString()}).ToArray();
 // Formal polynomial operations, coefficients in Fourier/Clifford forms.
 public static FT[] PAdd(FT[] a,FT[] b)=>Enumerable.Range(0,System.Math.Max(a.Length,b.Length)).Select(i=>Add(i<a.Length?a[i]:new(),i<b.Length?b[i]:new())).ToArray();
 public static FT[] PScale(FT[] a,Scalar s)=>a.Select(v=>Scale(v,s)).ToArray();
 public static FT[] PMul(FT[] a,FT[] b,char kind='W')
 {var t=Enumerable.Range(0,a.Length+b.Length-1).Select(_=>new FT()).ToArray();for(int i=0;i<a.Length;i++)for(int j=0;j<b.Length;j++)t[i+j]=Add(t[i+j],Product(a[i],b[j],kind));return t;}
 public static FT[] PD(FT[] a)=>a.Select(D).ToArray();
 public static bool PConstant(FT[] a,FT c)=>a.Length>0&&Equal(a[0],c)&&a.Skip(1).All(v=>v.Count==0);
 public static Rational[] PPair(FT[] a,FT[] b)
 {var t=new Rational[a.Length+b.Length-1];for(int i=0;i<a.Length;i++)for(int j=0;j<b.Length;j++)t[i+j]+=Pair(a[i],b[j]);return t;}
 public static FT[] PChain(FT[] f,FT p1,FT p2,bool first)=>f.Select(v=>Chain(v,p1,p2,first)).ToArray();
 public static Rational[] RAdd(Rational[] a,Rational[] b)=>Enumerable.Range(0,System.Math.Max(a.Length,b.Length)).Select(i=>(i<a.Length?a[i]:new Rational(0))+(i<b.Length?b[i]:new Rational(0))).ToArray();
 public static Rational[] RScale(Rational[] a,Rational s)=>a.Select(v=>v*s).ToArray();
 public static bool REqual(Rational[] a,Rational[] b)=>Enumerable.Range(0,System.Math.Max(a.Length,b.Length)).All(i=>(i<a.Length?a[i]:new Rational(0))==(i<b.Length?b[i]:new Rational(0)));
}

// Independent first-order dual-number construction: all products differentiate
// their actual inputs, including both contraction tensors, B and torsion.
readonly record struct Jet(FT Value,FT Delta)
{
 public static Jet Fixed(FT v)=>new(v,new());
 public static Jet Add(Jet a,Jet b)=>new(Fourier.Add(a.Value,b.Value),Fourier.Add(a.Delta,b.Delta));
 public static Jet Scale(Jet a,Scalar c)=>new(Fourier.Scale(a.Value,c),Fourier.Scale(a.Delta,c));
 public static Jet Product(Jet a,Jet b,char kind='W')=>new(Fourier.Product(a.Value,b.Value,kind),Fourier.Add(Fourier.Product(a.Delta,b.Value,kind),Fourier.Product(a.Value,b.Delta,kind)));
 public static Jet D(Jet a)=>new(Fourier.D(a.Value),Fourier.D(a.Delta));
 public static Jet Star(Jet a)=>new(Fourier.Star(a.Value),Fourier.Star(a.Delta));
 public static Jet Chain(Jet f,Jet p1,Jet p2,bool first)
 {var sf=Star(f);var one=first?Product(p1,sf,'C'):Fixed(new());var inner=Product(p2,sf,'A');var zero=Star(inner);var outer=Product(p1,zero,'C');var upper=Add(one,Scale(Star(outer),Fourier.Half*-1));var lower=Star(upper);
  foreach(var (jet,degree) in new[]{(f,2),(sf,12),(one,13),(inner,14),(zero,0),(outer,1),(upper,13),(lower,1)})if(!Fourier.Typed(jet.Value,degree)||!Fourier.Typed(jet.Delta,degree))throw new InvalidOperationException("Jet chain intermediate type");return lower;}
 public static (Rational Value,Rational Delta) Pair(Jet a,Jet b)=>(Fourier.Pair(a.Value,b.Value),Fourier.Pair(a.Delta,b.Value)+Fourier.Pair(a.Value,b.Delta));
}
