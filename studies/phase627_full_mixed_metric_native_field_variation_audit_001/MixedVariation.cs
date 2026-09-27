using static Algebra;
using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal sealed class MixedVariation
{
 public bool DerivativeIdentityPassed{get;private set;}=true;
 public FT CAdjointFixed{get;}
 public FT CAdjointOracle{get;}
 public FT MovingAdjointDelta{get;}
 public FT[] Raw0{get;}
 public FT[] Raw2{get;}
 public FT[] FieldFirst0{get;}
 public FT[] FieldFirst2{get;}
 public FT[] Oracle0{get;}
 public FT[] Oracle2{get;}
 public FT OrdinaryAdjoint{get;}
 public FT[] EulerCovariant{get;}
 public FT[] EulerMoving{get;}
 public FT[] Native0{get;}
 public FT Native2{get;}
 public FT NativeDivergence{get;}
 public FT NativeAdjointAsFrame{get;}
 public FT[,] CurrentCoefficients{get;}=new FT[14,14];
 public FT[,] CurrentPartialCoefficients{get;}=new FT[14,14];
 public FT[,] GreenVariationCoefficients{get;}=new FT[14,14];
 public FT[] NativeEuler{get;}
 public MixedVariation(MixedBackground b,MixedMetricGerm g,Action<string,FT>? intermediate=null)
 {
  var a=g.Motion;var vx=PairingMotion(a,b.X);var v2y=PairingMotion(a,b.AdjointX);
  var deltaDX=P(g.DeltaB,b.X,'C');
  var front=new FT[3];Jet[] inputs=[new(b.F,g.DeltaFFixed),new(b.DX,deltaDX),Jet.Fixed(b.Q)];
  for(int piece=0;piece<3;piece++)
  {
   var stages=Forward(inputs[piece],a);for(int stage=0;stage<8;stage++){intermediate?.Invoke($"fixed_p{piece}_s{stage}_value",stages[stage].Value);intermediate?.Invoke($"fixed_p{piece}_s{stage}_delta",stages[stage].Delta);}
   front[piece]=Sum(stages[7].Delta,PairingMotion(a,b.KInputs[piece]));
  }
  var fixedReverse=Reverse(Jet.Fixed(b.X),a);for(int stage=0;stage<9;stage++){intermediate?.Invoke($"reverse_s{stage}_value",fixedReverse[stage].Value);intermediate?.Invoke($"reverse_s{stage}_delta",fixedReverse[stage].Delta);}
  CAdjointFixed=Sum(fixedReverse[8].Delta,FixedAdjoint(vx));
  CAdjointOracle=CAdjoint(b.X,a,true);
  // Independent MOVING adjoint: transport both its input and output form
  // slots, with the full word-transpose operator, not the sought C^dag result.
  var dx=Motion(a,b.X,true);var dy=FixedAdjoint(dx,true);
  MovingAdjointDelta=Sum(dy,Times(Motion(a,b.AdjointX,true),-1));
  var deltaBBack=Transpose(g.DeltaB,b.AdjointX);var bBack=Transpose(b.B,CAdjointFixed);var qBack=Transpose(b.X,CAdjointFixed);
  Raw0=[front[0],Times(Sum(front[1],bBack,deltaBBack),new Rational(1,2)),Times(Sum(front[2],qBack),new Rational(1,3)),Times(vx,907712)];
  Raw2=[new FT(),Times(CAdjointFixed,new Rational(1,2)),new FT(),new FT()];
  // FIELD-FIRST: independently differentiate the original first-action dual
  // in adapted coordinates. No C^dag or metric-first response is an input.
  var oldBBack=Transpose(b.B,b.AdjointX,'C',true);var oldQBack=Transpose(b.X,b.AdjointX,'C',true);
  var dbAdapted=Sum(g.DeltaB,Motion(a,b.B,true));
  var derivativeBBack=Sum(Transpose(dbAdapted,b.AdjointX,'C',true),Transpose(b.B,dy,'C',true));
  var derivativeQBack=Sum(Transpose(dx,b.AdjointX,'C',true),Transpose(b.X,dy,'C',true));
  var derivativeDX=Sum(Motion(a,b.DX,true),P(g.DeltaB,b.X,'C',true));
  var derivativeQ=Sum(P(dx,b.X,'W',true),P(b.X,dx,'W',true));
  FieldFirst0=[Sum(FixedForward(g.DeltaFAdapted,true)[7],MotionAdjoint(a,b.KInputs[0],true)),Times(Sum(FixedForward(derivativeDX,true)[7],derivativeBBack,MotionAdjoint(a,Sum(b.KInputs[1],oldBBack),true)),new Rational(1,2)),Times(Sum(FixedForward(derivativeQ,true)[7],derivativeQBack,MotionAdjoint(a,Sum(b.KInputs[2],oldQBack),true)),new Rational(1,3)),Times(Sum(dx,MotionAdjoint(a,b.X,true)),907712)];
  FieldFirst2=[new FT(),Times(Sum(dy,MotionAdjoint(a,b.AdjointX,true)),new Rational(1,2)),new FT(),new FT()];
  // Independent adapted-frame/ordered-word route. All original Clifford
  // coefficients, including noncyclic grade5 reverse outputs, remain present.
  var sourceOther=Sum(C(b.F,a,true),FixedForward(g.DeltaFFixed,true)[7]);
  var kineticOther=Sum(C(b.DX,a,true),FixedForward(P(g.DeltaB,b.X,'C',true),true)[7],Transpose(b.B,CAdjointOracle,'C',true),Transpose(g.DeltaB,b.AdjointX,'C',true));
  var cubicOther=Sum(C(b.Q,a,true),Transpose(b.X,CAdjointOracle,'C',true));
  Oracle0=[sourceOther,Times(kineticOther,new Rational(1,2)),Times(cubicOther,new Rational(1,3)),Times(PairingMotion(a,b.X,true),907712)];
  Oracle2=[new FT(),Times(CAdjointOracle,new Rational(1,2)),new FT(),new FT()];
  var divergence=new FT();for(int z=0;z<14;z++)
  {
   var derivative=Times(Sum(CAdjoint(b.X,g.MotionCovariant[z]),CAdjoint(b.CovariantFrame[z],a)),new Rational(1,2));
   var weights=Enumerable.Range(0,14).Select(mu=>b.Frame[mu,z]).ToArray();
   var partial=Times(CAdjoint(b.X,Homogeneous.Combine(g.MotionPartial,weights),true),new Rational(1,2));
   var alternate=Sum(partial,Derivative(Homogeneous.Combine(b.Omega,weights),Oracle2[1],true));
   DerivativeIdentityPassed&=Mixed.Equal(derivative,alternate);intermediate?.Invoke($"covariant_z{z}",derivative);intermediate?.Invoke($"covariantOracle_z{z}",alternate);
   divergence=Sum(divergence,Times(Contract(derivative,z),-Sigma(z)));
  }
  // Ordinary d^dag is NOT D_B^dag: remove the algebraic spin-reference adjoint.
  OrdinaryAdjoint=Sum(divergence,Times(Transpose(b.B,Raw2[1]),-1));
  EulerCovariant=[Raw0[0],Sum(Raw0[1],OrdinaryAdjoint),Raw0[2],Raw0[3]];
  EulerMoving=MovingEuler(b,g);
  Native0=Raw0.Select(t=>CoordinateDual(b.Frame,t)).ToArray();Native2=CoordinateDual(b.Frame,Raw2[1]);
  var nativeDivergence=new FT();
  var adaptedYDelta=FixedAdjoint(Motion(a,b.X));
  var dualSlices=TwoFormSlices(Raw2[1]);var dySlices=TwoFormSlices(adaptedYDelta);var ySlices=TwoFormSlices(b.AdjointX);
  for(int mu=0;mu<14;mu++)
  {
   var dualPartial=Times(CAdjoint(b.X,g.MotionPartial[mu],true),new Rational(1,2));intermediate?.Invoke($"partial_mu{mu}",dualPartial);var partialSlices=TwoFormSlices(dualPartial);
   for(int nu=0;nu<14;nu++)
   {
   var current=new FT();var derivative=new FT();var green=new FT();
   for(int i=0;i<14;i++)for(int j=0;j<14;j++)
   {
    int orientation=i==j?0:i<j?1:-1;
    Rational coefficient=b.Frame[mu,i]*b.Frame[nu,j]*Sigma(i)*Sigma(j)*orientation;
    Rational changed=(b.FramePartial[mu][mu,i]*b.Frame[nu,j]+b.Frame[mu,i]*b.FramePartial[mu][nu,j])*Sigma(i)*Sigma(j)*orientation;
    Rational metricChanged=(g.DeltaFrame[mu,i]*b.Frame[nu,j]+b.Frame[mu,i]*g.DeltaFrame[nu,j])*Sigma(i)*Sigma(j)*orientation;
    if(coefficient!=0){current=Sum(current,Times(dualSlices[i,j],coefficient));derivative=Sum(derivative,Times(partialSlices[i,j],coefficient));green=Sum(green,Times(dySlices[i,j],coefficient*new Rational(1,2)));}
    if(changed!=0)derivative=Sum(derivative,Times(dualSlices[i,j],changed));
    if(metricChanged!=0)green=Sum(green,Times(ySlices[i,j],metricChanged*new Rational(1,2)));
   }
   CurrentCoefficients[mu,nu]=current;GreenVariationCoefficients[mu,nu]=green;
   // Normalized coordinate divergence retains the baseline volume derivative.
   derivative=Sum(derivative,Times(current,b.FrameLift[mu].Trace()*-1));CurrentPartialCoefficients[mu,nu]=derivative;
   nativeDivergence=Sum(nativeDivergence,P(Unit(1<<nu,0,1),derivative));
   }
  }
  NativeDivergence=nativeDivergence;NativeAdjointAsFrame=FrameDual(b.InverseFrame,Times(nativeDivergence,-1));
  NativeEuler=[Native0[0],Sum(Native0[1],Times(nativeDivergence,-1)),Native0[2],Native0[3]];
 }
 static FT[] MovingEuler(MixedBackground b,MixedMetricGerm g)
 {
  var dx=Motion(g.Motion,b.X);var dy=FixedAdjoint(dx);var partialDy=g.MotionPartial.Select(p=>FixedAdjoint(Motion(p,b.X))).ToArray();
  var deltaReverseFrame=new FT[14];for(int a=0;a<14;a++)
  {
   var derivative=new FT();for(int mu=0;mu<14;mu++)
   {
    if(g.DeltaFrame[mu,a]!=0)derivative=Sum(derivative,Times(Derivative(b.Omega[mu],b.AdjointX),g.DeltaFrame[mu,a]));
    if(b.Frame[mu,a]!=0)derivative=Sum(derivative,Times(Sum(partialDy[mu],Derivative(b.Omega[mu],dy),Derivative(g.DeltaOmega[mu],b.AdjointX)),b.Frame[mu,a]));
   }deltaReverseFrame[a]=derivative;
  }
  var reverse=Divergence(deltaReverseFrame);
  var deltaDX=Sum(Motion(g.Motion,b.DX),P(g.DeltaB,b.X,'C'));
  var deltaQ=Sum(P(dx,b.X),P(b.X,dx));
  FT[] adapted=[FixedForward(g.DeltaFAdapted)[7],Times(Sum(FixedForward(deltaDX)[7],reverse),new Rational(1,2)),Times(Sum(FixedForward(deltaQ)[7],Transpose(dx,b.AdjointX),Transpose(b.X,dy)),new Rational(1,3)),Times(dx,907712)];
  // P5 is off shell: this is delta G_fixed + V1 G, NOT delta G alone.
  return adapted.Select((t,piece)=>Sum(t,MotionAdjoint(g.Motion,b.GradientPieces[piece]))).ToArray();
 }
 public static FT CoordinateDual(Matrix frame,FT t)
 {
  // Exterior Gram raising is essential. The native coefficient arrays use only
  // the Clifford trace pairing, without an additional coordinate-form metric.
  return Pullback(SpinGeometry.Transpose(frame),Raise(t),true);
 }
 public static FT FrameDual(Matrix inverse,FT native)
 {
  return Raise(Pullback(SpinGeometry.Transpose(inverse),native,true));
 }
}
