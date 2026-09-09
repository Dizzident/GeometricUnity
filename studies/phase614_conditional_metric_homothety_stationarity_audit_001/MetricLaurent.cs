using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class MetricResources
{
 public static int MaximumMonomials{get;private set;}
 public static int MaximumAbsoluteExponent{get;private set;}
 public static void Track(IEnumerable<int> exponents){var a=exponents.ToArray();MaximumMonomials=System.Math.Max(MaximumMonomials,a.Length);MaximumAbsoluteExponent=System.Math.Max(MaximumAbsoluteExponent,a.Select(System.Math.Abs).DefaultIfEmpty().Max());}
 public static Rational Pow(Rational x,int e){if(e<0){x=new Rational(x.Denominator,x.Numerator);e=-e;}Rational r=1;for(int i=0;i<e;i++)r*=x;return r;}
}

// Exact finite Laurent polynomials; no truncation or sampled coefficient fit.
internal sealed class LP
{
 public SortedDictionary<int,FT> Coefficients{get;}=new();
 public LP(IEnumerable<KeyValuePair<int,FT>> coefficients){foreach(var x in coefficients)if(x.Value.Count!=0)Coefficients[x.Key]=new(x.Value);MetricResources.Track(Coefficients.Keys);}
 public static LP Zero=>new([]);
 public static LP Mono(FT x,int exponent=0)=>new(new Dictionary<int,FT>{{exponent,x}});
 public static LP Add(LP a,LP b){var t=new Dictionary<int,FT>(a.Coefficients);foreach(var x in b.Coefficients)t[x.Key]=Fourier.Add(t.GetValueOrDefault(x.Key)??new(),x.Value);return new(t);}
 public static LP Scale(LP a,Scalar s)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key,Fourier.Scale(x.Value,s))));
 public static LP Shift(LP a,int e)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key+e,x.Value)));
 public static LP Product(LP a,LP b,char kind='W',bool naive=false){var t=new Dictionary<int,FT>();foreach(var x in a.Coefficients)foreach(var y in b.Coefficients){int e=x.Key+y.Key;FT xy=naive?NaiveProduct(x.Value,y.Value,kind):Fourier.Product(x.Value,y.Value,kind);t[e]=Fourier.Add(t.GetValueOrDefault(e)??new(),xy);}return new(t);}
 public static LP Star(LP a,int degree){if(!Typed(a,degree))throw new InvalidOperationException("metric star domain");return new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key+14-2*degree,Fourier.Star(x.Value))));}
 public static LP StarAdjoint(LP a,int forwardDegree){if(!Typed(a,14-forwardDegree))throw new InvalidOperationException("metric star transpose domain");return new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key+2*forwardDegree-14,Adjoint.StarAdjoint(x.Value,forwardDegree))));}
 public static LP BracketAdjoint(LP phi,LP y,int phiDegree,char kind){if(!Typed(phi,phiDegree))throw new InvalidOperationException("metric bracket degree");var t=new Dictionary<int,FT>();foreach(var a in phi.Coefficients)foreach(var b in y.Coefficients){int e=a.Key+b.Key-2*phiDegree;t[e]=Fourier.Add(t.GetValueOrDefault(e)??new(),Adjoint.BracketAdjoint(a.Value,b.Value,kind));}return new(t);}
 public static LP D(LP a)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key,Fourier.D(x.Value))));
 public static LP Ddag(LP a)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key-2,Adjoint.DAdjoint(x.Value))));
 public static LP Derivative(LP a)=>new(a.Coefficients.Where(x=>x.Key!=0).Select(x=>KeyValuePair.Create(x.Key-1,Fourier.Scale(x.Value,x.Key))));
 public static FT Evaluate(LP a,Rational lambda)=>a.Coefficients.Aggregate(new FT(),(t,x)=>Fourier.Add(t,Fourier.Scale(x.Value,new Scalar(MetricResources.Pow(lambda,x.Key),0))));
 public static bool Equal(LP a,LP b)=>a.Coefficients.Count==b.Coefficients.Count&&a.Coefficients.All(x=>b.Coefficients.TryGetValue(x.Key,out var y)&&Fourier.Equal(x.Value,y));
 public static bool Typed(LP a,int degree)=>a.Coefficients.Values.All(t=>Fourier.Typed(t,degree));
 public static bool RealConstant(LP a,int degree)=>Typed(a,degree)&&a.Coefficients.Values.All(t=>HAnti(t)&&t.Keys.All(k=>k.K0==0&&k.K1==0));
 public static LP[] ForwardStages(LP f,bool freezePhi=false,bool naive=false)
 {var p1=Mono(Caa.Gamma1,freezePhi?0:1);var p2=Mono(Caa.Gamma2,freezePhi?0:2);var sf=Star(f,2);var one=Product(p1,sf,'C',naive);var inner=Product(p2,sf,'A',naive);var zero=Star(inner,14);var outer=Product(p1,zero,'A',naive);var upper=Add(one,Scale(Star(outer,1),Fourier.Half*-1));var lower=Star(upper,13);LP[] stages=[f,sf,one,inner,zero,outer,upper,lower];int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++)if(!RealConstant(stages[i],degrees[i]))throw new InvalidOperationException("real typed metric forward stages");return stages;}
 public static LP K(LP f)=>ForwardStages(f)[7];
 public static LP[] ReverseStages(LP y)
 {var p1=Mono(Caa.Gamma1,1);var p2=Mono(Caa.Gamma2,2);var upper=StarAdjoint(y,13);var first=StarAdjoint(BracketAdjoint(p1,upper,1,'C'),2);var outerOne=StarAdjoint(upper,1);var zero=BracketAdjoint(p1,outerOne,1,'A');var top=StarAdjoint(zero,14);var twelve=BracketAdjoint(p2,top,2,'A');var second=Scale(StarAdjoint(twelve,2),Fourier.Half*-1);LP[] stages=[y,upper,first,outerOne,zero,top,twelve,second];int[] degrees=[1,13,2,1,0,14,12,2];for(int i=0;i<8;i++)if(!RealConstant(stages[i],degrees[i]))throw new InvalidOperationException("real typed metric reverse stages");return stages;}
 public static LP Kad(LP y){var stages=ReverseStages(y);return Add(stages[2],stages[7]);}
 public static LP DQdag(LP t,LP y)=>BracketAdjoint(t,y,1,'C');
 public static SP Pair(LP a,LP b,int degree,bool density){if(!Typed(a,degree)||!Typed(b,degree))throw new InvalidOperationException("metric pairing degree");var t=new Dictionary<int,Rational>();foreach(var x in a.Coefficients)foreach(var y in b.Coefficients){int e=x.Key+y.Key-2*degree+(density?14:0);t[e]=t.GetValueOrDefault(e)+Fourier.Pair(x.Value,y.Value);}return new(t);}
 public static SP Top(LP a){if(!Typed(a,14))throw new InvalidOperationException("metric top degree");return new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key,Fourier.Top(x.Value))));}
 public static object[] Terms(LP a)=>a.Coefficients.Select(x=>(object)new{exponent=x.Key,tensor=Fourier.Terms(x.Value)}).ToArray();
}

internal sealed class SP
{
 public SortedDictionary<int,Rational> Coefficients{get;}=new();
 public SP(IEnumerable<KeyValuePair<int,Rational>> coefficients){foreach(var x in coefficients)if(x.Value!=0)Coefficients[x.Key]=x.Value;MetricResources.Track(Coefficients.Keys);}
 public static SP Mono(Rational c,int exponent=0)=>new(new Dictionary<int,Rational>{{exponent,c}});
 public static SP Add(SP a,SP b){var t=new Dictionary<int,Rational>(a.Coefficients);foreach(var x in b.Coefficients)t[x.Key]=t.GetValueOrDefault(x.Key)+x.Value;return new(t);}
 public static SP Scale(SP a,Rational s)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key,x.Value*s)));
 public static SP Shift(SP a,int e)=>new(a.Coefficients.Select(x=>KeyValuePair.Create(x.Key+e,x.Value)));
 public static SP Derivative(SP a)=>new(a.Coefficients.Where(x=>x.Key!=0).Select(x=>KeyValuePair.Create(x.Key-1,x.Value*x.Key)));
 public static Rational Evaluate(SP a,Rational lambda)=>a.Coefficients.Aggregate((Rational)0,(t,x)=>t+x.Value*MetricResources.Pow(lambda,x.Key));
 public static bool Equal(SP a,SP b)=>a.Coefficients.Count==b.Coefficients.Count&&a.Coefficients.All(x=>b.Coefficients.TryGetValue(x.Key,out var y)&&x.Value==y);
 public static object[] Terms(SP a)=>a.Coefficients.Select(x=>(object)new{exponent=x.Key,coefficient=x.Value.ToString()}).ToArray();
}
