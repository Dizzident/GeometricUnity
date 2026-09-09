using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;
using static Fourier;
using static Cyclic;

// Literal source applications; no expected matrix or carrier enters this class.
internal sealed class SourceHessian
{
 public FT Gamma{get;}=new(); public FT Gamma2{get;}=new();
 public FT BackgroundUnit{get;} public FT Phi1{get;} public FT[] Phi2{get;}
 private readonly FT[] kadBackground=new FT[2];
 public bool AdjointPassed{get;private set;}=true;
 public SourceHessian(int h)
 {
  for(int j=0;j<14;j++)Gamma=Fourier.Add(Gamma,One(1<<j,1<<j,1));
  for(int i=0;i<14;i++)for(int j=i+1;j<14;j++){int m=(1<<i)|(1<<j);Gamma2=Fourier.Add(Gamma2,One(m,m,1));}
  BackgroundUnit=Fourier.Add(Gamma,Fourier.Scale(Omega(Gamma),-h));Phi1=Fourier.Add(Gamma,Fourier.Scale(Omega(Gamma),h));
  Phi2=[Fourier.Scale(Omega(Gamma2),Scalar.I*-h),Gamma2];
  for(int slot=0;slot<2;slot++)kadBackground[slot]=KA(BackgroundUnit,slot);
 }
 public FT K(FT v,int slot=0)=>Chain(v,Phi1,Phi2[slot],slot==0);
 public FT KA(FT v,int slot=0)
 {var literal=Adjoint.KAdjointLiteral(v,Phi1,Phi2[slot],slot==0);var other=Adjoint.KAdjointSimplified(v,Phi1,Phi2[slot],slot==0);AdjointPassed&=Equal(literal,other);return literal;}
 public static FT DQ(FT s,FT v)=>Fourier.Add(Product(s,v),Product(v,s));
 public FT[] Legs(FT v,int slot=0)=>[K(DQ(BackgroundUnit,v),slot),Adjoint.DQAdjoint(v,kadBackground[slot]),Adjoint.DQAdjoint(BackgroundUnit,KA(v,slot))];
 public FT Origin(FT v,int slot=0)=>Fourier.Scale(Fourier.Add(K(D(v),slot),Adjoint.DAdjoint(KA(v,slot))),Fourier.Half);
 public FT Apply(FT v,Rational kappa,int slot=0)
 {var legs=Legs(v,slot);FT result=Fourier.Add(Origin(v,slot),Fourier.Scale(legs.Aggregate(new FT(),Fourier.Add),new Scalar(kappa*new Rational(-1,1248),0)));return slot==0?Fourier.Add(result,Fourier.Scale(v,new Scalar(kappa,0))):result;}
}

internal static class Closure
{
 public static FT Mode(int n,bool sine,int form,int blade,Scalar coefficient)
 {if(n==0)return sine?new FT():One(form,blade,coefficient);var t=new FT();foreach(var q in Trig(0,sine,form,blade))Put(t,(q.Key.Form,q.Key.Blade,q.Key.K0*n,0),q.Value*coefficient);return t;}
 public static FT[] Basis(int n)
 {
  var u=Mode(n,false,1,4,1);var b=Mode(n,false,4,1,1);if(n==0)return[u,b,Omega(u),Omega(b)];
  FT e=new();for(int j=1;j<14;j++)if(j!=2)e=Fourier.Add(e,Mode(n,true,1<<j,4^(1<<j),Algebra.BladeSign(4,1<<j)));
  return[u,b,Omega(u),Omega(b),e,Mode(n,true,1,5,-1),Mode(n,true,4,Full,1)];
 }
 public static FT Z(int n)
 {FT t=new();for(int j=1;j<14;j++)if(j!=2){int m=1^(1<<j)^4;int sign=Algebra.BladeSign(1,1<<j)*Algebra.BladeSign(1^(1<<j),4);t=Fourier.Add(t,Mode(n,false,1<<j,m,sign));}return t;}
 public static FT Chiral(FT t,int h)=>Fourier.Add(t,Fourier.Scale(Omega(t),h));
 public static FT[][] ExpectedLegs(FT[] b,int h)
 {
  var rows=new FT[b.Length][];for(int i=0;i<4;i++)rows[i]=[Fourier.Scale(Chiral(b[i],h),-48),Fourier.Scale(b[i^1],-96),Fourier.Scale(Chiral(b[i],-h),-48)];
  if(b.Length==7){var e=b[4];var e0=b[5];var q=b[6];var x=Fourier.Add(Fourier.Scale(e0,12),Fourier.Scale(e,11));
   rows[4]=[Fourier.Scale(x,8),Fourier.Scale(x,96),Fourier.Add(Fourier.Add(Fourier.Scale(e0,96),Fourier.Scale(e,88)),Fourier.Scale(q,1152*h))];
   rows[5]=[Fourier.Scale(e,8),Fourier.Scale(e,96),Fourier.Add(Fourier.Scale(e,8),Fourier.Scale(q,96*h))];
   rows[6]=[Fourier.Scale(Fourier.Add(e,e0),-96*h),new FT(),new FT()];}return rows;
 }
 public static FT[] ExpectedOrigin(FT[] b,int h,int n)
 {if(n==0)return Enumerable.Range(0,4).Select(_=>new FT()).ToArray();return[Fourier.Scale(b[4],n),Fourier.Scale(b[4],n),Fourier.Scale(b[4],-h*n),Fourier.Scale(b[4],-h*n),Fourier.Scale(Chiral(Fourier.Add(b[0],b[1]),h),-12*n),new FT(),new FT()];}
 public static Rational[][] ExpectedMatrix(int h,int n,Rational k)
 {
  int size=n==0?4:7;var m=Zeros(size);for(int i=0;i<4;i++){m[i][i]=k*new Rational(14,13);m[i^1][i]=k*new Rational(1,13);}
  if(size==7){m[4][0]=n;m[4][1]=n;m[4][2]=-h*n;m[4][3]=-h*n;m[0][4]=-12*n;m[1][4]=-12*n;m[2][4]=-12*h*n;m[3][4]=-12*h*n;
   m[4][4]=k*new Rational(1,78);m[5][4]=k*new Rational(-14,13);m[6][4]=k*new Rational(-12*h,13);
   m[4][5]=k*new Rational(-7,78);m[5][5]=k;m[6][5]=k*new Rational(-h,13);m[4][6]=k*new Rational(h,13);m[5][6]=k*new Rational(h,13);m[6][6]=k;}return m;
 }
 public static FT Linear(FT[] b,Rational[] coordinates)=>b.Select((v,i)=>Fourier.Scale(v,new Scalar(coordinates[i],0))).Aggregate(new FT(),Fourier.Add);
 public static Rational[] Coordinates(FT t,FT[] basis,Rational[][] inverse)=>Multiply(inverse,basis.Select(v=>new[]{Fourier.Pair(v,t)}).ToArray()).Select(row=>row[0]).ToArray();
 public static Rational[][] Columns(IEnumerable<Rational[]> columns)=>Transpose(columns.ToArray());
 public static Rational[][] ScaleMatrix(Rational[][] a,Rational s)=>a.Select(row=>row.Select(v=>v*s).ToArray()).ToArray();
 public static Rational[][] AddMatrix(Rational[][] a,Rational[][] b)=>a.Select((row,i)=>row.Select((v,j)=>v+b[i][j]).ToArray()).ToArray();
 public static Rational[][] Shift(Rational[][] a,Rational eigen)=>AddMatrix(a,ScaleMatrix(Identity(a.Length),eigen*-1));
 public static Rational[][] Evaluate(CP p,Rational[][] a)
 {var result=Zeros(a.Length);var power=Identity(a.Length);foreach(var q in p.Coefficients){result=AddMatrix(result,ScaleMatrix(power,q));power=Multiply(power,a);}return result;}
 public static CP Factor(Rational root)=>new(root*-1,1);
 public static CP Minimal(int n,Rational k)
 {if(n==0)return k==0?CP.Variable:Factor(k)*Factor(k*new Rational(15,13));if(k==0)return CP.Variable*CP.Variable*CP.Variable;return Factor(k)*Factor(k*new Rational(15,13))*Factor(k*new Rational(15,13))*CP.Variable*Factor(k*new Rational(12,13))*Factor(k*new Rational(85,78));}
 public static CP CharacteristicExpected(int n,Rational k)=>n==0?Factor(k)*Factor(k)*Factor(k*new Rational(15,13))*Factor(k*new Rational(15,13)):Factor(k)*Factor(k)*Factor(k*new Rational(15,13))*Factor(k*new Rational(15,13))*CP.Variable*Factor(k*new Rational(12,13))*Factor(k*new Rational(85,78));
 // Original scalar-action mixed coefficient: only forward K and ordered wedge
 // products enter; no trace adjoint, actual Hessian, expected column or Gram inverse.
 public static Rational[][] ActionHessianUnit(FT[] b,SourceHessian source,bool cubic)
 {
  var m=Zeros(b.Length);var s=source.BackgroundUnit;
  if(!cubic){var kd=b.Select(v=>source.K(D(v))).ToArray();for(int i=0;i<b.Length;i++)for(int j=0;j<b.Length;j++)m[i][j]=(Fourier.Pair(b[i],kd[j])+Fourier.Pair(b[j],kd[i]))*new Rational(1,2);return m;}
  var ksv=b.Select(v=>source.K(Product(s,v))).ToArray();var kvs=b.Select(v=>source.K(Product(v,s))).ToArray();
  for(int i=0;i<b.Length;i++)for(int j=0;j<b.Length;j++)
  {
   // The six ordered placements of S,V,W in B(T,K(T wedge T)).
   var kvw=source.K(Product(b[i],b[j]));var kwv=source.K(Product(b[j],b[i]));
   // Exchange each pair before taking the real H-anti trace pairing.
   m[i][j]=Fourier.Pair(s,Fourier.Add(kvw,kwv))+Fourier.Pair(b[i],Fourier.Add(ksv[j],kvs[j]))+Fourier.Pair(b[j],Fourier.Add(ksv[i],kvs[i]));
  }return m;
 }
}
