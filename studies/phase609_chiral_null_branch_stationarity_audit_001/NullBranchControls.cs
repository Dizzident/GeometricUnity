using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Coordinate fields remain fixed when a metric derivative is requested.
// The orthonormal coframe varies theta'^a=(1+t*m_a)theta^a at t=0.
internal static class NullControls
{
 public static Scalar RealPhase(int blade)=>AdjointSign(blade)==-1?1:Scalar.I;
 public static FT RealBasis(int form,int blade)=>One(form,blade,RealPhase(blade));
 public static FT ProjectTwice(FT x,int h)=>Add(x,Scale(Omega(x),h));
 public static bool Parity(FT x,int parity)=>x.Keys.All(k=>Degree(k.Blade)%2==parity);
 public static bool InNull(FT x,int h)=>Parity(x,1)&&Equal(Omega(x),Scale(x,h));
 public static FT CovariantAdjoint(FT b,FT y)=>Add(Adjoint.DAdjoint(y),Adjoint.BracketAdjoint(b,y,'C'));
 public static FT NaiveChain(FT f,FT p1,FT p2,bool first)
 {var sf=Star(f);var one=first?NaiveProduct(p1,sf,'C'):new FT();var inner=NaiveProduct(p2,sf,'A');var outer=NaiveProduct(p1,Star(inner),'C');return Star(Add(one,Scale(Star(outer),Fourier.Half*-1)));}
 public static FT Weight(FT a,int[] weights,bool starWeight)
 {
  if(weights.Length!=14)throw new ArgumentException("14 metric weights");var r=new FT();int trace=weights.Sum();foreach(var q in a){int sum=Enumerable.Range(0,14).Where(i=>(q.Key.Form&(1<<i))!=0).Sum(i=>weights[i]);Put(r,q.Key,q.Value*(starWeight?trace-2*sum:sum));}return r;
 }
 public static Jet MetricStar(Jet a,int[] weights)=>new(Star(a.Value),Add(Star(a.Delta),Star(Weight(a.Value,weights,true))));
 public static Jet MetricChain(Jet f,Jet p1,Jet p2,int[] weights,bool first)
 {
  var sf=MetricStar(f,weights);var one=first?Jet.Product(p1,sf,'C'):Jet.Fixed(new());var inner=Jet.Product(p2,sf,'A');var zero=MetricStar(inner,weights);var outer=Jet.Product(p1,zero,'C');var upper=Jet.Add(one,Jet.Scale(MetricStar(outer,weights),Fourier.Half*-1));var lower=MetricStar(upper,weights);
  foreach(var (j,d) in new[]{(f,2),(sf,12),(one,13),(inner,14),(zero,0),(outer,1),(upper,13),(lower,1)})if(!Typed(j.Value,d)||!Typed(j.Delta,d))throw new InvalidOperationException("Metric chain type");return lower;
 }
 public static (Rational Value,Rational Delta) MetricPair(Jet a,Jet b,int[] weights)
  =>(Pair(a.Value,b.Value),Pair(a.Delta,b.Value)+Pair(a.Value,b.Delta)+Pair(Weight(a.Value,weights,true),b.Value));
 public static Rational[] ActionPolynomial(FT[] state,FT background,FT curvature,FT p1,FT p2,bool first,int gamma,int kappa)
 {
  var q=PMul(state,state);var db=state.Select(v=>Covariant(background,v)).ToArray();var source=PPair(state,[NaiveChain(curvature,p1,p2,first)]);var kinetic=RScale(PPair(state,db.Select(v=>NaiveChain(v,p1,p2,first)).ToArray()),new Rational(1,2));var cubic=RScale(PPair(state,q.Select(v=>NaiveChain(v,p1,p2,first)).ToArray()),new Rational(gamma,3));var mass=first?RScale(PPair(state,state),new Rational(kappa,2)):new Rational[]{0};return RAdd(RAdd(source,kinetic),RAdd(cubic,mass));
 }
 // Pointwise independent jet: dVariation is an independently declared d(delta S)
 // at the point. No integration by parts and no projection on L are used.
 public static Rational[] VariationLegs(FT state,FT valueVariation,FT dVariation,FT background,FT curvature,FT p1,FT p2,bool first,int gamma,int kappa)
 {
  if(!ConstantReal(state,1)||!ConstantReal(valueVariation,1)||!ConstantReal(dVariation,2))throw new ArgumentException("pointwise real jet carrier");
  FT K(FT f)=>NaiveChain(f,p1,p2,first);var db=Covariant(background,state);var dbv=Add(dVariation,Product(background,valueVariation,'C'));var q=Product(state,state);var dq=Add(Product(state,valueVariation),Product(valueVariation,state));
  return [Pair(valueVariation,K(curvature)),(Pair(valueVariation,K(db))+Pair(state,K(dbv)))*new Rational(1,2),(Pair(valueVariation,K(q))+Pair(state,K(dq)))*new Rational(gamma,3),first?Pair(valueVariation,state)*kappa:0];
 }
 public static bool ConstantReal(FT t,int degree)=>Typed(t,degree)&&HAnti(t)&&t.Keys.All(k=>k.K0==0&&k.K1==0);
 public static (Rational Value,Rational Delta)[] ActionJet(Jet state,Jet background,Jet p1,Jet p2,int[] weights,bool first,int gamma,int kappa)
 {
  var curvature=Jet.Add(Jet.D(background),Jet.Product(background,background));var db=Jet.Add(Jet.D(state),Jet.Add(Jet.Product(background,state),Jet.Product(state,background)));var q=Jet.Product(state,state);
  var source=MetricPair(state,MetricChain(curvature,p1,p2,weights,first),weights);var kinetic=MetricPair(state,MetricChain(db,p1,p2,weights,first),weights);var cubic=MetricPair(state,MetricChain(q,p1,p2,weights,first),weights);var mass=MetricPair(state,state,weights);
  return [source,(kinetic.Value*new Rational(1,2),kinetic.Delta*new Rational(1,2)),(cubic.Value*new Rational(gamma,3),cubic.Delta*new Rational(gamma,3)),first?(mass.Value*new Rational(kappa,2),mass.Delta*new Rational(kappa,2)):(0,0)];
 }
}
