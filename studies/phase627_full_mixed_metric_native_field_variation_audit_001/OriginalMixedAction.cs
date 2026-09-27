using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Coefficients of 1,h,u,hu in the original action. No first-gradient or mixed
// response tensor is an input to this independent scalar product-rule route.
internal sealed record BiTensor(FT Value,FT H,FT U,FT HU)
{
 public static BiTensor Fixed(FT t)=>new(t,new FT(),new FT(),new FT());
 public static BiTensor Add(params BiTensor[] x)=>new(Sum(x.Select(v=>v.Value).ToArray()),Sum(x.Select(v=>v.H).ToArray()),Sum(x.Select(v=>v.U).ToArray()),Sum(x.Select(v=>v.HU).ToArray()));
 public static BiTensor Scale(BiTensor x,Rational c)=>new(Times(x.Value,c),Times(x.H,c),Times(x.U,c),Times(x.HU,c));
 public static BiTensor Product(BiTensor x,BiTensor y,char kind='W',bool reverseOrder=false)
 {
  var value=P(x.Value,y.Value,kind,reverseOrder);
  var h=Sum(P(x.H,y.Value,kind,reverseOrder),P(x.Value,y.H,kind,reverseOrder));
  var u=Sum(P(x.U,y.Value,kind,reverseOrder),P(x.Value,y.U,kind,reverseOrder));
  // The original bivariate coefficient is evaluated with independent word
  // products/accumulation. This shared four-term formula is NOT claimed to be
  // two independent nested AD implementations; those coefficient derivations
  // are the separate raw metric-first and adapted field-first routes.
  // The cross coefficient has no factorial2.
  var hu=reverseOrder
   ?Sum(P(x.U,y.H,kind,true),P(x.Value,y.HU,kind,true),P(x.HU,y.Value,kind,true),P(x.H,y.U,kind,true))
   :Sum(P(x.HU,y.Value,kind),P(x.H,y.U,kind),P(x.U,y.H,kind),P(x.Value,y.HU,kind));
  return new(value,h,u,hu);
 }
 public static BiTensor Star(BiTensor x,Matrix motion,bool oracle=false)=>new(Mixed.Hodge(x.Value),Sum(Mixed.Hodge(x.H),StarDelta(motion,x.Value,oracle)),Mixed.Hodge(x.U),Sum(Mixed.Hodge(x.HU),StarDelta(motion,x.U,oracle)));
}

internal sealed record OriginalMixedResult(BiTensor[][] Stages,Rational[] Value,Rational[] Metric,Rational[] Field,Rational[] Mixed);
internal static class OriginalMixedAction
{
 public static BiTensor[] Chain(BiTensor input,Matrix a,bool oracle)
 {
  var p1=new BiTensor(Phi1,Times(Motion(a,Phi1,oracle),-1),new FT(),new FT());
  var p2=new BiTensor(Phi2,Times(Motion(a,Phi2,oracle),-1),new FT(),new FT());
  var sf=BiTensor.Star(input,a,oracle);var first=BiTensor.Product(p1,sf,'C',oracle);var inner=BiTensor.Product(p2,sf,'A',oracle);
  var zero=BiTensor.Star(inner,a,oracle);var outer=BiTensor.Product(p1,zero,'A',oracle);
  var upper=BiTensor.Add(first,BiTensor.Scale(BiTensor.Star(outer,a,oracle),new Rational(-1,2)));var lower=BiTensor.Star(upper,a,oracle);
  return[input,sf,first,inner,zero,outer,upper,lower];
 }
 public static OriginalMixedResult Evaluate(MixedBackground b,MixedMetricGerm g,FT nativeU,FT nativeDu,bool reverseOrder=false)
 {
  // BOTH probe inputs are native coordinate forms. Transform every slot at
  // both noncoincident points before using the baseline orthonormal pairing.
  var u=Pullback(b.Frame,nativeU,reverseOrder);var du=Pullback(b.Frame,nativeDu,reverseOrder);
  var x=new BiTensor(b.X,new FT(),u,new FT());
  var spin=new BiTensor(b.B,g.DeltaB,new FT(),new FT());
  var partial=new BiTensor(Pullback(b.Frame,b.NativeExterior,reverseOrder),new FT(),du,new FT());
  var covariant=BiTensor.Add(partial,BiTensor.Product(spin,x,'C',reverseOrder));
  var curvature=new BiTensor(b.F,g.DeltaFFixed,new FT(),new FT());
  var quadratic=BiTensor.Product(x,x,'W',reverseOrder);
  var chains=new[]{Chain(curvature,g.Motion,reverseOrder),Chain(covariant,g.Motion,reverseOrder),Chain(quadratic,g.Motion,reverseOrder)};
  Rational[] weights=[1,new Rational(1,2),new Rational(1,3),new Rational(907712,2)];
  var value=new Rational[4];var metric=new Rational[4];var field=new Rational[4];var mixed=new Rational[4];
  for(int p=0;p<4;p++)
  {
   var top=BiTensor.Product(x,BiTensor.Star(p<3?chains[p][7]:x,g.Motion,reverseOrder),'W',reverseOrder);
   value[p]=Top(top.Value)*weights[p];metric[p]=Top(top.H)*weights[p];field[p]=Top(top.U)*weights[p];mixed[p]=Top(top.HU)*weights[p];
  }
  return new(chains,value,metric,field,mixed);
 }
}
