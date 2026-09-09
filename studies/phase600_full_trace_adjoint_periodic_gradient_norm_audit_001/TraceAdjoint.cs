using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Real bilinear trace transposes. No coefficient conjugation, positive metric,
// chirality projection, anticipated target or finite-carrier compression.
internal static class Adjoint
{
 public static FT BracketAdjoint(FT phi,FT output,char kind)
 {
  if(kind is not ('C' or 'A'))throw new ArgumentException("C or A required");var t=new FT();
  foreach(var a in phi)foreach(var y in output)
  {
   if((a.Key.Form&y.Key.Form)!=a.Key.Form)continue;int input=y.Key.Form^a.Key.Form;
   int metric=Degree(a.Key.Form&0x3f80)%2==0?1:-1;
   int ab=BladeSign(a.Key.Blade,y.Key.Blade),ba=BladeSign(y.Key.Blade,a.Key.Blade);
   int factor=metric*Shuffle(a.Key.Form,input)*(kind=='C'?ba-ab:ab+ba);
   Scalar value=a.Value*y.Value*factor;if(kind=='A')value*=Scalar.I;
   Put(t,(input,a.Key.Blade^y.Key.Blade,a.Key.K0+y.Key.K0,a.Key.K1+y.Key.K1),value);
  }
  return t;
 }
 public static FT StarAdjoint(FT output,int forwardDegree)
 {
  if(forwardDegree<0||forwardDegree>14||!Typed(output,14-forwardDegree))throw new ArgumentException("Hodge adjoint domain");
  return Scale(Star(output),forwardDegree*(14-forwardDegree)%2==0?1:-1);
 }
 public static FT DAdjoint(FT output)
 {
  var t=new FT();for(int axis=0;axis<2;axis++)foreach(var q in Partial(output,axis))if((q.Key.Form&(1<<axis))!=0)
  {int contraction=Degree(q.Key.Form&((1<<axis)-1))%2==0?1:-1;Put(t,(q.Key.Form^(1<<axis),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(-Sigma(axis)*contraction));}return t;
 }
 public static FT KAdjointLiteral(FT y,FT phi1,FT phi2,bool first=true)
 {
  // Reverse star13 (C_phi1 star2 - .5 star1 C_phi1 star14 A_phi2 star2).
  var upper=StarAdjoint(y,13);
  var firstLeg=first?StarAdjoint(BracketAdjoint(phi1,upper,'C'),2):new FT();
  var outerOne=StarAdjoint(upper,1);var zero=BracketAdjoint(phi1,outerOne,'C');
  var top=StarAdjoint(zero,14);var twelve=BracketAdjoint(phi2,top,'A');
  var secondLeg=Scale(StarAdjoint(twelve,2),new Scalar(new Rational(-1,2),0));
  if(!Typed(y,1)||!Typed(upper,13)||!Typed(firstLeg,2)||!Typed(outerOne,1)||!Typed(zero,0)||!Typed(top,14)||!Typed(twelve,12)||!Typed(secondLeg,2))throw new InvalidOperationException("Reverse-chain types");
  return Add(firstLeg,secondLeg);
 }
 public static FT KAdjointSimplified(FT y,FT phi1,FT phi2,bool first=true)
 {
  // Independently simplified using star13 star1=I before transposition.
  var firstLeg=first?Scale(Star(BracketAdjoint(phi1,Star(y),'C')),-1):new FT();
  var secondLeg=Scale(Star(BracketAdjoint(phi2,Star(BracketAdjoint(phi1,y,'C')),'A')),new Scalar(new Rational(-1,2),0));
  if(!Typed(firstLeg,2)||!Typed(secondLeg,2))throw new InvalidOperationException("Simplified adjoint type");return Add(firstLeg,secondLeg);
 }
 // DQ_S[V]=S wedge V+V wedge S equals the coefficient-C wedge map.
 public static FT DQAdjoint(FT s,FT y)=>BracketAdjoint(s,y,'C');
 public static FT QuadraticHessian(FT t,FT phi1,FT phi2,bool first=true)=>Scale(Add(Chain(D(t),phi1,phi2,first),DAdjoint(KAdjointLiteral(t,phi1,phi2,first))),new Scalar(new Rational(1,2),0));
 public static FT QuadraticHessianSimplified(FT t,FT phi1,FT phi2,bool first=true)=>Scale(Add(Chain(D(t),phi1,phi2,first),DAdjoint(KAdjointSimplified(t,phi1,phi2,first))),new Scalar(new Rational(1,2),0));
 public static Rational CoefficientPair(FT a,FT b)
 {Rational sum=0;foreach(var q in a)if(b.TryGetValue(q.Key,out var z))sum+=q.Value.Real*z.Real+q.Value.Imaginary*z.Imaginary;return sum;}
}
