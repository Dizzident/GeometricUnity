using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;
using static Fourier;
using static Cyclic;
using static Closure;

internal static class General
{
 public static FT Sc(FT t,Rational s)=>Fourier.Scale(t,new Scalar(s,0));
 public static FT Sum(params FT[] a)=>a.Aggregate(new FT(),Fourier.Add);
 public static FT[] Basis12(int n)
 {var old=Basis(n);var z=Fourier.Scale(Z(n),Scalar.I);if(n==0)return[old[0],old[1],old[2],old[3],z,Omega(z)];return[old[0],old[1],old[2],old[3],z,Omega(z),old[4],old[5],old[6],Mode(n,true,4,0,Scalar.I),Fourier.Scale(Omega(old[4]),Scalar.I),Fourier.Scale(Omega(old[5]),Scalar.I)];}
 public static FT[] OriginExpected(FT[] b,int h,int n,int slot)
 {
  if(n==0)return Enumerable.Range(0,6).Select(_=>new FT()).ToArray();var pz=Chiral(b[4],h);var px=Chiral(Sum(b[0],b[1]),h);var pa=Chiral(Sum(b[0],Sc(b[1],-1)),h);var tt=Sum(b[10],Sc(b[11],6));
  if(slot==0)return[Sc(b[6],n),Sc(b[6],n),Sc(b[6],-h*n),Sc(b[6],-h*n),Sum(Sc(b[9],-12*n),Sc(tt,-2*h*n)),Sum(Sc(b[9],12*h*n),Sc(tt,2*n)),Sc(px,-12*n),new FT(),new FT(),Sc(pz,n),Sc(pz,-2*h*n),Sc(pz,-h*n)];
  return[Sc(b[9],n),Sc(b[9],-n),Sc(b[9],-h*n),Sc(b[9],h*n),Sc(Sum(b[6],b[7]),12*n),Sc(Sum(b[6],b[7]),-12*h*n),Sc(pz,-12*n),Sc(pz,-n),new FT(),Sc(pa,-n),new FT(),new FT()];
 }
 public static FT[][] LegsExpected(FT[] b,int h,int slot)
 {
  var result=new FT[b.Length][];var a=Sum(b[0],Sc(b[1],-1));var z=b[4];
  if(slot==0)
  {
   for(int i=0;i<4;i++)result[i]=[Sc(Chiral(b[i],h),-48),Sc(b[i^1],-96),Sc(Chiral(b[i],-h),-48)];
   result[4]=[Sc(Chiral(z,h),432),Sc(z,1056),Sc(Chiral(z,-h),432)];
  }
  else
  {
   result[0]=[Sc(Chiral(z,h),4),Sc(Omega(z),-112*h),Sc(Chiral(z,-h),-44)];result[1]=result[0].Select(v=>Sc(v,-1)).ToArray();
   result[2]=[Sc(result[0][0],h),Omega(result[0][1]),Sc(result[0][2],-h)];result[3]=result[2].Select(v=>Sc(v,-1)).ToArray();
   result[4]=[Sc(Chiral(a,h),-528),Sc(Omega(a),1344*h),Sc(Chiral(a,-h),48)];
  }
  result[5]=[Sc(result[4][0],h),Omega(result[4][1]),Sc(result[4][2],-h)];
  if(b.Length==6)return result;
  var e=b[6];var e0=b[7];var q=b[8];var t=b[10];var t0=b[11];var E=Sum(e,e0);var U=Sum(t,t0);
  result[9]=[new FT(),new FT(),new FT()];
  if(slot==0)
  {
   var x=Sum(Sc(e0,12),Sc(e,11));result[6]=[Sc(x,8),Sc(x,96),Sum(Sc(e0,96),Sc(e,88),Sc(q,1152*h))];
   result[7]=[Sc(e,8),Sc(e,96),Sum(Sc(e,8),Sc(q,96*h))];result[8]=[Sc(E,-96*h),new FT(),new FT()];
   var at=Sum(Sc(t,-184),Sc(t0,-96));var at0=Sum(Sc(t,-8),Sc(t0,-96));
   result[10]=[at,Sum(Sc(t,1056),Sc(t0,1152)),at];result[11]=[at0,Sc(t,96),at0];
  }
  else
  {
   result[6]=[new FT(),Sc(Sum(Sc(t,11),Sc(t0,12)),112*h),Sc(U,-1152*h)];result[7]=[new FT(),Sc(t,112*h),Sc(U,-96*h)];result[8]=[new FT(),new FT(),Sc(U,-96)];
   result[10]=[Sum(Sc(E,1152*h),Sc(q,-1152)),Sc(Sum(Sc(e,11),Sc(e0,12)),-112*h),new FT()];
   result[11]=[Sum(Sc(E,96*h),Sc(q,-96)),Sc(e,-112*h),new FT()];
  }return result;
 }
 public static FT[] PotentialExpected(FT[] b,int h,int slot)
 {
  // Independent combined-column oracle, not a sum of the leg table.
  var r=new FT[b.Length];var a=Sum(b[0],Sc(b[1],-1));var oa=Omega(a);var z=b[4];var oz=b[5];
  if(slot==0){for(int i=0;i<4;i++)r[i]=Sc(Sum(Sc(b[i],14),b[i^1]),new Rational(1,13));r[4]=Sc(z,new Rational(-7,13));r[5]=Sc(oz,new Rational(-7,13));}
  else{r[0]=Sc(Sum(Sc(z,5),Sc(oz,8*h)),new Rational(1,156));r[1]=Sc(r[0],-1);r[2]=Sc(Sum(Sc(z,8*h),Sc(oz,5)),new Rational(1,156));r[3]=Sc(r[2],-1);r[4]=Sc(Sum(Sc(a,5),Sc(oa,-8*h)),new Rational(1,13));r[5]=Sc(Sum(Sc(a,-8*h),Sc(oa,5)),new Rational(1,13));}
  if(b.Length==6)return r;var e=b[6];var e0=b[7];var q=b[8];var j=b[9];var t=b[10];var t0=b[11];
  if(slot==0){r[6]=Sum(Sc(e,new Rational(1,78)),Sc(e0,new Rational(-14,13)),Sc(q,new Rational(-12*h,13)));r[7]=Sum(e0,Sc(e,new Rational(-7,78)),Sc(q,new Rational(-h,13)));r[8]=Sum(q,Sc(Sum(e,e0),new Rational(h,13)));r[9]=j;r[10]=Sc(Sum(Sc(t,35),Sc(t0,-60)),new Rational(1,78));r[11]=Sc(Sum(Sc(t,-5),Sc(t0,90)),new Rational(1,78));}
  else{r[6]=Sc(Sum(Sc(t,-5),Sc(t0,-12)),new Rational(h,78));r[7]=Sc(Sum(Sc(t,-1),Sc(t0,6)),new Rational(h,78));r[8]=Sc(Sum(t,t0),new Rational(1,13));r[9]=new FT();r[10]=Sum(Sc(Sum(Sc(e,5),Sc(e0,12)),new Rational(h,78)),Sc(q,new Rational(12,13)));r[11]=Sum(Sc(Sum(e,Sc(e0,-6)),new Rational(h,78)),Sc(q,new Rational(1,13)));}return r;
 }
 public static Rational[][] ActionSlot(FT[] b,SourceHessian source,int slot,bool cubic)
 {
  var m=Zeros(b.Length);var s=source.BackgroundUnit;
  if(!cubic){var kd=b.Select(v=>source.K(D(v),slot)).ToArray();for(int i=0;i<b.Length;i++)for(int j=0;j<b.Length;j++)m[i][j]=(Fourier.Pair(b[i],kd[j])+Fourier.Pair(b[j],kd[i]))*new Rational(1,2);return m;}
  var left=b.Select(v=>source.K(Product(s,v),slot)).ToArray();var right=b.Select(v=>source.K(Product(v,s),slot)).ToArray();
  for(int i=0;i<b.Length;i++)for(int j=0;j<b.Length;j++)m[i][j]=Fourier.Pair(s,Sum(source.K(Product(b[i],b[j]),slot),source.K(Product(b[j],b[i]),slot)))+Fourier.Pair(b[i],Sum(left[j],right[j]))+Fourier.Pair(b[j],Sum(left[i],right[i]));return m;
 }
 public static FT[] FlagBasis(FT[] b,int h)
 {var x=Sum(b[0],b[1]);var a=Sum(b[0],Sc(b[1],-1));FT[] plus=[Chiral(x,h),Chiral(a,h),Chiral(b[4],h)],minus=[Sc(Chiral(x,-h),new Rational(1,2)),Sc(Chiral(a,-h),new Rational(1,2)),Sc(Chiral(b[4],-h),new Rational(1,2))];return b.Length==6?plus.Concat(minus).ToArray():plus.Concat(b.Skip(6)).Concat(minus).ToArray();}
 public static FT[] FactorBasis(FT[] b,int h)
 {var flag=FlagBasis(b,h);if(b.Length==6)return flag;FT[] even=[Sum(b[6],b[7]),b[8],Sum(b[10],b[11]),b[9],Sum(b[6],Sc(b[7],-12)),Sum(b[10],Sc(b[11],-12))];return flag.Take(3).Concat(even).Concat(flag.Skip(9)).ToArray();}
 public static CP[][] PolynomialMatrix(Rational[][] a,Rational[][] b)=>a.Select((row,i)=>row.Select((v,j)=>new CP(v,b[i][j])).ToArray()).ToArray();
 public static CP[][] PZero(int n)=>Enumerable.Range(0,n).Select(_=>Enumerable.Range(0,n).Select(_=>(CP)0).ToArray()).ToArray();
 public static CP[][] PMultiply(CP[][] a,CP[][] b)=>a.Select(row=>Enumerable.Range(0,b[0].Length).Select(j=>Enumerable.Range(0,b.Length).Select(k=>row[k]*b[k][j]).Aggregate((CP)0,(s,v)=>s+v)).ToArray()).ToArray();
 public static CP[][] PTransform(Rational[][] inverse,CP[][] matrix,Rational[][] forward)=>PMultiply(PMultiply(inverse.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray(),matrix),forward.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray());
 public static CP[] FastCharacteristic(CP[][] a)
 {
  // Faddeev-LeVerrier in characteristic zero. Entries may themselves be formal c polynomials.
  int n=a.Length;var work=Identity(n).Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray();var coefficients=new CP[n+1];coefficients[n]=1;
  for(int k=1;k<=n;k++){work=PMultiply(a,work);CP trace=0;for(int i=0;i<n;i++)trace+=work[i][i];var coefficient=trace.Scale(new Rational(-1,k));coefficients[n-k]=coefficient;for(int i=0;i<n;i++)work[i][i]+=coefficient;}return coefficients;
 }
 public static CP FastCharacteristic(Rational[][] a)=>new(FastCharacteristic(a.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray()).Select(v=>v[0]).ToArray());
 public static CP[] LambdaProduct(CP[] a,CP[] b){var r=Enumerable.Range(0,a.Length+b.Length-1).Select(_=>(CP)0).ToArray();for(int i=0;i<a.Length;i++)for(int j=0;j<b.Length;j++)r[i+j]+=a[i]*b[j];return r;}
 public static CP[] Factors(int n,Rational k,CP c)
 {CP[] alpha=[(CP)(k*new Rational(-15,13)),1],odd=[(c*c-14).Scale(k*k*new Rational(1,26)),(CP)(k*new Rational(-6,13)),1];var p=LambdaProduct(LambdaProduct(alpha,alpha),LambdaProduct(odd,odd));if(n==0)return p;
  CP[] even=[((CP)5-c*c).Scale(k*k*new Rational(12,169)),(CP)(k*new Rational(-17,13)),1],hook=[((CP)8075+(c*c).Scale(49)).Scale(k*k*new Rational(1,6084)),(CP)(k*new Rational(-30,13)),1];return LambdaProduct(LambdaProduct(LambdaProduct(LambdaProduct(p,new CP[]{(CP)(k*-1),1}),new CP[]{0,1}),even),hook);}
 public static CP Factors(int n,Rational k,Rational c)=>new(Factors(n,k,(CP)c).Select(v=>v[0]).ToArray());
 public static bool PSame(CP[] a,CP[] b)=>a.Length==b.Length&&a.Zip(b,(x,y)=>x.Same(y)).All(v=>v);
 public static bool PMatrixSame(CP[][] a,CP[][] b)=>a.Length==b.Length&&a.Zip(b,(x,y)=>PSame(x,y)).All(v=>v);
 public static CP CertifiedMinimal(int n,Rational k,Rational c)
 {if(k==0)return n==0?CP.Variable:CP.Variable*CP.Variable*CP.Variable;var alpha=Factor(k*new Rational(15,13));var odd=new CP(k*k*(c*c-14)*new Rational(1,26),k*new Rational(-6,13),1);if(n==0)return alpha*odd;if(c!=0)throw new InvalidOperationException("Uncertified general-c minimal requested");return alpha*alpha*Factor(k*new Rational(-7,13))*Factor(k*new Rational(-7,13))*Factor(k)*CP.Variable*Factor(k*new Rational(12,13))*Factor(k*new Rational(85,78))*Factor(k*new Rational(5,13))*Factor(k*new Rational(95,78));}
 public static int[] ExpectedRanks(int n,Rational k,Rational c)=>n==0?(k==0?[0,0,0]:[6,6,6]):k!=0?[11,11,11]:c==0?[4,2,0]:[6,3,0];
 public static object PText(CP[][] a)=>a.Select(row=>row.Select(v=>v.Text()).ToArray()).ToArray();
 public static CP[][] FactorMatrixExpected(int h,int n)
 {
  int size=n==0?6:12;var m=PZero(size);CP c=CP.Variable;int minus=n==0?3:9;
  foreach(int start in new[]{0,minus})m[start][start]=(CP)new Rational(15,13);
  m[1][1]=1;m[2][2]=(CP)new Rational(-7,13);m[1][2]=c.Scale(new Rational(-3,13));m[2][1]=c.Scale(new Rational(1,6));
  m[minus+1][minus+1]=1;m[minus+2][minus+2]=(CP)new Rational(-7,13);m[minus+1][minus+2]=c;m[minus+2][minus+1]=c.Scale(new Rational(-1,26));
  if(n==0)return m;
  m[3][3]=(CP)new Rational(-1,13);m[3][4]=(CP)new Rational(h,13);m[3][5]=c.Scale(new Rational(h,13));
  m[4][3]=-h;m[4][4]=1;m[4][5]=c;m[5][3]=c.Scale(new Rational(-h,13));m[5][4]=c.Scale(new Rational(1,13));m[5][5]=(CP)new Rational(5,13);
  m[6][6]=1;m[7][7]=(CP)new Rational(85,78);m[7][8]=c.Scale(new Rational(-7*h,78));m[8][7]=c.Scale(new Rational(7*h,78));m[8][8]=(CP)new Rational(95,78);return m;
 }
}
