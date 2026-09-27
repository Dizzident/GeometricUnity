using static Algebra;
using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal sealed record WardPrimitive(string Name,BiTensor Tensor);
internal sealed record WardDensity(BiTensor[][] Stages,BiTensor[] TopForms,
 Rational[] Value,Rational[] Metric,Rational[] Field,Rational[] Mixed);
internal sealed record WardActionResult(bool Compensated,bool Oracle,FT Eta,
 FT NativeDeta,FT Deta,FT DBeta,FT Tangent,FT DBTangent,
 WardPrimitive[] Primitives,WardDensity Literal,WardDensity Descended);
internal sealed record WardAccelerationResult(bool Oracle,FT Eta,FT NativeDeta,
 FT W,FT Dw,FT DBw,FT[] PartialW,FT[] CovariantW,FT DwFromJet,FT DBwFromJet,
 FT[] DeltaBPartialInFrame,FT[] CovariantAdjointX,FT CrossQ,FT[][] Stages,Rational[] Original,Rational[] Euler,
 Rational[] Current,Rational[] CoordinateCurrent,
 Rational[][] CurrentCovariantDerivative,Rational Divergence);

// RIGHT epsilon variation, δε=epsilon*eta, in the fixed native chimeric
// identification. These are coefficients modulo h^2=u^2=0, not a finite
// approximation used outside a first mixed derivative. Every Clifford blade,
// including the central iI direction, is allowed; no restricted carrier is
// used by any operator below.
//
// Arbitrary jets: any H-anti-Hermitian eta0, eta_mu and symmetric eta_mu,nu
// are realized by a real quadratic polynomial in a fixed real blade basis.
// Ordinary d^2 eta is sum_{mu<nu}(eta_mu,nu-eta_nu,mu) dx_mu^dx_nu=0.
// Expanding D_B=d+[B,.] then gives D_B^2 eta=[dB+B^2,eta]=[F,eta].
// Consequently Evaluate needs eta and its FULL ordinary native first jet;
// omission of the symmetric second jet is an exact exterior quotient, not
// a claim that eta is affine, invariant, constant, or has zero covariant
// Hessian. SymmetricSecondJetImage supplies the corresponding full-tensor
// wedge control for any blade coefficient and any of the105 symmetric slots.
// Zero Fourier frequency is required: these are local coordinate jets, not
// a general Fourier-transpose implementation.
internal static class MixedWard
{
 public static long Evaluations{get;private set;}
 public static long AccelerationEvaluations{get;private set;}
 public static long ChainEvaluations{get;private set;}
 public static long SecondJetImages{get;private set;}

 static void Local(FT t,int degree,bool realDomain=true)
 {
  bool RationalCanonical(Rational r)=>r.Denominator.Sign>0&&
   System.Numerics.BigInteger.GreatestCommonDivisor(System.Numerics.BigInteger.Abs(r.Numerator),r.Denominator).IsOne;
  if(!Typed(t,degree)||t.Any(q=>q.Key.Form<0||q.Key.Form>=16384||q.Key.Blade<0||q.Key.Blade>=16384||
    q.Key.K0!=0||q.Key.K1!=0||q.Value.IsZero||!RationalCanonical(q.Value.Real)||!RationalCanonical(q.Value.Imaginary))||realDomain&&!HAnti(t))
   throw new ArgumentException("Ward input must be a complete typed zero-frequency real-domain local tensor");
 }
 static BiTensor Neg(BiTensor x)=>BiTensor.Scale(x,-1);
 static BiTensor Mul(BiTensor x,BiTensor y,bool oracle)=>BiTensor.Product(x,y,'W',oracle);
 static BiTensor Bracket(BiTensor x,BiTensor y,bool oracle)=>BiTensor.Product(x,y,'C',oracle);
 static BiTensor Conjugate(BiTensor left,BiTensor x,BiTensor right,bool oracle)=>Mul(Mul(left,x,oracle),right,oracle);

 static BiTensor[] Chain(BiTensor input,BiTensor firstPhi,BiTensor outerPhi,
  BiTensor innerPhi,Matrix motion,bool oracle)
 {
  ChainEvaluations++;
  var sf=BiTensor.Star(input,motion,oracle);
  var first=BiTensor.Product(firstPhi,sf,'C',oracle);
  var inner=BiTensor.Product(innerPhi,sf,'A',oracle);
  var zero=BiTensor.Star(inner,motion,oracle);
  var outer=BiTensor.Product(outerPhi,zero,'A',oracle);
  var upper=BiTensor.Add(first,BiTensor.Scale(BiTensor.Star(outer,motion,oracle),new Rational(-1,2)));
  BiTensor[] stages=[input,sf,first,inner,zero,outer,upper,BiTensor.Star(upper,motion,oracle)];
  return stages;
 }
 static WardDensity Density(BiTensor t,BiTensor f,BiTensor derivative,BiTensor q,
  BiTensor firstPhi,BiTensor outerPhi,BiTensor innerPhi,Matrix motion,bool oracle)
 {
  var stages=new[]{Chain(f,firstPhi,outerPhi,innerPhi,motion,oracle),
   Chain(derivative,firstPhi,outerPhi,innerPhi,motion,oracle),
   Chain(q,firstPhi,outerPhi,innerPhi,motion,oracle)};
  var tops=new BiTensor[4];var value=new Rational[4];var metric=new Rational[4];
  var field=new Rational[4];var mixed=new Rational[4];
  Rational[] weights=[1,new Rational(1,2),new Rational(1,3),new Rational(907712,2)];
  for(int piece=0;piece<4;piece++)
  {
   tops[piece]=Mul(t,BiTensor.Star(piece<3?stages[piece][7]:t,motion,oracle),oracle);
   value[piece]=Top(tops[piece].Value)*weights[piece];
   metric[piece]=Top(tops[piece].H)*weights[piece];
   field[piece]=Top(tops[piece].U)*weights[piece];
   mixed[piece]=Top(tops[piece].HU)*weights[piece];
  }
  return new(stages,tops,value,metric,field,mixed);
 }

 public static WardActionResult Evaluate(MixedBackground b,MixedMetricGerm g,
  FT eta,FT nativeDeta,bool compensated,bool oracle=false)
 {
  Local(eta,0);Local(nativeDeta,1);Local(b.X,1);Local(b.B,1);Local(b.F,2);Local(b.DX,2);
  Local(g.DeltaB,1);Local(g.DeltaFFixed,2);Local(g.DeltaBExterior,2);Local(g.DeltaCurvatureFromConnection,2);Evaluations++;
  var identity=Unit(0,0,1);var empty=new FT();
  var deta=Pullback(b.Frame,nativeDeta,oracle);
  var dbeta=Sum(deta,P(b.B,eta,'C',oracle));
  var acceleration=P(g.DeltaB,eta,'C',oracle);
  var tangent=compensated?Sum(dbeta,P(b.X,eta,'C',oracle)):new FT();
  // D_B(D_A eta)=[F,eta]+[D_B X,eta]-[X,D_B eta]_graded.
  var dbTangent=compensated?Sum(P(b.F,eta,'C',oracle),P(b.DX,eta,'C',oracle),
   Times(P(b.X,dbeta,'C',oracle),-1)):new FT();

  var epsilon=new BiTensor(identity,empty,eta,empty);
  var inverse=new BiTensor(identity,empty,Times(eta,-1),empty);
  var depsilon=new BiTensor(empty,empty,dbeta,acceleration);
  var dinverse=new BiTensor(empty,empty,Times(dbeta,-1),Times(acceleration,-1));
  var d2epsilon=new BiTensor(empty,empty,P(b.F,eta,'C',oracle),P(g.DeltaFFixed,eta,'C',oracle));
  // The baseline F is the full curvature independently tied by the caller's
  // preflight to passed618/626, not a newly inferred flat reference. At mixed
  // order independently expand delta(D_B^2 eta) using ACTUAL ordinary
  // d(dotB) and the first eta jet. The eta-derivative terms must cancel by
  // graded Jacobi, giving [d(dotB)+[B,dotB]_graded,eta]. Neither the expanded
  // tensor nor DeltaCurvatureFromConnection is obtained from DeltaFFixed.
  var d2Expanded=new BiTensor(empty,empty,d2epsilon.U,
   Sum(P(g.DeltaBExterior,eta,'C',oracle),Times(P(g.DeltaB,deta,'C',oracle),-1),
    P(g.DeltaB,dbeta,'C',oracle),P(b.B,acceleration,'C',oracle)));
  var curvatureVariationCommutator=new BiTensor(empty,empty,empty,P(g.DeltaCurvatureFromConnection,eta,'C',oracle));
  var referenceCurvature=new BiTensor(b.F,g.DeltaFFixed,empty,empty);
  var difference=Mul(inverse,depsilon,oracle);
  var differenceDerivative=BiTensor.Add(Mul(dinverse,depsilon,oracle),Mul(inverse,d2epsilon,oracle));

  // IMPORTANT: the compensated tangent V=D_A0 eta is FIXED at the baseline.
  // Following D_A(h) eta would insert acceleration here and make the mixed
  // Ward cancellation tautological. varpi_HU is zero; only its changing
  // reference derivative contributes [dotB,V] to D_Bvarpi_HU.
  var varpi=new BiTensor(b.X,empty,tangent,empty);
  var dbVarpi=new BiTensor(b.DX,P(g.DeltaB,b.X,'C',oracle),dbTangent,P(g.DeltaB,tangent,'C',oracle));
  var t=BiTensor.Add(varpi,Neg(difference));
  var dt=BiTensor.Add(dbVarpi,Neg(differenceDerivative));
  var rotatedCurvature=BiTensor.Add(referenceCurvature,differenceDerivative,Mul(difference,difference,oracle));
  var covariant=BiTensor.Add(dt,Bracket(difference,t,oracle));
  var quadratic=Mul(t,t,oracle);

  // All THREE Phi occurrences are formed by separate literal conjugations.
  BiTensor Phi(FT gamma)=>new(gamma,Times(Motion(g.Motion,gamma,oracle),-1),empty,empty);
  var unrotatedFirst=Phi(Phi1);var unrotatedOuter=Phi(Phi1);var unrotatedInner=Phi(Phi2);
  var first=Conjugate(inverse,unrotatedFirst,epsilon,oracle);
  var outer=Conjugate(inverse,unrotatedOuter,epsilon,oracle);
  var inner=Conjugate(inverse,unrotatedInner,epsilon,oracle);
  var literal=Density(t,rotatedCurvature,covariant,quadratic,first,outer,inner,g.Motion,oracle);

  // Independently differentiate the pullback by the exterior product rule.
  // The final minus sign is from T's form degree1, not a convention choice.
  var descended=Conjugate(epsilon,t,inverse,oracle);
  var descendedDerivative=BiTensor.Add(Mul(Mul(depsilon,t,oracle),inverse,oracle),
   Conjugate(epsilon,dt,inverse,oracle),Neg(Mul(Mul(epsilon,t,oracle),dinverse,oracle)));
  var derivativeOracle=Conjugate(epsilon,covariant,inverse,oracle);
  var descendedCurvature=Conjugate(epsilon,rotatedCurvature,inverse,oracle);
  var descendedQuadratic=Mul(descended,descended,oracle);
  var baseDensity=Density(descended,referenceCurvature,descendedDerivative,descendedQuadratic,
   unrotatedFirst,unrotatedOuter,unrotatedInner,g.Motion,oracle);
  WardPrimitive[] primitives=
  [
   new("epsilon",epsilon),new("inverse",inverse),new("epsilonInverse",Mul(epsilon,inverse,oracle)),
   new("inverseEpsilon",Mul(inverse,epsilon,oracle)),new("referenceCurvature",referenceCurvature),
   new("DBepsilon",depsilon),new("DBinverse",dinverse),new("DBSquaredEpsilon",d2epsilon),
   new("DBSquaredEpsilonExpanded",d2Expanded),new("curvatureVariationCommutator",curvatureVariationCommutator),
   new("gaugeDifference",difference),new("DBgaugeDifference",differenceDerivative),
   new("varpi",varpi),new("DBvarpi",dbVarpi),new("T",t),new("DBT",dt),
   new("rotatedCurvature",rotatedCurvature),new("covariantT",covariant),new("quadraticT",quadratic),
   new("unrotatedPhiFirst",unrotatedFirst),new("unrotatedPhiOuter",unrotatedOuter),new("unrotatedPhiInner",unrotatedInner),
   new("phiFirst",first),new("phiOuter",outer),new("phiInner",inner),new("descendedT",descended),
   new("descendedDerivative",descendedDerivative),new("descendedDerivativeOracle",derivativeOracle),
   new("descendedCurvature",descendedCurvature),new("descendedQuadratic",descendedQuadratic),
   new("descendedPhiFirst",Conjugate(epsilon,first,inverse,oracle)),
   new("descendedPhiOuter",Conjugate(epsilon,outer,inverse,oracle)),
   new("descendedPhiInner",Conjugate(epsilon,inner,inverse,oracle))
  ];
  return new(compensated,oracle,eta,nativeDeta,deta,dbeta,tangent,dbTangent,primitives,literal,baseDensity);
 }

 public static WardAccelerationResult Acceleration(MixedBackground b,MixedMetricGerm g,
  FT eta,FT nativeDeta,bool oracle=false)
 {
  Local(eta,0);Local(nativeDeta,1);Local(g.DeltaB,1);Local(g.DeltaBExterior,2);
  Local(b.X,1);Local(b.B,1);Local(b.F,2);Local(b.DX,2);Local(b.Q,2);Local(b.AdjointX,2);
  foreach(var t in b.KInputs.Concat(b.GradientPieces))Local(t,1);
  AccelerationEvaluations++;
  var deta=Pullback(b.Frame,nativeDeta,oracle);
  var w=P(g.DeltaB,eta,'C',oracle);
  var dw=Sum(P(g.DeltaBExterior,eta,'C',oracle),Times(P(g.DeltaB,deta,'C',oracle),-1));
  var dbw=Sum(dw,P(b.B,w,'C',oracle));
  var partialW=new FT[14];var covariantW=new FT[14];var covariantY=new FT[14];var partialBJ=new FT[14];
  for(int nu=0;nu<14;nu++)
  {
   var partialB=new FT();
   for(int mu=0;mu<14;mu++)
   {
    var spin=Spin(g.DeltaOmega[mu]);
    var partialSpin=Spin(g.DeltaOmegaPartial[nu,mu]);
    for(int a=0;a<14;a++)
    {
     if(b.FramePartial[nu][mu,a]!=0)partialB=Sum(partialB,Times(P(Unit(1<<a,0,1),spin,'W',oracle),b.FramePartial[nu][mu,a]));
     if(b.Frame[mu,a]!=0)partialB=Sum(partialB,Times(P(Unit(1<<a,0,1),partialSpin,'W',oracle),b.Frame[mu,a]));
    }
   }
   partialBJ[nu]=partialB;
   partialW[nu]=Sum(P(partialB,eta,'C',oracle),P(g.DeltaB,Component(nativeDeta,1<<nu),'C',oracle));
  }
  for(int a=0;a<14;a++)
  {
   covariantW[a]=new FT();covariantY[a]=new FT();
   for(int mu=0;mu<14;mu++)if(b.Frame[mu,a]!=0)
   {
    covariantW[a]=Sum(covariantW[a],Times(Sum(partialW[mu],Derivative(b.Omega[mu],w,oracle)),b.Frame[mu,a]));
    covariantY[a]=Sum(covariantY[a],Times(Derivative(b.Omega[mu],b.AdjointX,oracle),b.Frame[mu,a]));
   }
  }
  var dbwFromJet=WedgeCoordinate(covariantW,Matrix.Identity(14));
  var dwFromJet=Sum(dbwFromJet,Times(P(b.B,w,'C',oracle),-1));
  var cross=Sum(P(b.X,w,'W',oracle),P(w,b.X,'W',oracle));
  FT[][] stages=[FixedForward(dbw,oracle),FixedForward(cross,oracle)];
  Rational[] original=[Pair(w,b.KInputs[0]),
   (Pair(w,b.KInputs[1])+Pair(b.X,stages[0][7]))*new Rational(1,2),
   (Pair(w,b.KInputs[2])+Pair(b.X,stages[1][7]))*new Rational(1,3),Pair(b.X,w)*907712];
  var euler=b.GradientPieces.Select(t=>Pair(w,t)).ToArray();
  var current=new Rational[14];var coordinateCurrent=new Rational[14];var currentDerivative=new Rational[14][];
  for(int a=0;a<14;a++)current[a]=Pair(Contract(b.AdjointX,a),w)*new Rational(Sigma(a),2);
  for(int mu=0;mu<14;mu++)for(int a=0;a<14;a++)coordinateCurrent[mu]+=b.Frame[mu,a]*current[a];
  Rational divergence=0;
  for(int z=0;z<14;z++)
  {
   currentDerivative[z]=new Rational[14];
   for(int a=0;a<14;a++)currentDerivative[z][a]=
    (Pair(Contract(covariantY[z],a),w)+Pair(Contract(b.AdjointX,a),covariantW[z]))*new Rational(Sigma(a),2);
   divergence+=currentDerivative[z][z];
  }
  return new(oracle,eta,nativeDeta,w,dw,dbw,partialW,covariantW,dwFromJet,dbwFromJet,
   partialBJ,covariantY,cross,stages,original,euler,current,coordinateCurrent,currentDerivative,divergence);
 }

 public static FT SymmetricSecondJetImage(int first,int second,FT coefficient,bool oracle=false)
 {
  if(first<0||first>=14||second<first||second>=14)throw new ArgumentOutOfRangeException(nameof(first));
  Local(coefficient,0);SecondJetImages++;
  var left=P(P(Unit(1<<first,0,1),Unit(1<<second,0,1),'W',oracle),coefficient,'W',oracle);
  // A diagonal Hessian has one slot, not twice its coefficient.
  return first==second?left:Sum(left,P(P(Unit(1<<second,0,1),Unit(1<<first,0,1),'W',oracle),coefficient,'W',oracle));
 }
}
