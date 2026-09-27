using static Mixed;

// Uniform FULL homogeneous-background error transfer. No operator below is
// applied to an uncomputed stationary field; epsilon is the passed626 bound.
internal sealed record MixedError(
 Rational Radius,Rational FieldError,Rational MotionNorm,Rational MotionDerivativeSum,
 Rational ReferenceNorm,Rational ReferenceVariationNorm,Rational Raw0Majorant,
 Rational Raw2Majorant,Rational EulerMajorant,Rational Raw0Error,Rational Raw2Error,
 Rational EulerError,Rational NativeOneFormDualNorm,Rational NativeTwoFormDualNorm,
 Rational NativeRaw0Error,Rational NativeRaw2Error,Rational NativeEulerError,
 Rational NativeFieldNorm,Rational[] NativeFirstJetNorms)
{
 static Rational ColumnNorm(Matrix m)
 {Rational largest=0;for(int j=0;j<14;j++){Rational sum=0;for(int i=0;i<14;i++)sum+=Abs(m[i,j]);if((sum-largest).Numerator.Sign>0)largest=sum;}return largest;}
 static Rational RowNorm(Matrix m)=>ColumnNorm(SpinGeometry.Transpose(m));
 public static MixedError Create(MixedBackground b,MixedMetricGerm g,Rational epsilon)
 {
  Rational r=new(120,907712),k=2576,kappa=907712,m=MatrixNorm(g.Motion),mz=g.MotionCovariant.Aggregate((Rational)0,(s,z)=>s+MatrixNorm(z));
  Rational beta=Norm(g.DeltaB),reference=Norm(b.B);
  // ||D_B E||<=20||E|| is used ONLY for the difference of two invariant
  // backgrounds. Derivatives of the noninvariant metric germ are the explicit mz.
  Rational raw0=k*((20+2*reference)*m+2*beta+4*m*r)+2*kappa*m;
  Rational raw2=k*m;
  Rational euler=k*(mz+(40+4*reference)*m+2*beta+4*m*r)+2*kappa*m;
  Rational native=ColumnNorm(b.Frame),nativeTwo=native*native;
  var jetNorms=b.FrameLift.Select(l=>RowNorm(b.InverseFrame*l)).ToArray();
  return new(r,epsilon,m,mz,reference,beta,raw0,raw2,euler,raw0*epsilon,raw2*epsilon,euler*epsilon,native,nativeTwo,native*raw0*epsilon,nativeTwo*raw2*epsilon,native*euler*epsilon,RowNorm(b.InverseFrame),jetNorms);
 }
}
